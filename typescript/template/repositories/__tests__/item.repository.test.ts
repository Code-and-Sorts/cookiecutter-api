import "reflect-metadata";
{% if cloud_service == 'Azure Function App' -%}
import { CosmosClient } from "@azure/cosmos";
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
import { Firestore } from "@google-cloud/firestore";
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import { DynamoDBDocumentClient } from "@aws-sdk/lib-dynamodb";
{%- endif %}
import { ItemRepository } from "@repositories";
import { injectable } from "inversify";

let mockResult;
{% if cloud_service == 'Azure Function App' %}
@injectable()
class MockCosmosClient {
    public database = jest.fn().mockImplementation(() => ({
        container: jest.fn(() => mockResult),
    }));
}

describe('ItemRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockCosmosClient = new MockCosmosClient() as unknown as CosmosClient;
    const mockItemRepository = new ItemRepository(mockCosmosClient.database('mock-db').container('mock-container'));
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
@injectable()
class MockFirestore {
    public collection = jest.fn().mockImplementation(() => mockResult);
}

describe('ItemRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockFirestore = new MockFirestore() as unknown as Firestore;
    const mockItemRepository = new ItemRepository(mockFirestore.collection('mock-collection'));
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
@injectable()
class MockDynamoDBDocumentClient {
    public send = jest.fn().mockImplementation(() => mockResult);
}

describe('ItemRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockDocClient = new MockDynamoDBDocumentClient() as unknown as DynamoDBDocumentClient;
    const mockItemRepository = new ItemRepository(mockDocClient, 'mock-table');
{%- endif %}
    const mockItemId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockItem = {
        name: 'mockItem',
    };
    const mockItemsRepositoryResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mockItem1',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mockItem2',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        }
    ];
    const mockItemUpdate = {
        name: 'mockItemUpdate',
    }

    describe('create', () => {
        it('should successfully create an item', async () => {
            const repository = jest.spyOn(mockItemRepository, 'create')
                .mockResolvedValue(mockItemsRepositoryResponse[0]);
            await mockItemRepository.create(mockItem);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItem);
        });
    });

    describe('get', () => {
        it('should successfully get an item', async () => {
            const repository = jest.spyOn(mockItemRepository, 'get')
                .mockResolvedValue(mockItemsRepositoryResponse[0]);
            await mockItemRepository.get(mockItemId);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItemId);
        });
    });

    describe('list', () => {
        it('should successfully list items', async () => {
            const repository = jest.spyOn(mockItemRepository, 'list')
                .mockResolvedValue(mockItemsRepositoryResponse);
            await mockItemRepository.list();
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should successfully update an item', async () => {
            const repository = jest.spyOn(mockItemRepository, 'update')
                .mockResolvedValue(mockItemsRepositoryResponse[0]);
            await mockItemRepository.update(mockItemUpdate);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItemUpdate);
        });
    });

    describe('replace', () => {
        it('should successfully replace an item', async () => {
            const repository = jest.spyOn(mockItemRepository, 'replace')
                .mockResolvedValue(mockItemsRepositoryResponse[0]);
            await mockItemRepository.replace(mockItemsRepositoryResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItemsRepositoryResponse[0]);
        });
    });

    describe('delete', () => {
        it('should successfully delete an item', async () => {
            const repository = jest.spyOn(mockItemRepository, 'delete')
                .mockResolvedValue();
            await mockItemRepository.delete(mockItemId);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItemId);
        });
    });
});
