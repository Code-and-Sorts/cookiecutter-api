{% if cookiecutter.cloud_service == 'Azure Function App' -%}
process.env.COSMOS_DB_URL = 'https://cosmos-mock.documents.azure.com:443/';
process.env.COSMOS_DB_KEY = 'mock-cosmos-key';
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
process.env.GCP_PROJECT_ID = 'mock-gcp-project';
process.env.FIRESTORE_DATABASE = '(default)';
process.env.FIRESTORE_COLLECTION = '{{cookiecutter.project_endpoint}}';
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
process.env.AWS_REGION = 'us-east-1';
process.env.DYNAMODB_TABLE_NAME = 'mock-table';
{%- endif %}

import { injectable } from 'inversify';
import { {{cookiecutter.project_class_name}}Service } from '@services';
import { {{cookiecutter.project_class_name}}Repository } from '@repositories';
import { {{cookiecutter.project_class_name}}EntitySchema } from '@models';

let mockResult;
const mockGet = jest.fn().mockImplementation(() => mockResult);
const mockList = jest.fn().mockImplementation(() => mockResult);
const mockCreate = jest.fn().mockImplementation(() => mockResult);
const mockUpdate = jest.fn().mockImplementation(() => mockResult);
const mockDelete = jest.fn().mockImplementation(() => mockResult);

@injectable()
class Mock{{cookiecutter.project_class_name}}Repository {
    constructor() { }
    get = mockGet;
    list = mockList;
    create = mockCreate;
    update = mockUpdate;
    delete = mockDelete;
}

describe('{{cookiecutter.project_class_name}}Service', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mock{{cookiecutter.project_class_name}}Repository = new Mock{{cookiecutter.project_class_name}}Repository() as unknown as {{cookiecutter.project_class_name}}Repository;
    const mock{{cookiecutter.project_class_name}}Service = new {{cookiecutter.project_class_name}}Service(mock{{cookiecutter.project_class_name}}Repository);
    const mock{{cookiecutter.project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mock{{cookiecutter.project_class_name}}Create = {
        name: 'mock{{cookiecutter.project_class_name}}1',
    };
    const mock{{cookiecutter.project_class_name}}CreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{cookiecutter.project_class_name}}1',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
    };
    const mock{{cookiecutter.project_class_name}}Update = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{cookiecutter.project_class_name}}1-updated',
    };
    const mock{{cookiecutter.project_class_name}}UpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{cookiecutter.project_class_name}}1-updated',
    };
    const mock{{cookiecutter.project_class_name}}sResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mock{{cookiecutter.project_class_name}}1',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mock{{cookiecutter.project_class_name}}2',
        }
    ];
    const mock{{cookiecutter.project_class_name}}sRepositoryResponse = [
        {
            id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
            name: 'mock{{cookiecutter.project_class_name}}1',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        },
        {
            id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21',
            name: 'mock{{cookiecutter.project_class_name}}2',
            isDeleted: false,
            createdTimestamp: '2024-03-24T00:00:00.000Z',
            updatedTimestamp: '2024-03-24T00:00:00.000Z',
            createdBy: 'mockUser',
            updatedBy: 'mockUser',
        }
    ];
    const mock{{cookiecutter.project_class_name}}sRepositoryUpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{cookiecutter.project_class_name}}1-updated',
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };

    describe('create{{cookiecutter.project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{cookiecutter.project_class_name}}Repository, 'create')
                .mockReturnValue(Promise.resolve(mock{{cookiecutter.project_class_name}}sRepositoryResponse[0]));
            jest
                .spyOn({{cookiecutter.project_class_name}}EntitySchema, 'parse')
                .mockReturnValue(mock{{cookiecutter.project_class_name}}CreateSchema);
            const result = await mock{{cookiecutter.project_class_name}}Service.create{{cookiecutter.project_class_name}}(mock{{cookiecutter.project_class_name}}Create);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}sResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}CreateSchema);
        });
    });

    describe('get{{cookiecutter.project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{cookiecutter.project_class_name}}Repository, 'get')
                .mockReturnValue(Promise.resolve(mock{{cookiecutter.project_class_name}}sRepositoryResponse[0]));
            const result = await mock{{cookiecutter.project_class_name}}Service.get{{cookiecutter.project_class_name}}(mock{{cookiecutter.project_class_name}}Id);
            expect(mockGet).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}sResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
        });
    });

    describe('get{{cookiecutter.project_class_name}}s', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{cookiecutter.project_class_name}}Repository, 'list')
                .mockReturnValue(Promise.resolve(mock{{cookiecutter.project_class_name}}sRepositoryResponse));
            const result = await mock{{cookiecutter.project_class_name}}Service.get{{cookiecutter.project_class_name}}s();
            expect(mockList).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}sResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('update{{cookiecutter.project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{cookiecutter.project_class_name}}Repository, 'update')
                .mockReturnValue(Promise.resolve(mock{{cookiecutter.project_class_name}}sRepositoryUpdateResponse));
            const result = await mock{{cookiecutter.project_class_name}}Service.update{{cookiecutter.project_class_name}}(mock{{cookiecutter.project_class_name}}Update);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}UpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('delete{{cookiecutter.project_class_name}}', () => {
        it('should successfully call repository', async () => {
            const repository = jest
                .spyOn(mock{{cookiecutter.project_class_name}}Repository, 'delete');
            await mock{{cookiecutter.project_class_name}}Service.delete{{cookiecutter.project_class_name}}(mock{{cookiecutter.project_class_name}}Id);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
        });
    });
});
