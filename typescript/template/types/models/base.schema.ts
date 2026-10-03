{%- from 'typescript/_model.jinja' import entity_schema, field_imports, read_value, request_schema, response_schema -%}
{%- set user_fields = base_fields | rejectattr("system") | list -%}
{%- set body %}
// Server-managed: set by the repository, never accepted in a request body.
export const SYSTEM_FIELDS = [{% for f in base_fields | selectattr("system") %}'{{ f.name }}'{{ ", " if not loop.last }}{% endfor %}] as const;

export type SystemField = (typeof SYSTEM_FIELDS)[number];

// A stored record: the built-in fields and the fields base_model adds.
export const BaseEntitySchema = z.object({
    id: z.string(),
    isDeleted: z.boolean(),
    createdTimestamp: z.string(),
    updatedTimestamp: z.string(),
    createdBy: z.string().optional(),
    updatedBy: z.string().optional(),
{%- for f in user_fields %}
    {{ f.name }}: {{ entity_schema(f) }},
{%- endfor %}
});

export type BaseEntity = z.infer<typeof BaseEntitySchema>;

// Request bodies are strict: unknown and server-managed fields are rejected, and values are never converted.
export const BaseCreateRequestSchema = z.strictObject({
{%- for f in user_fields | selectattr("in_create") %}
    {{ f.name }}: {{ request_schema(f, "create") }},
{%- endfor %}
});

export const BaseReplaceRequestSchema = z.strictObject({
{%- for f in user_fields | selectattr("in_replace") %}
    {{ f.name }}: {{ request_schema(f, "replace") }},
{%- endfor %}
});

// Fields an update leaves out stay unchanged, so it applies no defaults.
export const BaseUpdateRequestSchema = z.strictObject({
{%- for f in user_fields | selectattr("in_update") %}
    {{ f.name }}: {{ request_schema(f, "update") }},
{%- endfor %}
});

export const BaseResponseSchema = z.object({
    id: z.string(),
{%- for f in user_fields | rejectattr("hidden") %}
    {{ f.name }}: {{ response_schema(f) }},
{%- endfor %}
});

export type BaseCreateRequest = z.infer<typeof BaseCreateRequestSchema>;

export type BaseReplaceRequest = z.infer<typeof BaseReplaceRequestSchema>;

export type BaseUpdateRequest = z.infer<typeof BaseUpdateRequestSchema>;

export type BaseResponse = z.infer<typeof BaseResponseSchema>;

export const toBaseResponse = (record: BaseEntity): BaseResponse => ({
    id: record.id,
{%- for f in user_fields | rejectattr("hidden") %}
    {{ f.name }}: {{ read_value(f) }},
{%- endfor %}
});

// The fields a client may set, in a resource's record schema.
export const clientFieldsOf = (schema: z.ZodObject): string[] =>
    Object.keys(schema.shape).filter((field) => !(SYSTEM_FIELDS as readonly string[]).includes(field));
{%- endset -%}
import { z } from 'zod';
{{ field_imports(body) }}{{ body }}
