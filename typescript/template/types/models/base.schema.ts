{%- from 'shared/_fields.jinja' import REQUEST_KINDS -%}
{%- from 'typescript/_model.jinja' import entity_schema, field_imports, read_value, request_schema, response_schema -%}
{%- set body %}
// Server-managed: set by the repository, never accepted in a request body.
export const SYSTEM_FIELDS = [{% for f in base_fields | selectattr("system") %}'{{ f.name }}'{{ ", " if not loop.last }}{% endfor %}] as const;

export type SystemField = (typeof SYSTEM_FIELDS)[number];

export const BaseEntitySchema = z.object({
    id: z.string(),
    isDeleted: z.boolean(),
    createdTimestamp: z.string(),
    updatedTimestamp: z.string(),
    createdBy: z.string().optional(),
    updatedBy: z.string().optional(),
{%- for f in client_base_fields %}
    {{ f.name }}: {{ entity_schema(f) }},
{%- endfor %}
});

export type BaseEntity = z.infer<typeof BaseEntitySchema>;

// Request bodies are strict: unknown and server-managed fields are rejected, and values are never converted.
{%- for op, kind in REQUEST_KINDS %}
export const Base{{ kind }}RequestSchema = z.strictObject({
{%- for f in client_base_fields | selectattr("in_" ~ op) %}
    {{ f.name }}: {{ request_schema(f, op) }},
{%- endfor %}
});

export type Base{{ kind }}Request = z.infer<typeof Base{{ kind }}RequestSchema>;
{% endfor %}
export const BaseResponseSchema = z.object({
    id: z.string(),
{%- for f in client_base_fields | rejectattr("hidden") %}
    {{ f.name }}: {{ response_schema(f) }},
{%- endfor %}
});

export type BaseResponse = z.infer<typeof BaseResponseSchema>;

export const toBaseResponse = (record: BaseEntity): BaseResponse => ({
    id: record.id,
{%- for f in client_base_fields | rejectattr("hidden") %}
    {{ f.name }}: {{ read_value(f) }},
{%- endfor %}
});

export const clientFieldsOf = (schema: z.ZodObject): string[] =>
    Object.keys(schema.shape).filter((field) => !(SYSTEM_FIELDS as readonly string[]).includes(field));
{%- endset -%}
import { z } from 'zod';
{{ field_imports(body) }}{{ body }}
