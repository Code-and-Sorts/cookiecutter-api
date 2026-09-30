import { randomUUID } from 'node:crypto';
import { z } from 'zod';

export const BaseIdentifier = z.object({
    id: z.string().default(() => randomUUID()),
});

export const BaseSchema = BaseIdentifier.extend({
    isDeleted: z.boolean().default(false),
    // No per-field defaults: the service sets both from one clock reading so a new record's
    // createdTimestamp and updatedTimestamp are always identical.
    createdTimestamp: z.string(),
    updatedTimestamp: z.string(),
    createdBy: z.string().optional(),
    updatedBy: z.string().optional(),
});

export type BaseItemRecord = z.infer<typeof BaseSchema>;
