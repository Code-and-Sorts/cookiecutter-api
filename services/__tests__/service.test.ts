
process.env.GCP_PROJECT_ID = 'mock-gcp-project';
process.env.FIRESTORE_DATABASE = '(default)';

import { injectable } from 'inversify';
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

describe('CatService', () => {
    const mockGet = jest.fn();
    const mockList = jest.fn();
    const mockCreate = jest.fn();
    const mockUpdate = jest.fn();
    const mockReplace = jest.fn();
    const mockDelete = jest.fn();

    @injectable()
    class MockCatRepository {
        get = mockGet;
        list = mockList;
        create = mockCreate;
        update = mockUpdate;
        replace = mockReplace;
        delete = mockDelete;
    }

    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockRepository = new MockCatRepository() as unknown as CatRepository;
    const mockService = new CatService(mockRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockCreateRequest = { name: 'mockCat1' };
    const mockCreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
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

    @injectable()
    class MockDogRepository {
        get = mockGet;
        list = mockList;
        create = mockCreate;
        update = mockUpdate;
        replace = mockReplace;
        delete = mockDelete;
    }

    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockRepository = new MockDogRepository() as unknown as DogRepository;
    const mockService = new DogService(mockRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockCreateRequest = { name: 'mockDog1' };
    const mockCreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
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
            const repository = jest.spyOn(mockRepository, 'replace').mockReturnValue(Promise.resolve(mockRepositoryUpdateResponse));
            jest.spyOn(DogEntitySchema, 'parse').mockReturnValue(mockRepositoryUpdateResponse);
            const result = await mockService.replace(mockUpdateRequest);
            expect(mockReplace).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockUpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
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
