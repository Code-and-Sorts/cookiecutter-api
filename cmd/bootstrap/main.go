package main

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"os"
	"slices"
	"time"

	"github.com/aws/aws-sdk-go-v2/aws"
	awsconfig "github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb"
	"github.com/aws/aws-sdk-go-v2/service/dynamodb/types"

	"kittenclaws/utils"
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
		utils.Getenv("DYNAMODB_TABLE_NAME_ANIMALS", "animals"),
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
