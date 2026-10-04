import { z } from 'zod';

export const BaseIdentifier = z.object({
    id: z.string(),
});

const AuditFields = z.object({
    createdTimestamp: z.string(),
    createdBy: z.string().optional(),
    updatedTimestamp: z.string(),
    updatedBy: z.string().optional(),
});

export const BaseSchema = BaseIdentifier.extend({
    isDeleted: z.boolean(),
    ...AuditFields.shape,
});

export type BaseItemRecord = z.infer<typeof BaseSchema>;

// z.object strips unknown keys, so isDeleted and database metadata never reach a response.
export const responseSchema = <S extends z.ZodRawShape>(fields: z.ZodObject<S>) =>
    z.object({ ...BaseIdentifier.shape, ...fields.shape, ...AuditFields.shape });

export const responseMapper =
    <R>(schema: z.ZodType<R>) =>
    (record: BaseItemRecord): R =>
        schema.parse(record);
