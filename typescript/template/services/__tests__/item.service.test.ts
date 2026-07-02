{% if cloud_service == 'Azure Function App' -%}
process.env.COSMOS_DB_URL = 'https://cosmos-mock.documents.azure.com:443/';
process.env.COSMOS_DB_KEY = 'mock-cosmos-key';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
process.env.GCP_PROJECT_ID = 'mock-gcp-project';
process.env.FIRESTORE_DATABASE = '(default)';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
process.env.AWS_REGION = 'us-east-1';
{%- endif %}

import { injectable } from 'inversify';
import { ItemService } from '@services';
import { ItemRepository } from '@repositories';
import { ItemEntitySchema } from '@models';

let mockResult;
const mockGet = jest.fn().mockImplementation(() => mockResult);
const mockList = jest.fn().mockImplementation(() => mockResult);
const mockCreate = jest.fn().mockImplementation(() => mockResult);
const mockUpdate = jest.fn().mockImplementation(() => mockResult);
const mockReplace = jest.fn().mockImplementation(() => mockResult);
const mockDelete = jest.fn().mockImplementation(() => mockResult);

@injectable()
class MockItemRepository {
    constructor() { }
    get = mockGet;
    list = mockList;
    create = mockCreate;
    update = mockUpdate;
    replace = mockReplace;
    delete = mockDelete;
}

describe('ItemService', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockItemRepository = new MockItemRepository() as unknown as ItemRepository;
    const mockItemService = new ItemService(mockItemRepository);
    const mockItemId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockItemCreate = {
        name: 'mockItem1',
    };
    const mockItemCreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockItem1',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
    };
    const mockItemUpdate = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockItem1-updated',
    };
    const mockItemUpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockItem1-updated',
    };
    const mockItemsResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mockItem1',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mockItem2',
        }
    ];
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
    const mockItemsRepositoryUpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockItem1-updated',
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };

    describe('create', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mockItemRepository, 'create')
                .mockReturnValue(Promise.resolve(mockItemsRepositoryResponse[0]));
            jest
                .spyOn(ItemEntitySchema, 'parse')
                .mockReturnValue(mockItemCreateSchema);
            const result = await mockItemService.create(mockItemCreate);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockItemsResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItemCreateSchema);
        });
    });

    describe('get', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mockItemRepository, 'get')
                .mockReturnValue(Promise.resolve(mockItemsRepositoryResponse[0]));
            const result = await mockItemService.get(mockItemId);
            expect(mockGet).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockItemsResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItemId);
        });
    });

    describe('list', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mockItemRepository, 'list')
                .mockReturnValue(Promise.resolve(mockItemsRepositoryResponse));
            const result = await mockItemService.list();
            expect(mockList).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockItemsResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mockItemRepository, 'update')
                .mockReturnValue(Promise.resolve(mockItemsRepositoryUpdateResponse));
            const result = await mockItemService.update(mockItemUpdate);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockItemUpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('replace', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mockItemRepository, 'replace')
                .mockReturnValue(Promise.resolve(mockItemsRepositoryUpdateResponse));
            jest
                .spyOn(ItemEntitySchema, 'parse')
                .mockReturnValue(mockItemsRepositoryUpdateResponse);
            const result = await mockItemService.replace(mockItemUpdate);
            expect(mockReplace).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockItemUpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('delete', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mockItemRepository, 'delete');
            await mockItemService.delete(mockItemId);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockItemId);
        });
    });
});
