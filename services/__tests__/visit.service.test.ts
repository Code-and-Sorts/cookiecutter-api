import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { VisitService } from '@services';
import { VisitRepository } from '@repositories';
import { NotFoundError } from '@errors';
import { MockFn } from '../../test/mocks';

describe('VisitService', () => {
    const repository = {
        create: jest.fn<MockFn>(),
        get: jest.fn<MockFn>(),
        update: jest.fn<MockFn>(),
    };
    const service = new VisitService(repository as unknown as VisitRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockFields = { tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], reason: "sample", visitedOn: "2026-01-01", cost: 1.5, paid: false, checkedAt: ["2026-01-15T10:00:00.000Z"] };
    const mockRecord = {
        id: mockId,
        ...mockFields,
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };
    const mockResponse = { id: mockId, ...{ tenantId: "public", region: "eu", priority: 1, rank: 1.5, labels: [], reason: "sample", visitedOn: "2026-01-01", cost: 1.5, paid: false, checkedAt: ["2026-01-15T10:00:00.000Z"] } };

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
            reason: null,
            visitedOn: null,
            cost: null,
            paid: false,
            checkedAt: null,
        });
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
});
