import { describe, it, expect } from '@jest/globals';
import { z } from 'zod';
import { responseMapper, responseSchema } from '@models';

describe('responseSchema', () => {
    const toResponse = responseMapper(responseSchema(z.object({ name: z.string() })));
    const response = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockName',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockCreator',
        updatedTimestamp: '2024-03-25T00:00:00.000Z',
        updatedBy: 'mockUser',
    };
    const { createdBy, updatedBy, ...anonymous } = response;

    it('should answer with the item and audit fields in order and drop stored-only fields', () => {
        const stored = { ...response, isDeleted: false, _etag: 'mockEtag', _rid: 'mockRid' };
        const actual = toResponse(stored);
        expect(actual).toStrictEqual(response);
        expect(Object.keys(actual)).toEqual(['id', 'name', 'createdTimestamp', 'createdBy', 'updatedTimestamp', 'updatedBy']);
    });

    it('should leave out createdBy and updatedBy when the record has none', () => {
        const actual = toResponse({ ...anonymous, isDeleted: false });
        expect(actual).toStrictEqual(anonymous);
        expect(actual).not.toHaveProperty('createdBy');
        expect(actual).not.toHaveProperty('updatedBy');
    });
});
