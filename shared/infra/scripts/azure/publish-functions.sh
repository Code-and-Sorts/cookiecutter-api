#!/usr/bin/env bash
set -euo pipefail

app="$1"
worker_runtime="$2"

if [ ! -f local.settings.json ]; then
  printf '{"IsEncrypted": false, "Values": {"FUNCTIONS_WORKER_RUNTIME": "%s"}}\n' "$worker_runtime" > local.settings.json
fi
func azure functionapp publish "$app"
