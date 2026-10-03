import { z } from 'zod';
import {
    BaseCreateRequestSchema,
    BaseReplaceRequestSchema,
    BaseEntity,
    BaseResponse,
    toBaseResponse,
} from './base.schema';

export type DogRecord = BaseEntity & {
    name?: string | null;
};

export const DogCreateRequestSchema = BaseCreateRequestSchema.extend({
    name: z.string().min(1),
});

export type DogCreateRequest = z.infer<typeof DogCreateRequestSchema>;

export const DogReplaceRequestSchema = BaseReplaceRequestSchema.extend({
    name: z.string().min(1),
});

export type DogReplaceRequest = z.infer<typeof DogReplaceRequestSchema>;

export type DogResponse = BaseResponse & {
    name: string | null;
};

export const toDogResponse = (record: DogRecord): DogResponse => ({
    ...toBaseResponse(record),
    name: record.name ?? null,
});
