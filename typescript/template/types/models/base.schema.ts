import { z } from 'zod';

export const BaseIdentifier = z.object({
    id: z.string(),
});

export const BaseSchema = BaseIdentifier.extend({
    isDeleted: z.boolean(),
    createdTimestamp: z.string(),
    updatedTimestamp: z.string(),
    createdBy: z.string().optional(),
    updatedBy: z.string().optional(),
});

export type BaseItemRecord = z.infer<typeof BaseSchema>;

export const responseMapper =
    <R>(schema: z.ZodType<R>) =>
    (record: BaseItemRecord): R =>
        schema.parse(record);
