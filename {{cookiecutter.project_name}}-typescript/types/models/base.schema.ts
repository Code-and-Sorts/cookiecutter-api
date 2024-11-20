import { z } from 'zod';
const { v4: uuidv4 } = require('uuid');

export const BaseIdentifier = z.object({
    id: z.string().default(() => uuidv4()),
});

export const BaseSchema = BaseIdentifier.merge(
    z.object({
        isDeleted: z.boolean().default(false),
        createdTimestamp: z.string().default(new Date().toISOString()),
        updatedTimestamp: z.string().default(new Date().toISOString()),
        createdBy: z.string().optional(),
        updatedBy: z.string().optional(),
    }),
);

export type BaseItemRecord = z.infer<typeof BaseSchema>;
