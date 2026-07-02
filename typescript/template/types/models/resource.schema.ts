import { z } from 'zod';
import { BaseIdentifier, BaseSchema } from './base.schema';
{% for resource in resources %}
export const {{ resource.name }}Schema = z.object({
    name: z.string().optional(),
});

export const {{ resource.name }}UpdateSchema = z.object({
    id: z.string(),
}).merge({{ resource.name }}Schema).strict();

export const {{ resource.name }}RequestSchema = {{ resource.name }}Schema.strict();

export const {{ resource.name }}ResponseSchema = z.object({
    ...BaseIdentifier.shape,
    ...{{ resource.name }}Schema.shape,
});

export const {{ resource.name }}EntitySchema = z.object({
    ...{{ resource.name }}Schema.shape,
    ...BaseSchema.shape,
});

export type {{ resource.name }}Record = z.infer<typeof {{ resource.name }}EntitySchema>;

export type {{ resource.name }} = z.infer<typeof {{ resource.name }}Schema>;

export type {{ resource.name }}Response = z.infer<typeof {{ resource.name }}ResponseSchema>;

export type {{ resource.name }}Update = z.infer<typeof {{ resource.name }}UpdateSchema>;
{% endfor %}