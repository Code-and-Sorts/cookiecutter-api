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
        delete: jest.fn<MockFn>(),
    };
    const service = new CatService(repository as unknown as CatRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockFields = { name: 'mockCat1' };
    const mockResponse = {
        id: mockId,
        ...mockFields,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockCreator',
        updatedTimestamp: '2024-03-25T00:00:00.000Z',
        updatedBy: 'mockUser',
    };
    const mockRecord = { ...mockResponse, isDeleted: false, _etag: 'mockEtag' };

    beforeEach(() => {
        jest.resetAllMocks();
    });

    it('should create through the repository and answer with the response fields only', async () => {
        repository.create.mockResolvedValue(mockRecord);
        expect(await service.create(mockFields, 'mockUser')).toStrictEqual(mockResponse);
        expect(repository.create).toHaveBeenCalledWith(mockFields, 'mockUser');
    });

    it('should get through the repository and answer with the response fields only', async () => {
        repository.get.mockResolvedValue(mockRecord);
        expect(await service.get(mockId)).toStrictEqual(mockResponse);
        expect(repository.get).toHaveBeenCalledWith(mockId);
    });

    it('should list through the repository and answer with the response fields only', async () => {
        repository.list.mockResolvedValue([mockRecord, { ...mockRecord, id: 'other' }]);
        expect(await service.list(5)).toStrictEqual([mockResponse, { ...mockResponse, id: 'other' }]);
        expect(repository.list).toHaveBeenCalledWith(5);
    });

    it('should update through the repository and answer with the response fields only', async () => {
        repository.update.mockResolvedValue(mockRecord);
        expect(await service.update(mockId, mockFields, 'mockUser')).toStrictEqual(mockResponse);
        expect(repository.update).toHaveBeenCalledWith(mockId, mockFields, 'mockUser');
    });

    it('should propagate not found from update', async () => {
        repository.update.mockRejectedValue(new NotFoundError('missing'));
        await expect(service.update(mockId, mockFields)).rejects.toBeInstanceOf(NotFoundError);
    });

    it('should delete through the repository', async () => {
        repository.delete.mockResolvedValue(undefined);
        await service.delete(mockId, 'mockUser');
        expect(repository.delete).toHaveBeenCalledWith(mockId, 'mockUser');
    });
});
