import { z } from 'zod';
import { hasUniqueItems } from './fields.schema';

export type SystemField = 'id' | 'isDeleted' | 'createdTimestamp' | 'updatedTimestamp' | 'createdBy' | 'updatedBy';

export type BaseEntity = {
    id: string;
    isDeleted: boolean;
    createdTimestamp: string;
    updatedTimestamp: string;
    createdBy?: string;
    updatedBy?: string;
    /** Owning tenant */
    tenantId?: string | null;
    region?: 'eu' | 'us' | null;
    priority?: number | null;
    rank?: number | null;
    labels?: string[] | null;
};

export const BaseCreateRequestSchema = z.strictObject({
    tenantId: z.string().min(1).max(64).default("public"),
    region: z.enum(["eu", "us"]).default("eu"),
    priority: z.int().gte(0).nullable().optional(),
    rank: z.number(),
    labels: z.array(z.string()).refine(hasUniqueItems, 'must not repeat an item').default(() => []),
});

export type BaseCreateRequest = z.infer<typeof BaseCreateRequestSchema>;

export const BaseReplaceRequestSchema = z.strictObject({
    tenantId: z.string().min(1).max(64).default("public"),
    priority: z.int().gte(0).nullable().optional(),
    rank: z.number(),
    labels: z.array(z.string()).refine(hasUniqueItems, 'must not repeat an item').default(() => []),
});

export type BaseReplaceRequest = z.infer<typeof BaseReplaceRequestSchema>;

// Fields an update leaves out stay unchanged, so it applies no defaults.
export const BaseUpdateRequestSchema = z.strictObject({
    tenantId: z.string().min(1).max(64).optional(),
    priority: z.int().gte(0).nullable().optional(),
    rank: z.number().optional(),
    labels: z.array(z.string()).refine(hasUniqueItems, 'must not repeat an item').optional(),
});

export type BaseUpdateRequest = z.infer<typeof BaseUpdateRequestSchema>;

export type BaseResponse = {
    id: string;
    tenantId: string | null;
    region: 'eu' | 'us' | null;
    priority: number | null;
    rank: number | null;
    labels: string[] | null;
};

export const toBaseResponse = (record: BaseEntity): BaseResponse => ({
    id: record.id,
    tenantId: record.tenantId ?? "public",
    region: record.region ?? "eu",
    priority: record.priority ?? null,
    rank: record.rank ?? null,
    labels: record.labels ?? [],
});
