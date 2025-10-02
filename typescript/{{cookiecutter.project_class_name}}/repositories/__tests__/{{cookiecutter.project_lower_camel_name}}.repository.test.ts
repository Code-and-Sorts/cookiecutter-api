{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import "reflect-metadata";
import { CosmosClient } from "@azure/cosmos";
import { {{cookiecutter.project_class_name}}Repository } from "@repositories";
import { injectable } from "inversify";

let mockResult;

@injectable()
class MockCosmosClient {
    public database = jest.fn().mockImplementation(() => ({
        container: jest.fn(() => mockResult),
    }));
}

describe('{{cookiecutter.project_class_name}}Repository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockCosmosClient = new MockCosmosClient() as unknown as CosmosClient;
    const mock{{cookiecutter.project_class_name}}Repository = new {{cookiecutter.project_class_name}}Repository(mockCosmosClient);
    const mock{{cookiecutter.project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mock{{cookiecutter.project_class_name}} = {
        name: 'mock{{cookiecutter.project_class_name}}',
    };
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
    const mock{{cookiecutter.project_class_name}}Update = {
        name: 'mock{{cookiecutter.project_class_name}}Update',
    }

    describe('create', () => {
        it('should successfully create a {{cookiecutter.project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{cookiecutter.project_class_name}}Repository, 'create')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}sRepositoryResponse[0]);
            const result = await mock{{cookiecutter.project_class_name}}Repository.create(mock{{cookiecutter.project_class_name}});
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}});
        });
    });

    describe('get', () => {
        it('should successfully get a {{cookiecutter.project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{cookiecutter.project_class_name}}Repository, 'get')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}sRepositoryResponse[0]);
            const result = await mock{{cookiecutter.project_class_name}}Repository.get(mock{{cookiecutter.project_class_name}}Id);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
        });
    });

    describe('list', () => {
        it('should successfully get {{cookiecutter.project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{cookiecutter.project_class_name}}Repository, 'list')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}sRepositoryResponse);
            const result = await mock{{cookiecutter.project_class_name}}Repository.list();
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should successfully update a {{cookiecutter.project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{cookiecutter.project_class_name}}Repository, 'update')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}sRepositoryResponse[0]);
            const result = await mock{{cookiecutter.project_class_name}}Repository.update(mock{{cookiecutter.project_class_name}}Update);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Update);
        });
    });

    describe('delete', () => {
        it('should successfully delete a {{cookiecutter.project_lower_camel_name}}', async () => {
            const repository = jest.spyOn(mock{{cookiecutter.project_class_name}}Repository, 'delete')
                .mockResolvedValue();
            const result = await mock{{cookiecutter.project_class_name}}Repository.delete(mock{{cookiecutter.project_class_name}}Id);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
        });
    });
});
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
// TODO: Add Firestore-specific tests for {{cookiecutter.project_class_name}}Repository
describe('{{cookiecutter.project_class_name}}Repository', () => {
    it.skip('TODO: Add Firestore-specific tests', () => {});
});
{%- endif %}