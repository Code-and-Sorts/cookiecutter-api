#!/usr/bin/env bash
# Usage: start-local-api.sh install|emulator|host
# Runs in the generated project's directory with LANGUAGE, CLOUD and READY_TIMEOUT set.
set -euo pipefail

# TypeScript projects use their package scripts; the other languages share the Make targets.
install_dependencies() {
  if [ "$LANGUAGE" = typescript ]; then
    yarn install --no-immutable
  else
    make install
  fi
}

start_emulator() {
  if [ "$LANGUAGE" = typescript ]; then
    yarn emulator:up
    yarn emulator:seed
  else
    make emulator-up emulator-seed
  fi
}

base_url() {
  case "$CLOUD" in
    "Azure Function App") echo "http://localhost:7071/api" ;;
    "GCP Cloud Function") echo "http://localhost:8080" ;;
    "AWS Lambda") echo "http://localhost:3000" ;;
    *) echo "::error::Unknown cloud service: $CLOUD" >&2; return 1 ;;
  esac
}

fail_with_host_log() {
  echo "::error::$1"
  cat host.log
  exit 1
}

start_host() {
  local url pid deadline
  url=$(base_url)
  if [ "$LANGUAGE" = typescript ]; then
    nohup yarn start:emulator > host.log 2>&1 &
  else
    nohup make run-emulator > host.log 2>&1 &
  fi
  pid=$!

  # Any HTTP status means the host is listening; the suite then waits for app code itself.
  deadline=$((SECONDS + READY_TIMEOUT))
  until curl -s -o /dev/null --max-time 5 "$url/"; do
    kill -0 "$pid" 2> /dev/null || fail_with_host_log "The host exited before it started listening."
    [ "$SECONDS" -lt "$deadline" ] || fail_with_host_log "The host did not listen on $url within ${READY_TIMEOUT}s."
    sleep 3
  done
  echo "Host is listening on $url"
  echo "base-url=$url" >> "$GITHUB_OUTPUT"
}

case "${1:-}" in
  install) install_dependencies ;;
  emulator) start_emulator ;;
  host) start_host ;;
  *) echo "Usage: $0 install|emulator|host" >&2; exit 2 ;;
esac
