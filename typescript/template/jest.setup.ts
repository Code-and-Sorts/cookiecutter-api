{% if cloud_service == 'Azure Function App' -%}
process.env.COSMOS_DB_URL ??= 'https://cosmos-mock.documents.azure.com:443/';
process.env.COSMOS_DB_KEY ??= 'mock-cosmos-key';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
process.env.GCP_PROJECT_ID ??= 'mock-gcp-project';
process.env.FIRESTORE_DATABASE ??= '(default)';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
process.env.AWS_REGION ??= 'us-east-1';
{%- endif %}
