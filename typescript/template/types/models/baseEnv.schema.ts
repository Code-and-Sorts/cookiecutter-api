import { z } from 'zod';
{% if cloud_service == 'Azure Function App' %}
export const baseEnvSchema = z.object({
    COSMOS_DB_URL: z.string().url(),
    COSMOS_DB_KEY: z.string().min(1),
});
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
export const baseEnvSchema = z.object({
    GCP_PROJECT_ID: z.string().min(1),
    FIRESTORE_DATABASE: z.string().default('(default)'),
    FIRESTORE_COLLECTION: z.string().default('{{project_endpoint}}'),
});
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
export const baseEnvSchema = z.object({
    AWS_REGION: z.string().default('us-east-1'),
    DYNAMODB_TABLE_NAME: z.string().min(1),
});
{%- endif %}
