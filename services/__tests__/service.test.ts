
process.env.AWS_REGION = 'us-east-1';

import {
    CatService,
    DogService,
} from '@services';
import {
    CatRepository,
    DogRepository,
} from '@repositories';
import {
    CatEntitySchema,
    DogEntitySchema,
} from '@models';
import { NotFoundError } from '@errors';

describe('CatService', () => {
    const mockGet = jest.fn();
    const mockList = jest.fn();
    const mockCreate = jest.fn();
    const mockUpdate = jest.fn();
    const mockReplace = jest.fn();
    const mockDelete = jest.fn();

    class MockCatRepository {
        get = mockGet;
        list = mockList;
        create = mockCreate;
        update = mockUpdate;
        replace = mockReplace;
        delete = mockDelete;
    }

    beforeEach(() => {
        jest.restoreAllMocks();
        jest.resetAllMocks();
    });

    const mockRepository = new MockCatRepository() as unknown as CatRepository;
    const mockService = new CatService(mockRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockCreateRequest = { name: 'mockCat1' };
    const mockCreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        isDeleted: false,
        name: 'mockCat1',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
    };
    const mockUpdateRequest = { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mockCat1-updated' };
    const mockUpdateResponse = { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mockCat1-updated' };
    const mockResponse = [
        { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mockCat1' },
        { id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21', name: 'mockCat2' },
    ];
    const mockRepositoryResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mockCat1',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mockCat2',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
    ];
    const mockRepositoryUpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockCat1-updated',
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };

    describe('create', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'create').mockReturnValue(Promise.resolve(mockRepositoryResponse[0]));
            jest.spyOn(CatEntitySchema, 'parse').mockReturnValue(mockCreateSchema);
            const result = await mockService.create(mockCreateRequest);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockCreateSchema);
        });
    });

    describe('get', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'get').mockReturnValue(Promise.resolve(mockRepositoryResponse[0]));
            const result = await mockService.get(mockId);
            expect(mockGet).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockId);
        });
    });

    describe('list', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'list').mockReturnValue(Promise.resolve(mockRepositoryResponse));
            const result = await mockService.list();
            expect(mockList).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'update').mockReturnValue(Promise.resolve(mockRepositoryUpdateResponse));
            const result = await mockService.update(mockUpdateRequest);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockUpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });

        it('should refresh updatedTimestamp without sending audit fields', async () => {
            mockUpdate.mockResolvedValue(mockRepositoryUpdateResponse);
            await mockService.update(mockUpdateRequest);
            const written = mockUpdate.mock.calls[0][0];
            expect(written).toEqual(expect.objectContaining(mockUpdateRequest));
            expect(written.createdBy).toBeUndefined();
            expect(written.createdTimestamp).toBeUndefined();
            expect(Date.parse(written.updatedTimestamp)).toBeGreaterThan(Date.parse(mockRepositoryUpdateResponse.updatedTimestamp));
        });

        it('should propagate not found for missing or soft-deleted records', async () => {
            mockUpdate.mockRejectedValue(new NotFoundError(`Record not found for ID ${mockId}.`));
            await expect(mockService.update(mockUpdateRequest)).rejects.toBeInstanceOf(NotFoundError);
        });
    });

    describe('delete', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'delete');
            await mockService.delete(mockId);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockId);
        });
    });
});

describe('DogService', () => {
    const mockGet = jest.fn();
    const mockList = jest.fn();
    const mockCreate = jest.fn();
    const mockUpdate = jest.fn();
    const mockReplace = jest.fn();
    const mockDelete = jest.fn();

    class MockDogRepository {
        get = mockGet;
        list = mockList;
        create = mockCreate;
        update = mockUpdate;
        replace = mockReplace;
        delete = mockDelete;
    }

    beforeEach(() => {
        jest.restoreAllMocks();
        jest.resetAllMocks();
    });

    const mockRepository = new MockDogRepository() as unknown as DogRepository;
    const mockService = new DogService(mockRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockCreateRequest = { name: 'mockDog1' };
    const mockCreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        isDeleted: false,
        name: 'mockDog1',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
    };
    const mockUpdateRequest = { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mockDog1-updated' };
    const mockUpdateResponse = { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mockDog1-updated' };
    const mockResponse = [
        { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mockDog1' },
        { id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21', name: 'mockDog2' },
    ];
    const mockRepositoryResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mockDog1',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mockDog2',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
    ];
    const mockRepositoryUpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockDog1-updated',
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };

    describe('create', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'create').mockReturnValue(Promise.resolve(mockRepositoryResponse[0]));
            jest.spyOn(DogEntitySchema, 'parse').mockReturnValue(mockCreateSchema);
            const result = await mockService.create(mockCreateRequest);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockCreateSchema);
        });
    });

    describe('get', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'get').mockReturnValue(Promise.resolve(mockRepositoryResponse[0]));
            const result = await mockService.get(mockId);
            expect(mockGet).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockId);
        });
    });

    describe('list', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'list').mockReturnValue(Promise.resolve(mockRepositoryResponse));
            const result = await mockService.list();
            expect(mockList).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('replace', () => {
        it('should successfully call repository', async () => {
            mockGet.mockResolvedValue(mockRepositoryResponse[0]);
            const repository = jest.spyOn(mockRepository, 'replace').mockReturnValue(Promise.resolve(mockRepositoryUpdateResponse));
            const result = await mockService.replace(mockUpdateRequest);
            expect(mockGet).toHaveBeenCalledWith(mockId);
            expect(mockReplace).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockUpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });

        it('should preserve createdBy and createdTimestamp and refresh updatedTimestamp', async () => {
            mockGet.mockResolvedValue(mockRepositoryResponse[0]);
            mockReplace.mockImplementation(async (entity) => entity);
            await mockService.replace(mockUpdateRequest);
            const written = mockReplace.mock.calls[0][0];
            expect(written).toEqual(expect.objectContaining({
                id: mockId,
                name: 'mockDog1-updated',
                isDeleted: false,
                createdBy: 'mockUser',
                createdTimestamp: '2024-03-24T00:00:00.000Z',
            }));
            expect(written.updatedBy).toBeUndefined();
            expect(Date.parse(written.updatedTimestamp)).toBeGreaterThan(Date.parse('2024-03-24T00:00:00.000Z'));
        });

        it('should not write when the record is missing or soft-deleted', async () => {
            mockGet.mockRejectedValue(new NotFoundError(`Record not found for ID ${mockId}.`));
            await expect(mockService.replace(mockUpdateRequest)).rejects.toBeInstanceOf(NotFoundError);
            expect(mockReplace).not.toHaveBeenCalled();
        });
    });

    describe('delete', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'delete');
            await mockService.delete(mockId);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockId);
        });
    });
});
