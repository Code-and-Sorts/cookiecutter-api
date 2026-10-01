{%- set containers = path_resources | unique(attribute='container') | list -%}
package main

import (
	"context"
	"errors"
	"fmt"
{%- if cloud_service == 'GCP Cloud Function' %}
	"io"
{%- endif %}
	"log/slog"
{%- if cloud_service == 'GCP Cloud Function' %}
	"net/http"
{%- endif %}
	"os"
	"slices"
{%- if cloud_service == 'Azure Function App' %}
	"strconv"
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
	"strings"
{%- endif %}
	"time"
{%- if cloud_service == 'Azure Function App' %}

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

	"{{project_endpoint}}/repositories"
	"{{project_endpoint}}/utils"
{%- elif cloud_service == 'AWS Lambda' %}

	"github.com/aws/aws-sdk-go-v2/aws"
	awsconfig "github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"

	"{{project_endpoint}}/utils"
{%- else %}

	"{{project_endpoint}}/utils"
{%- endif %}
)

const (
	timeout  = 2 * time.Minute
	maxDelay = 8 * time.Second
)

var errNotEmulator = errors.New("refusing to run outside the emulator")

func main() {
	slog.SetDefault(utils.NewLogger())
	ctx, cancel := context.WithTimeout(context.Background(), timeout)
	defer cancel()
	if err := run(ctx); err != nil {
		slog.Error("Emulator bootstrap failed", "error", err)
		os.Exit(1)
	}
}

func containerNames() []string {
	names := []string{
{%- for c in containers %}
{%- if cloud_service == 'Azure Function App' %}
		utils.Getenv("CosmosDbContainerName_{{ c.container_class }}", "{{ c.container }}"),
{%- elif cloud_service == 'GCP Cloud Function' %}
		utils.Getenv("FIRESTORE_COLLECTION_{{ c.env_key }}", "{{ c.container }}"),
{%- else %}
		utils.Getenv("DYNAMODB_TABLE_NAME_{{ c.env_key }}", "{{ c.container }}"),
{%- endif %}
{%- endfor %}
	}
	slices.Sort(names)
	return slices.Compact(names)
}

func retry(ctx context.Context, step func(context.Context) error) error {
	delay := time.Second
	for {
		err := step(ctx)
		if err == nil {
			return nil
		}
		slog.Info("Waiting for the emulator", "error", err, "retryIn", delay)
		select {
		case <-ctx.Done():
			return fmt.Errorf("the emulator was not ready within %s: %w", timeout, err)
		case <-time.After(delay):
		}
		delay = min(delay*2, maxDelay)
	}
}
{%- if cloud_service == 'Azure Function App' %}

func run(ctx context.Context) error {
	emulator, _ := strconv.ParseBool(os.Getenv("CosmosDbEmulator"))
	if !emulator {
		return fmt.Errorf("CosmosDbEmulator is not true: %w", errNotEmulator)
	}
	endpoint := os.Getenv("CosmosDbEndpoint")
	databaseName := os.Getenv("CosmosDbDatabaseName")
	cred, err := azcosmos.NewKeyCredential(os.Getenv("CosmosDbKey"))
	if err != nil {
		return err
	}
	client, err := azcosmos.NewClientWithKey(endpoint, cred, repositories.CosmosClientOptions(endpoint, emulator))
	if err != nil {
		return err
	}

	return retry(ctx, func(ctx context.Context) error {
		if _, err := client.CreateDatabase(ctx, azcosmos.DatabaseProperties{ID: databaseName}, nil); err != nil && !isConflict(err) {
			return err
		}
		database, err := client.NewDatabase(databaseName)
		if err != nil {
			return err
		}
		for _, name := range containerNames() {
			properties := azcosmos.ContainerProperties{
				ID:                     name,
				PartitionKeyDefinition: azcosmos.PartitionKeyDefinition{Paths: []string{"/id"}},
			}
			if _, err := database.CreateContainer(ctx, properties, nil); err != nil && !isConflict(err) {
				return err
			}
			slog.Info("Container is ready", "database", databaseName, "container", name)
		}
		return nil
	})
}

func isConflict(err error) bool {
	var respErr *azcore.ResponseError
	return errors.As(err, &respErr) && respErr.StatusCode == 409
}
{%- elif cloud_service == 'GCP Cloud Function' %}

func run(ctx context.Context) error {
	host := os.Getenv("FIRESTORE_EMULATOR_HOST")
	if host == "" {
		return fmt.Errorf("FIRESTORE_EMULATOR_HOST is not set: %w", errNotEmulator)
	}

	return retry(ctx, func(ctx context.Context) error {
		request, err := http.NewRequestWithContext(ctx, http.MethodGet, "http://"+host+"/", nil)
		if err != nil {
			return err
		}
		response, err := http.DefaultClient.Do(request)
		if err != nil {
			return err
		}
		defer response.Body.Close()
		body, err := io.ReadAll(response.Body)
		if err != nil {
			return err
		}
		if strings.TrimSpace(string(body)) != "Ok" {
			return fmt.Errorf("%s is not a Firestore emulator", host)
		}
		slog.Info("Firestore emulator is ready", "host", host, "project", os.Getenv("GCP_PROJECT_ID"), "collections", containerNames())
		return nil
	})
}
{%- else %}

func run(ctx context.Context) error {
	if os.Getenv("AWS_ENDPOINT_URL_DYNAMODB") == "" {
		return fmt.Errorf("AWS_ENDPOINT_URL_DYNAMODB is not set: %w", errNotEmulator)
	}
	cfg, err := awsconfig.LoadDefaultConfig(ctx)
	if err != nil {
		return err
	}
	client := dynamodb.NewFromConfig(cfg)

	return retry(ctx, func(ctx context.Context) error {
		// Listing first avoids an error response per existing table, which the SDK logs as a warning.
		existing := map[string]bool{}
		pages := dynamodb.NewListTablesPaginator(client, &dynamodb.ListTablesInput{})
		for pages.HasMorePages() {
			page, err := pages.NextPage(ctx)
			if err != nil {
				return err
			}
			for _, name := range page.TableNames {
				existing[name] = true
			}
		}
		for _, name := range containerNames() {
			if !existing[name] {
				if err := createTable(ctx, client, name); err != nil {
					return err
				}
			}
			waiter := dynamodb.NewTableExistsWaiter(client)
			if err := waiter.Wait(ctx, &dynamodb.DescribeTableInput{TableName: aws.String(name)}, 30*time.Second); err != nil {
				return err
			}
			slog.Info("Table is ready", "table", name)
		}
		return nil
	})
}

func createTable(ctx context.Context, client *dynamodb.Client, name string) error {
	_, err := client.CreateTable(ctx, &dynamodb.CreateTableInput{
		TableName: aws.String(name),
		AttributeDefinitions: []types.AttributeDefinition{
			{AttributeName: aws.String("id"), AttributeType: types.ScalarAttributeTypeS},
		},
		KeySchema: []types.KeySchemaElement{
			{AttributeName: aws.String("id"), KeyType: types.KeyTypeHash},
		},
		BillingMode: types.BillingModePayPerRequest,
	})
	var inUse *types.ResourceInUseException
	if err != nil && !errors.As(err, &inUse) {
		return err
	}
	return nil
}
{%- endif %}
