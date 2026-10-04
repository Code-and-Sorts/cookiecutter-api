import { z } from 'zod';
import { BaseSchema, responseMapper, responseSchema } from './base.schema';

export const CatSchema = z.object({
    name: z.string().min(1),
});

// strict() rejects id and the system fields, so a body can never overwrite them.
export const CatRequestSchema = CatSchema.strict();

export const CatUpdateSchema = CatSchema.partial().strict();

export const CatResponseSchema = responseSchema(CatSchema);

export const CatEntitySchema = z.object({
    ...CatSchema.shape,
    ...BaseSchema.shape,
});

export type CatRecord = z.infer<typeof CatEntitySchema>;

export type Cat = z.infer<typeof CatSchema>;

export type CatResponse = z.infer<typeof CatResponseSchema>;

export type CatUpdate = z.infer<typeof CatUpdateSchema>;

export const toCatResponse = responseMapper(CatResponseSchema);
