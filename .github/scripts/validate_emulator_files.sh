#!/usr/bin/env bash
set -euo pipefail

project="$1"
docker compose --project-directory "$project" config --quiet
if [ -f "$project/env.emulator.json" ]; then
  python -m json.tool "$project/env.emulator.json" > /dev/null
fi
