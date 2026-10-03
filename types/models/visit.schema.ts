import { z } from 'zod';
import { dateSchema, dateTimeSchema } from './fields.schema';
import {
    BaseCreateRequestSchema,
    BaseUpdateRequestSchema,
    BaseEntity,
    BaseResponse,
    toBaseResponse,
} from './base.schema';

export type VisitRecord = BaseEntity & {
    reason?: string | null;
    visitedOn?: string | null;
    cost?: number | null;
    paid?: boolean | null;
    checkedAt?: string[] | null;
};

export const VisitCreateRequestSchema = BaseCreateRequestSchema.extend({
    reason: z.string(),
    visitedOn: dateSchema,
    cost: z.number().gte(0).optional(),
});

export type VisitCreateRequest = z.infer<typeof VisitCreateRequestSchema>;

// Fields an update leaves out stay unchanged, so it applies no defaults.
export const VisitUpdateRequestSchema = BaseUpdateRequestSchema.extend({
    visitedOn: dateSchema.optional(),
    cost: z.number().gte(0).optional(),
    paid: z.boolean().optional(),
    checkedAt: z.array(dateTimeSchema).optional(),
});

export type VisitUpdateRequest = z.infer<typeof VisitUpdateRequestSchema>;

export type VisitResponse = BaseResponse & {
    reason: string | null;
    visitedOn: string | null;
    cost: number | null;
    paid: boolean | null;
    checkedAt: string[] | null;
};

export const toVisitResponse = (record: VisitRecord): VisitResponse => ({
    ...toBaseResponse(record),
    reason: record.reason ?? null,
    visitedOn: record.visitedOn ?? null,
    cost: record.cost ?? null,
    paid: record.paid ?? false,
    checkedAt: record.checkedAt ?? null,
});
