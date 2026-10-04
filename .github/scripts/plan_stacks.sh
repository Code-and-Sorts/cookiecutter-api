#!/usr/bin/env bash
set -euo pipefail

for stack in $(atmos list stacks); do
  echo "::group::$stack"
  atmos terraform generate varfile api -s "$stack" -f "$RUNNER_TEMP/$stack.tfvars.json"
  terraform -chdir=components/terraform/api test -test-directory=tests/stacks -var-file="$RUNNER_TEMP/$stack.tfvars.json"
  echo "::endgroup::"
done
