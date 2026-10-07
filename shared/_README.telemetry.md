{#- "Logs and OpenTelemetry" section of every README. The including README sets `telemetry`
    (how, trace, console, app_insights, flush, and optionally resource, otlp and azure). -#}
## Logs and OpenTelemetry

{{ telemetry.how }} A record carries the trace and span ids of {{ telemetry.trace }}, and
{%- if telemetry.resource %} {{ telemetry.resource }}
{%- else %} the resource attributes
{% for key, value in telemetry_resource | dictsort %}`{{ key }}={{ value }}`{{ ', ' if not loop.last }}{% endfor %};
`OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES` override or add to them{% endif %}. Unexpected errors are logged with
the exception and its stack trace, and the client only ever sees the generic 500 message.

The standard environment variables pick where the records go:

| Setting | Records go to |
| --- | --- |
{%- if cloud_service == 'Azure Function App' %}
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | {{ telemetry.app_insights }} |
{%- endif %}
| `OTEL_EXPORTER_OTLP_ENDPOINT` or `OTEL_EXPORTER_OTLP_LOGS_ENDPOINT` | {{ telemetry.otlp or 'An OTLP endpoint over HTTP/protobuf; `OTEL_EXPORTER_OTLP_HEADERS` and the other standard OTLP exporter settings apply' }} |
| Neither | {{ telemetry.console }} |

{% if cloud_service == 'Azure Function App' -%}
`host.json` sets `"telemetryMode": "OpenTelemetry"`, so the Functions host exports its own logs and traces with
OpenTelemetry to the same places. {{ telemetry.azure }}
{%- if include_infrastructure %} The infrastructure gives every hosting the Application Insights connection string and
sets `OTEL_SERVICE_NAME` and `OTEL_RESOURCE_ATTRIBUTES` (stage, region and platform).{% endif %}
{%- elif cloud_service == 'GCP Cloud Function' -%}
Cloud Logging reads the console lines as they are. To send OTLP instead, point `OTEL_EXPORTER_OTLP_ENDPOINT` at an
OpenTelemetry Collector or at Google's OTLP endpoint (`https://telemetry.googleapis.com`) with the credentials it needs.
{%- else -%}
CloudWatch Logs reads the console lines as they are. To send OTLP instead, add the
[AWS Distro for OpenTelemetry](https://aws-otel.github.io/docs/getting-started/lambda) collector layer to the function
and set `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318`; {{ telemetry.flush }}, because Lambda freezes the process
between invocations.
{%- endif %}
