import { z } from 'zod';
import { BaseSchema, responseMapper, responseSchema } from './base.schema';

export const KittenClawsSchema = z.object({
    name: z.string().min(1),
});

// strict() rejects id and the system fields, so a body can never overwrite them.
export const KittenClawsRequestSchema = KittenClawsSchema.strict();

export const KittenClawsUpdateSchema = KittenClawsSchema.partial().strict();

export const KittenClawsResponseSchema = responseSchema(KittenClawsSchema);

export const KittenClawsEntitySchema = z.object({
    ...KittenClawsSchema.shape,
    ...BaseSchema.shape,
});

export type KittenClawsRecord = z.infer<typeof KittenClawsEntitySchema>;

export type KittenClaws = z.infer<typeof KittenClawsSchema>;

export type KittenClawsResponse = z.infer<typeof KittenClawsResponseSchema>;

export type KittenClawsUpdate = z.infer<typeof KittenClawsUpdateSchema>;

export const toKittenClawsResponse = responseMapper(KittenClawsResponseSchema);
