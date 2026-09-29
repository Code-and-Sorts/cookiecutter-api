// Command main runs the Cloud Run function locally with the Functions
// Framework. Set FUNCTION_TARGET=api to serve it at every path.
package main

import (
	"log/slog"
	"os"

	"github.com/GoogleCloudPlatform/functions-framework-go/funcframework"

	// Registers the "api" function.
	_ "{{project_endpoint}}"
)

func main() {
	port := "8080"
	if envPort := os.Getenv("PORT"); envPort != "" {
		port = envPort
	}

	slog.Info("Listening for requests", "port", port)
	if err := funcframework.Start(port); err != nil {
		slog.Error("Server stopped", "error", err)
		os.Exit(1)
	}
}
