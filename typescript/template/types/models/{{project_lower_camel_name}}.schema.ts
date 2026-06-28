import { z } from 'zod';
import { BaseIdentifier, BaseSchema } from './base.schema';

export const {{project_class_name}}Schema = z.object({
    name: z.string().optional(),
});

export const {{project_class_name}}UpdateSchema = z.object({
    id: z.string(),
}).merge({{project_class_name}}Schema).strict();

export const {{project_class_name}}RequestSchema = {{project_class_name}}Schema.strict();

export const {{project_class_name}}ResponseSchema = z.object({
    ...BaseIdentifier.shape,
    ...{{project_class_name}}Schema.shape,
});

export const {{project_class_name}}EntitySchema = z.object({
    ...{{project_class_name}}Schema.shape,
    ...BaseSchema.shape,
});

export type {{project_class_name}}ItemRecord = z.infer<typeof {{project_class_name}}EntitySchema>;

export type {{project_class_name}} = z.infer<typeof {{project_class_name}}Schema>;

export type {{project_class_name}}Response = z.infer<typeof {{project_class_name}}ResponseSchema>;

export type {{project_class_name}}Update = z.infer<typeof {{project_class_name}}UpdateSchema>;
