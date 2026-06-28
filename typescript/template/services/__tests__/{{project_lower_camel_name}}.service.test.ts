{% if cloud_service == 'Azure Function App' -%}
process.env.COSMOS_DB_URL = 'https://cosmos-mock.documents.azure.com:443/';
process.env.COSMOS_DB_KEY = 'mock-cosmos-key';
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
process.env.GCP_PROJECT_ID = 'mock-gcp-project';
process.env.FIRESTORE_DATABASE = '(default)';
process.env.FIRESTORE_COLLECTION = '{{project_endpoint}}';
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
process.env.AWS_REGION = 'us-east-1';
process.env.DYNAMODB_TABLE_NAME = 'mock-table';
{%- endif %}

import { injectable } from 'inversify';
import { {{project_class_name}}Service } from '@services';
import { {{project_class_name}}Repository } from '@repositories';
import { {{project_class_name}}EntitySchema } from '@models';

let mockResult;
const mockGet = jest.fn().mockImplementation(() => mockResult);
const mockList = jest.fn().mockImplementation(() => mockResult);
const mockCreate = jest.fn().mockImplementation(() => mockResult);
const mockUpdate = jest.fn().mockImplementation(() => mockResult);
const mockDelete = jest.fn().mockImplementation(() => mockResult);

@injectable()
class Mock{{project_class_name}}Repository {
    constructor() { }
    get = mockGet;
    list = mockList;
    create = mockCreate;
    update = mockUpdate;
    delete = mockDelete;
}

describe('{{project_class_name}}Service', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mock{{project_class_name}}Repository = new Mock{{project_class_name}}Repository() as unknown as {{project_class_name}}Repository;
    const mock{{project_class_name}}Service = new {{project_class_name}}Service(mock{{project_class_name}}Repository);
    const mock{{project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mock{{project_class_name}}Create = {
        name: 'mock{{project_class_name}}1',
    };
    const mock{{project_class_name}}CreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{project_class_name}}1',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
    };
    const mock{{project_class_name}}Update = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{project_class_name}}1-updated',
    };
    const mock{{project_class_name}}UpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{project_class_name}}1-updated',
    };
    const mock{{project_class_name}}sResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mock{{project_class_name}}1',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mock{{project_class_name}}2',
        }
    ];
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
    const mock{{project_class_name}}sRepositoryUpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{project_class_name}}1-updated',
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };

    describe('create{{project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{project_class_name}}Repository, 'create')
                .mockReturnValue(Promise.resolve(mock{{project_class_name}}sRepositoryResponse[0]));
            jest
                .spyOn({{project_class_name}}EntitySchema, 'parse')
                .mockReturnValue(mock{{project_class_name}}CreateSchema);
            const result = await mock{{project_class_name}}Service.create{{project_class_name}}(mock{{project_class_name}}Create);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{project_class_name}}sResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{project_class_name}}CreateSchema);
        });
    });

    describe('get{{project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{project_class_name}}Repository, 'get')
                .mockReturnValue(Promise.resolve(mock{{project_class_name}}sRepositoryResponse[0]));
            const result = await mock{{project_class_name}}Service.get{{project_class_name}}(mock{{project_class_name}}Id);
            expect(mockGet).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{project_class_name}}sResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{project_class_name}}Id);
        });
    });

    describe('get{{project_class_name}}s', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{project_class_name}}Repository, 'list')
                .mockReturnValue(Promise.resolve(mock{{project_class_name}}sRepositoryResponse));
            const result = await mock{{project_class_name}}Service.get{{project_class_name}}s();
            expect(mockList).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{project_class_name}}sResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('update{{project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{project_class_name}}Repository, 'update')
                .mockReturnValue(Promise.resolve(mock{{project_class_name}}sRepositoryUpdateResponse));
            const result = await mock{{project_class_name}}Service.update{{project_class_name}}(mock{{project_class_name}}Update);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{project_class_name}}UpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('delete{{project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{project_class_name}}Repository, 'delete');
            await mock{{project_class_name}}Service.delete{{project_class_name}}(mock{{project_class_name}}Id);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{project_class_name}}Id);
        });
    });
});
