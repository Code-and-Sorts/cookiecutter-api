import "reflect-metadata";
import { CosmosClient } from "@azure/cosmos";
import {
    CatRepository,
    DogRepository,
} from "@repositories";
import { injectable } from "inversify";

let mockResult;

@injectable()
class MockCosmosClient {
    public database = jest.fn().mockImplementation(() => ({
        container: jest.fn(() => mockResult),
    }));
}

describe('CatRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockCosmosClient = new MockCosmosClient() as unknown as CosmosClient;
    const mockRepository = new CatRepository(mockCosmosClient.database('mock-db').container('mock-container'));
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockItem = { name: 'mockCat' };
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
        }
    ];
    const mockItemUpdate = { name: 'mockCatUpdate' };

    describe('create', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'addRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.create(mockItem);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockItem);
        });
    });

    describe('get', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'getRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.get(mockId);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockId);
        });
    });

    describe('list', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'getRecords').mockResolvedValue(mockRepositoryResponse);
            await mockRepository.list();
            expect(base).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'updateRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.update(mockItemUpdate);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockItemUpdate);
        });
    });

    describe('replace', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'replaceRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.replace(mockRepositoryResponse[0]);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockRepositoryResponse[0]);
        });
    });

    describe('delete', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'deleteRecord').mockResolvedValue();
            await mockRepository.delete(mockId);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockId);
        });
    });
});

describe('DogRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockCosmosClient = new MockCosmosClient() as unknown as CosmosClient;
    const mockRepository = new DogRepository(mockCosmosClient.database('mock-db').container('mock-container'));
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockItem = { name: 'mockDog' };
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
        }
    ];
    const mockItemUpdate = { name: 'mockDogUpdate' };

    describe('create', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'addRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.create(mockItem);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockItem);
        });
    });

    describe('get', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'getRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.get(mockId);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockId);
        });
    });

    describe('list', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'getRecords').mockResolvedValue(mockRepositoryResponse);
            await mockRepository.list();
            expect(base).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'updateRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.update(mockItemUpdate);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockItemUpdate);
        });
    });

    describe('replace', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'replaceRecord').mockResolvedValue(mockRepositoryResponse[0]);
            await mockRepository.replace(mockRepositoryResponse[0]);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockRepositoryResponse[0]);
        });
    });

    describe('delete', () => {
        it('should delegate to the base repository', async () => {
            const base = jest.spyOn(mockRepository, 'deleteRecord').mockResolvedValue();
            await mockRepository.delete(mockId);
            expect(base).toHaveBeenCalledTimes(1);
            expect(base).toHaveBeenCalledWith(mockId);
        });
    });
});
