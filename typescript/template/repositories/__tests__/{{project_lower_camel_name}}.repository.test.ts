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
import { {{project_class_name}}Repository } from "@repositories";
import { injectable } from "inversify";

let mockResult;
{% if cloud_service == 'Azure Function App' %}
@injectable()
class MockCosmosClient {
    public database = jest.fn().mockImplementation(() => ({
        container: jest.fn(() => mockResult),
    }));
}

describe('{{project_class_name}}Repository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockCosmosClient = new MockCosmosClient() as unknown as CosmosClient;
    const mock{{project_class_name}}Repository = new {{project_class_name}}Repository(mockCosmosClient);
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
@injectable()
class MockFirestore {
    public collection = jest.fn().mockImplementation(() => mockResult);
}

describe('{{project_class_name}}Repository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockFirestore = new MockFirestore() as unknown as Firestore;
    const mock{{project_class_name}}Repository = new {{project_class_name}}Repository(mockFirestore);
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
@injectable()
class MockDynamoDBDocumentClient {
    public send = jest.fn().mockImplementation(() => mockResult);
}

describe('{{project_class_name}}Repository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockDocClient = new MockDynamoDBDocumentClient() as unknown as DynamoDBDocumentClient;
    const mock{{project_class_name}}Repository = new {{project_class_name}}Repository(mockDocClient);
{%- endif %}
    const mock{{project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mock{{project_class_name}} = {
        name: 'mock{{project_class_name}}',
    };
    const mock{{project_class_name}}sRepositoryResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mock{{project_class_name}}1',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mock{{project_class_name}}2',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        }
    ];
    const mock{{project_class_name}}Update = {
        name: 'mock{{project_class_name}}Update',
    }

    describe('create', () => {
        it('should successfully create a {{project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{project_class_name}}Repository, 'create')
                .mockResolvedValue(mock{{project_class_name}}sRepositoryResponse[0]);
            const result = await mock{{project_class_name}}Repository.create(mock{{project_class_name}});
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{project_class_name}});
        });
    });

    describe('get', () => {
        it('should successfully get a {{project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{project_class_name}}Repository, 'get')
                .mockResolvedValue(mock{{project_class_name}}sRepositoryResponse[0]);
            const result = await mock{{project_class_name}}Repository.get(mock{{project_class_name}}Id);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{project_class_name}}Id);
        });
    });

    describe('list', () => {
        it('should successfully get {{project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{project_class_name}}Repository, 'list')
                .mockResolvedValue(mock{{project_class_name}}sRepositoryResponse);
            const result = await mock{{project_class_name}}Repository.list();
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should successfully update a {{project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{project_class_name}}Repository, 'update')
                .mockResolvedValue(mock{{project_class_name}}sRepositoryResponse[0]);
            const result = await mock{{project_class_name}}Repository.update(mock{{project_class_name}}Update);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{project_class_name}}Update);
        });
    });

    describe('delete', () => {
        it('should successfully delete a {{project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{project_class_name}}Repository, 'delete')
                .mockResolvedValue();
            const result = await mock{{project_class_name}}Repository.delete(mock{{project_class_name}}Id);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{project_class_name}}Id);
        });
    });
});
