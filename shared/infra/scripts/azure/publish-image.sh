#!/usr/bin/env bash
set -euo pipefail

registry="$1"
app="$2"
resource_group="$3"
image="$registry/api:$4"

az acr login --name "${registry%%.*}"
docker build --tag "$image" .
docker push "$image"
az containerapp update --name "$app" --resource-group "$resource_group" --image "$image"
