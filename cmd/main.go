// Command main runs the function locally; set FUNCTION_TARGET=api to serve it at every path.
package main

import (
	"log/slog"
	"os"

	"github.com/GoogleCloudPlatform/functions-framework-go/funcframework"

	// Registers the "api" function.
	_ "kittenclaws"
	"kittenclaws/utils"
)

func main() {
	port := utils.Getenv("PORT", "8080")

	slog.Info("Listening for requests", "port", port)
	if err := funcframework.Start(port); err != nil {
		slog.Error("Server stopped", "error", err)
		os.Exit(1)
	}
}
