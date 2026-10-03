import { z } from 'zod';

export type SystemField = 'id' | 'isDeleted' | 'createdTimestamp' | 'updatedTimestamp' | 'createdBy' | 'updatedBy';

export type BaseEntity = {
    id: string;
    isDeleted: boolean;
    createdTimestamp: string;
    updatedTimestamp: string;
    createdBy?: string;
    updatedBy?: string;
};

export const BaseCreateRequestSchema = z.strictObject({
});

export type BaseCreateRequest = z.infer<typeof BaseCreateRequestSchema>;

export const BaseReplaceRequestSchema = z.strictObject({
});

export type BaseReplaceRequest = z.infer<typeof BaseReplaceRequestSchema>;

// Fields an update leaves out stay unchanged, so it applies no defaults.
export const BaseUpdateRequestSchema = z.strictObject({
});

export type BaseUpdateRequest = z.infer<typeof BaseUpdateRequestSchema>;

export type BaseResponse = {
    id: string;
};

export const toBaseResponse = (record: BaseEntity): BaseResponse => ({
    id: record.id,
});
