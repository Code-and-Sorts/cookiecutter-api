import { z } from 'zod';
import { EMAIL_PATTERN, URI_PATTERN, dateSchema, dateTimeSchema, hasUniqueItems, uuidSchema } from './fields.schema';
import { newId, nowIso, todayIso } from '../../utils/clock.util';
import {
    BaseCreateRequestSchema,
    BaseReplaceRequestSchema,
    BaseUpdateRequestSchema,
    BaseEntity,
    BaseResponse,
    toBaseResponse,
} from './base.schema';

export type CatRecord = BaseEntity & {
    name?: string | null;
    breed?: 'siamese' | 'persian' | 'tabby' | null;
    ageYears?: number | null;
    weightKg?: number | null;
    indoor?: boolean | null;
    birthDate?: string | null;
    microchipId?: string | null;
    ownerEmail?: string | null;
    website?: string | null;
    tagCode?: string | null;
    tags?: string[] | null;
    scores?: number[] | null;
    adoptedAt?: string | null;
    lastVisit?: string | null;
    notes?: string | null;
};

export const CatCreateRequestSchema = BaseCreateRequestSchema.extend({
    name: z.string().min(1).max(100),
    breed: z.enum(["siamese", "persian", "tabby"]).default("tabby"),
    ageYears: z.int().gte(0).lte(40).default(0),
    weightKg: z.number().gt(0).lt(100).nullable().default(null),
    indoor: z.boolean().default(true),
    birthDate: dateSchema.default(todayIso),
    microchipId: uuidSchema.default(newId),
    ownerEmail: z.string().regex(EMAIL_PATTERN, 'must be an email address').default("unknown@example.com"),
    website: z.string().regex(URI_PATTERN, 'must be an absolute URI').nullable().optional(),
    tagCode: z.string().regex(new RegExp("^[A-Z]{3}-[0-9]{3}$", 'u')).optional(),
    tags: z.array(z.string()).max(3).refine(hasUniqueItems, 'must not repeat an item').default(() => []),
    scores: z.array(z.int()).min(1).nullable().optional(),
    adoptedAt: dateTimeSchema.nullable().default(nowIso),
    lastVisit: dateTimeSchema.default("2026-01-01T00:00:00.000Z"),
    notes: z.string().default("$none"),
});

export type CatCreateRequest = z.infer<typeof CatCreateRequestSchema>;

export const CatReplaceRequestSchema = BaseReplaceRequestSchema.extend({
    name: z.string().min(1).max(100),
    breed: z.enum(["siamese", "persian", "tabby"]).default("tabby"),
    ageYears: z.int().gte(0).lte(40).default(0),
    weightKg: z.number().gt(0).lt(100).nullable().default(null),
    indoor: z.boolean().default(true),
    birthDate: dateSchema.default(todayIso),
    ownerEmail: z.string().regex(EMAIL_PATTERN, 'must be an email address').default("unknown@example.com"),
    website: z.string().regex(URI_PATTERN, 'must be an absolute URI').nullable().optional(),
    tagCode: z.string().regex(new RegExp("^[A-Z]{3}-[0-9]{3}$", 'u')).optional(),
    tags: z.array(z.string()).max(3).refine(hasUniqueItems, 'must not repeat an item').default(() => []),
    scores: z.array(z.int()).min(1).nullable().optional(),
    adoptedAt: dateTimeSchema.nullable().default(nowIso),
    lastVisit: dateTimeSchema.default("2026-01-01T00:00:00.000Z"),
    notes: z.string().default("$none"),
});

export type CatReplaceRequest = z.infer<typeof CatReplaceRequestSchema>;

// Fields an update leaves out stay unchanged, so it applies no defaults.
export const CatUpdateRequestSchema = BaseUpdateRequestSchema.extend({
    name: z.string().min(1).max(100).optional(),
    ageYears: z.int().gte(0).lte(40).optional(),
    weightKg: z.number().gt(0).lt(100).nullable().optional(),
    indoor: z.boolean().optional(),
    ownerEmail: z.string().regex(EMAIL_PATTERN, 'must be an email address').optional(),
    website: z.string().regex(URI_PATTERN, 'must be an absolute URI').nullable().optional(),
    tags: z.array(z.string()).max(3).refine(hasUniqueItems, 'must not repeat an item').optional(),
    adoptedAt: dateTimeSchema.nullable().optional(),
    notes: z.string().optional(),
});

export type CatUpdateRequest = z.infer<typeof CatUpdateRequestSchema>;

export type CatResponse = BaseResponse & {
    name: string | null;
    breed: 'siamese' | 'persian' | 'tabby' | null;
    ageYears: number | null;
    weightKg: number | null;
    indoor: boolean | null;
    birthDate: string | null;
    microchipId: string | null;
    ownerEmail: string | null;
    website: string | null;
    tagCode: string | null;
    tags: string[] | null;
    scores: number[] | null;
    adoptedAt: string | null;
    lastVisit: string | null;
};

export const toCatResponse = (record: CatRecord): CatResponse => ({
    ...toBaseResponse(record),
    name: record.name ?? null,
    breed: record.breed ?? "tabby",
    ageYears: record.ageYears ?? 0,
    weightKg: record.weightKg ?? null,
    indoor: record.indoor ?? true,
    birthDate: record.birthDate ?? null,
    microchipId: record.microchipId ?? null,
    ownerEmail: record.ownerEmail ?? "unknown@example.com",
    website: record.website ?? null,
    tagCode: record.tagCode ?? null,
    tags: record.tags ?? [],
    scores: record.scores ?? null,
    adoptedAt: record.adoptedAt ?? null,
    lastVisit: record.lastVisit ?? "2026-01-01T00:00:00.000Z",
});
