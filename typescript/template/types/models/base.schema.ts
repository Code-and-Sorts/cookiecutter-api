import { randomUUID } from 'node:crypto';
import { z } from 'zod';

export const BaseIdentifier = z.object({
    id: z.string().default(() => randomUUID()),
});

export const BaseSchema = BaseIdentifier.extend({
    isDeleted: z.boolean().default(false),
    createdTimestamp: z.string().default(() => new Date().toISOString()),
    updatedTimestamp: z.string().default(() => new Date().toISOString()),
    createdBy: z.string().optional(),
    updatedBy: z.string().optional(),
});

export type BaseItemRecord = z.infer<typeof BaseSchema>;
