import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { CatService } from '@services';
import { CatRepository } from '@repositories';
import { NotFoundError } from '@errors';
import { MockFn } from '../../test/mocks';

describe('CatService', () => {
    const repository = {
        create: jest.fn<MockFn>(),
        get: jest.fn<MockFn>(),
        list: jest.fn<MockFn>(),
        update: jest.fn<MockFn>(),
        replace: jest.fn<MockFn>(),
        delete: jest.fn<MockFn>(),
    };
    const service = new CatService(repository as unknown as CatRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockFields = { tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], name: "sample", breed: "tabby", ageYears: 0, weightKg: 1.5, indoor: true, birthDate: "2026-01-01", microchipId: "6f1c2a3b-4d5e-4f60-8a7b-000000000000", ownerEmail: "unknown@example.com", website: "https://example.com/items/1", tagCode: "ABC-123", tags: [], scores: [1], adoptedAt: "2026-01-15T10:00:00.000Z", lastVisit: "2026-01-01T00:00:00.000Z", notes: "$none" };
    const mockRecord = {
        id: mockId,
        ...mockFields,
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };
    const mockResponse = { id: mockId, ...{ tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], name: "sample", breed: "tabby", ageYears: 0, weightKg: 1.5, indoor: true, birthDate: "2026-01-01", microchipId: "6f1c2a3b-4d5e-4f60-8a7b-000000000000", ownerEmail: "unknown@example.com", website: "https://example.com/items/1", tagCode: "ABC-123", tags: [], scores: [1], adoptedAt: "2026-01-15T10:00:00.000Z", lastVisit: "2026-01-01T00:00:00.000Z" } };

    beforeEach(() => {
        jest.resetAllMocks();
    });

    it('should create through the repository and answer with the response fields only', async () => {
        repository.create.mockResolvedValue(mockRecord);
        expect(await service.create(mockFields as never, 'mockUser')).toEqual(mockResponse);
        expect(repository.create).toHaveBeenCalledWith(mockFields, 'mockUser');
    });

    it('should get through the repository and answer with the response fields only', async () => {
        repository.get.mockResolvedValue(mockRecord);
        expect(await service.get(mockId)).toEqual(mockResponse);
        expect(repository.get).toHaveBeenCalledWith(mockId);
    });

    it('should read a field the record lacks as its static default', async () => {
        repository.get.mockResolvedValue({ id: mockId, isDeleted: false });
        expect(await service.get(mockId)).toEqual({
            id: mockId,
            tenantId: "public",
            region: "eu",
            priority: null,
            rank: null,
            labels: [],
            name: null,
            breed: "tabby",
            ageYears: 0,
            weightKg: null,
            indoor: true,
            birthDate: null,
            microchipId: null,
            ownerEmail: "unknown@example.com",
            website: null,
            tagCode: null,
            tags: [],
            scores: null,
            adoptedAt: null,
            lastVisit: "2026-01-01T00:00:00.000Z",
        });
    });

    it('should list through the repository and answer with the response fields only', async () => {
        repository.list.mockResolvedValue([mockRecord, { ...mockRecord, id: 'other' }]);
        expect(await service.list(5)).toEqual([mockResponse, { ...mockResponse, id: 'other' }]);
        expect(repository.list).toHaveBeenCalledWith(5);
    });

    it('should update through the repository and answer with the response fields only', async () => {
        repository.update.mockResolvedValue(mockRecord);
        expect(await service.update(mockId, mockFields as never, 'mockUser')).toEqual(mockResponse);
        expect(repository.update).toHaveBeenCalledWith(mockId, mockFields, 'mockUser');
    });

    it('should propagate not found from update', async () => {
        repository.update.mockRejectedValue(new NotFoundError('missing'));
        await expect(service.update(mockId, mockFields as never)).rejects.toBeInstanceOf(NotFoundError);
    });

    it('should replace through the repository and answer with the response fields only', async () => {
        repository.replace.mockResolvedValue(mockRecord);
        expect(await service.replace(mockId, mockFields as never, 'mockUser')).toEqual(mockResponse);
        expect(repository.replace).toHaveBeenCalledWith(mockId, mockFields, 'mockUser');
    });

    it('should propagate not found from replace', async () => {
        repository.replace.mockRejectedValue(new NotFoundError('missing'));
        await expect(service.replace(mockId, mockFields as never)).rejects.toBeInstanceOf(NotFoundError);
    });

    it('should delete through the repository', async () => {
        repository.delete.mockResolvedValue(undefined);
        await service.delete(mockId, 'mockUser');
        expect(repository.delete).toHaveBeenCalledWith(mockId, 'mockUser');
    });
});
