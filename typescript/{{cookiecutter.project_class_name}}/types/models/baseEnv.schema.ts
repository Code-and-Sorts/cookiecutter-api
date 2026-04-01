import { z } from 'zod';
{% if cookiecutter.cloud_service == 'Azure Function App' %}
export const baseEnvSchema = z.object({
    COSMOS_DB_URL: z.string().url(),
    COSMOS_DB_KEY: z.string().min(1),
});
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
export const baseEnvSchema = z.object({
    GCP_PROJECT_ID: z.string().min(1),
    FIRESTORE_DATABASE: z.string().default('(default)'),
    FIRESTORE_COLLECTION: z.string().default('{{cookiecutter.project_endpoint}}'),
});
{%- endif %}
