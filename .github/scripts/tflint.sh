#!/usr/bin/env bash
set -euo pipefail

for attempt in 1 2 3; do
  tflint --init && break
  [ "$attempt" -lt 3 ] || exit 1
  sleep 10
done
tflint --format compact
