package main

import (
	"context"
	"errors"
	"fmt"
	"io"
	"log/slog"
	"net/http"
	"os"
	"slices"
	"strings"
	"time"

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
		utils.Getenv("FIRESTORE_COLLECTION_ANIMALS", "animals"),
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
