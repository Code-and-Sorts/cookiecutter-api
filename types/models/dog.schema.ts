import { z } from 'zod';
import { BaseSchema, responseMapper, responseSchema } from './base.schema';

export const DogSchema = z.object({
    name: z.string().min(1),
});

// strict() rejects id and the system fields, so a body can never overwrite them.
export const DogRequestSchema = DogSchema.strict();

export const DogUpdateSchema = DogSchema.partial().strict();

export const DogResponseSchema = responseSchema(DogSchema);

export const DogEntitySchema = z.object({
    ...DogSchema.shape,
    ...BaseSchema.shape,
});

export type DogRecord = z.infer<typeof DogEntitySchema>;

export type Dog = z.infer<typeof DogSchema>;

export type DogResponse = z.infer<typeof DogResponseSchema>;

export type DogUpdate = z.infer<typeof DogUpdateSchema>;

export const toDogResponse = responseMapper(DogResponseSchema);
