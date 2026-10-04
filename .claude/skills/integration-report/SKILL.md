---
name: integration-report
description: Run the integration tests for every language and cloud and build an HTML request log to validate the API by eye - each request's method, URL, headers and body, the response status, body and headers, and the stored document before and after every create, update, replace and delete. Use when the user asks for the integration request log, an HTML report or output of the integration test requests, or to see or validate what the generated APIs send, return and store.
---

# Integration request log

Builds one page from the integration suite's request recordings: a language x cloud grid picks a run, its requests
are grouped by test, and each expands to the request, the response and, for writes, the stored document before
(when the request names an id) and after, with changed fields highlighted.

The suite records with `pytest ... --record requests.jsonl` (`tests/integration/recorder.py`). The integration
workflow passes it on every job, uploads `requests-<language>-<cloud>-<fixture>` artifacts, and prints the file
between `=== REQUEST LOG BEGIN ===` and `=== REQUEST LOG END ===` in the job log.

## Steps

1. **Pick the scope.** Default: every language and cloud with the `single` fixture (the default answers, 113
   requests per run). Use `edge` only when the user asks for many resources; it is about six times larger. The
   workflow file comes from the ref you dispatch, so push the branch first.
2. **Run the workflow.** Dispatch `integration-tests.yaml` on the branch (GitHub MCP `actions_run_trigger`,
   `run_workflow`, inputs `language`, `cloud`, `fixture`). Find the run id, then wait for it to complete without
   polling in the foreground: a background shell loop on
   `https://api.github.com/repos/Code-and-Sorts/cookiecutter-api/actions/runs/<id>` works for this public repo.
   If a job fails, report it; the report still builds from the jobs that recorded.
3. **Fetch the logs.** Prefer the artifacts when the host can reach them (`gh run download <id> -p 'requests-*' -D
   <dir>`). In a cloud session the artifact store is usually blocked, so read each `<language> / <cloud> /
   <fixture>` job's log with GitHub MCP `get_job_logs` (`job_id`, `return_content: true`, `tail_lines: 3000`).
   Each result is too large to show and is saved to a file; pass those files to the extractor:

   ```bash
   python .claude/skills/integration-report/extract_job_logs.py <logs dir> <saved log file>...
   ```

4. **Build the page.**

   ```bash
   python .claude/skills/integration-report/build_report.py <logs dir> <scratchpad>/request-log.html \
     --fixture single --run-url https://github.com/Code-and-Sorts/cookiecutter-api/actions/runs/<id>
   ```

   It prints the request count and names any language/cloud without a log.
5. **Publish it.** Publish the HTML with the Artifact tool when the session has it (the same file path keeps the
   same link on a rebuild), otherwise hand over the file. Tell the user the run, the fixture, and how many requests
   each combination recorded.

## Running one combination locally

Start the project's emulator and host as `.github/actions/start-local-api/start-local-api.sh` does, then:

```bash
uv run --project tests/integration pytest tests/integration --project-dir <project> --base-url <url> \
  --record <logs dir>/requests-<language>-<cloud>-<fixture>/requests.jsonl
```

GCP works in a cloud session (the Firestore emulator only needs Docker); Azure needs Azure Functions Core Tools and
AWS needs `sam local`, which cloud sessions usually cannot download, so run those through the workflow.
