import { z } from 'zod';
{% if cloud_service == 'Azure Function App' %}
export const baseEnvSchema = z.object({
    COSMOS_DB_URL: z.string().url(),
    COSMOS_DB_KEY: z.string().min(1),
    COSMOS_DB_DATABASE_NAME: z.string().default('{{ project_endpoint }}s-sql-db'),
{%- for container in resources | map(attribute='container') | unique %}
    COSMOS_CONTAINER_{{ container | upper | replace('-', '_') }}: z.string().default('{{ container }}'),
{%- endfor %}
});
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
export const baseEnvSchema = z.object({
    GCP_PROJECT_ID: z.string().min(1),
    FIRESTORE_DATABASE: z.string().default('(default)'),
{%- for container in resources | map(attribute='container') | unique %}
    FIRESTORE_COLLECTION_{{ container | upper | replace('-', '_') }}: z.string().default('{{ container }}'),
{%- endfor %}
});
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
export const baseEnvSchema = z.object({
    AWS_REGION: z.string().default('us-east-1'),
{%- for container in resources | map(attribute='container') | unique %}
    DYNAMODB_TABLE_NAME_{{ container | upper | replace('-', '_') }}: z.string().min(1).default('{{ container }}'),
{%- endfor %}
});
{%- endif %}
