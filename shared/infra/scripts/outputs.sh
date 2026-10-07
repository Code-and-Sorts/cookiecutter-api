#!/usr/bin/env bash
set -euo pipefail

component="$(dirname "$0")/../components/terraform/api"
for name in "$@"; do
  value="$(terraform -chdir="$component" output -raw "$name")"
  echo "$name=$value" >> "$GITHUB_OUTPUT"
done
