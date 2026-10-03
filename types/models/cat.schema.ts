import { z } from 'zod';
import {
    BaseCreateRequestSchema,
    BaseUpdateRequestSchema,
    BaseEntity,
    BaseResponse,
    toBaseResponse,
} from './base.schema';

export type CatRecord = BaseEntity & {
    name?: string | null;
};

export const CatCreateRequestSchema = BaseCreateRequestSchema.extend({
    name: z.string().min(1),
});

export type CatCreateRequest = z.infer<typeof CatCreateRequestSchema>;

// Fields an update leaves out stay unchanged, so it applies no defaults.
export const CatUpdateRequestSchema = BaseUpdateRequestSchema.extend({
    name: z.string().min(1).optional(),
});

export type CatUpdateRequest = z.infer<typeof CatUpdateRequestSchema>;

export type CatResponse = BaseResponse & {
    name: string | null;
};

export const toCatResponse = (record: CatRecord): CatResponse => ({
    ...toBaseResponse(record),
    name: record.name ?? null,
});
