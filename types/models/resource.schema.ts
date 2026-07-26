import { z } from 'zod';
import { BaseIdentifier, BaseSchema } from './base.schema';

export const CatSchema = z.object({
    name: z.string().optional(),
});

export const CatUpdateSchema = z.object({
    id: z.string(),
}).merge(CatSchema).strict();

export const CatRequestSchema = CatSchema.strict();

export const CatResponseSchema = z.object({
    ...BaseIdentifier.shape,
    ...CatSchema.shape,
});

export const CatEntitySchema = z.object({
    ...CatSchema.shape,
    ...BaseSchema.shape,
});

export type CatRecord = z.infer<typeof CatEntitySchema>;

export type Cat = z.infer<typeof CatSchema>;

export type CatResponse = z.infer<typeof CatResponseSchema>;

export type CatUpdate = z.infer<typeof CatUpdateSchema>;

export const DogSchema = z.object({
    name: z.string().optional(),
});

export const DogUpdateSchema = z.object({
    id: z.string(),
}).merge(DogSchema).strict();

export const DogRequestSchema = DogSchema.strict();

export const DogResponseSchema = z.object({
    ...BaseIdentifier.shape,
    ...DogSchema.shape,
});

export const DogEntitySchema = z.object({
    ...DogSchema.shape,
    ...BaseSchema.shape,
});

export type DogRecord = z.infer<typeof DogEntitySchema>;

export type Dog = z.infer<typeof DogSchema>;

export type DogResponse = z.infer<typeof DogResponseSchema>;

export type DogUpdate = z.infer<typeof DogUpdateSchema>;
