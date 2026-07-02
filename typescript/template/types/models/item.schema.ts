import { z } from 'zod';
import { BaseIdentifier, BaseSchema } from './base.schema';

export const ItemSchema = z.object({
    name: z.string().optional(),
});

export const ItemUpdateSchema = z.object({
    id: z.string(),
}).merge(ItemSchema).strict();

export const ItemRequestSchema = ItemSchema.strict();

export const ItemResponseSchema = z.object({
    ...BaseIdentifier.shape,
    ...ItemSchema.shape,
});

export const ItemEntitySchema = z.object({
    ...ItemSchema.shape,
    ...BaseSchema.shape,
});

export type ItemRecord = z.infer<typeof ItemEntitySchema>;

export type Item = z.infer<typeof ItemSchema>;

export type ItemResponse = z.infer<typeof ItemResponseSchema>;

export type ItemUpdate = z.infer<typeof ItemUpdateSchema>;
