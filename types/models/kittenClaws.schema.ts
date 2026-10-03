import { z } from 'zod';
import {
    BaseCreateRequestSchema,
    BaseUpdateRequestSchema,
    BaseEntity,
    BaseResponse,
    toBaseResponse,
} from './base.schema';

export type KittenClawsRecord = BaseEntity & {
    name?: string | null;
};

export const KittenClawsCreateRequestSchema = BaseCreateRequestSchema.extend({
    name: z.string().min(1),
});

export type KittenClawsCreateRequest = z.infer<typeof KittenClawsCreateRequestSchema>;

// Fields an update leaves out stay unchanged, so it applies no defaults.
export const KittenClawsUpdateRequestSchema = BaseUpdateRequestSchema.extend({
    name: z.string().min(1).optional(),
});

export type KittenClawsUpdateRequest = z.infer<typeof KittenClawsUpdateRequestSchema>;

export type KittenClawsResponse = BaseResponse & {
    name: string | null;
};

export const toKittenClawsResponse = (record: KittenClawsRecord): KittenClawsResponse => ({
    ...toBaseResponse(record),
    name: record.name ?? null,
});
