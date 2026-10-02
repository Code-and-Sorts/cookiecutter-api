package main

import (
	"context"
	"errors"
	"fmt"
	"log/slog"
	"os"
	"slices"
	"strconv"
	"time"

	"github.com/Azure/azure-sdk-for-go/sdk/azcore"
	"github.com/Azure/azure-sdk-for-go/sdk/data/azcosmos"

	"kittenclaws/repositories"
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
		utils.Getenv("CosmosDbContainerName_Animals", "animals"),
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
