import { z } from 'zod';
import { BaseIdentifier, BaseSchema } from './base.schema';

export const KittenClawsSchema = z.object({
    name: z.string().optional(),
});

export const KittenClawsUpdateSchema = z.object({
    id: z.string(),
}).merge(KittenClawsSchema).strict();

export const KittenClawsRequestSchema = KittenClawsSchema.strict();

export const KittenClawsResponseSchema = z.object({
    ...BaseIdentifier.shape,
    ...KittenClawsSchema.shape,
});

export const KittenClawsEntitySchema = z.object({
    ...KittenClawsSchema.shape,
    ...BaseSchema.shape,
});

export type KittenClawsRecord = z.infer<typeof KittenClawsEntitySchema>;

export type KittenClaws = z.infer<typeof KittenClawsSchema>;

export type KittenClawsResponse = z.infer<typeof KittenClawsResponseSchema>;

export type KittenClawsUpdate = z.infer<typeof KittenClawsUpdateSchema>;
