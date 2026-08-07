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
import {
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
} from "@repositories";
import { injectable } from "inversify";

let mockResult;
{% if cloud_service == 'Azure Function App' %}
@injectable()
class MockCosmosClient {
    public database = jest.fn().mockImplementation(() => ({
        container: jest.fn(() => mockResult),
    }));
}
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
@injectable()
class MockFirestore {
    public collection = jest.fn().mockImplementation(() => mockResult);
}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
@injectable()
class MockDynamoDBDocumentClient {
    public send = jest.fn().mockImplementation(() => mockResult);
}
{%- endif %}
{% for resource in resources %}
{%- set r = resource.name %}
describe('{{ r }}Repository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });
{%- if cloud_service == 'Azure Function App' %}

    const mockCosmosClient = new MockCosmosClient() as unknown as CosmosClient;
    const mockRepository = new {{ r }}Repository(mockCosmosClient.database('mock-db').container('mock-container'));
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    const mockFirestore = new MockFirestore() as unknown as Firestore;
    const mockRepository = new {{ r }}Repository(mockFirestore.collection('mock-collection'));
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    const mockDocClient = new MockDynamoDBDocumentClient() as unknown as DynamoDBDocumentClient;
    const mockRepository = new {{ r }}Repository(mockDocClient, 'mock-table');
{%- endif %}
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockItem = { name: 'mock{{ r }}' };
    const mockRepositoryResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mock{{ r }}1',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mock{{ r }}2',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        }
    ];
    const mockItemUpdate = { name: 'mock{{ r }}Update' };

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
{% endfor %}