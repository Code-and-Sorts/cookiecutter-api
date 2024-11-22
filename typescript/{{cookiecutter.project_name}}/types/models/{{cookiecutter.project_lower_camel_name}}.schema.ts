import { z } from 'zod';
import { BaseIdentifier, BaseSchema } from './base.schema';

export const {{cookiecutter.project_class_name}}Schema = z.object({
    name: z.string().optional(),
});

export const {{cookiecutter.project_class_name}}UpdateSchema = z.object({
    id: z.string(),
}).merge({{cookiecutter.project_class_name}}Schema).strict();

export const {{cookiecutter.project_class_name}}RequestSchema = {{cookiecutter.project_class_name}}Schema.strict();

export const {{cookiecutter.project_class_name}}ResponseSchema = z.object({
    ...BaseIdentifier.shape,
    ...{{cookiecutter.project_class_name}}Schema.shape,
});

export const {{cookiecutter.project_class_name}}EntitySchema = z.object({
    ...{{cookiecutter.project_class_name}}Schema.shape,
    ...BaseSchema.shape,
});

export type {{cookiecutter.project_class_name}}ItemRecord = z.infer<typeof {{cookiecutter.project_class_name}}EntitySchema>;

export type {{cookiecutter.project_class_name}} = z.infer<typeof {{cookiecutter.project_class_name}}Schema>;

export type {{cookiecutter.project_class_name}}Response = z.infer<typeof {{cookiecutter.project_class_name}}ResponseSchema>;

export type {{cookiecutter.project_class_name}}Update = z.infer<typeof {{cookiecutter.project_class_name}}UpdateSchema>;
