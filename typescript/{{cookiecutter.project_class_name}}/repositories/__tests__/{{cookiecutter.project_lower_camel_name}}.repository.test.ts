import "reflect-metadata";
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { CosmosClient } from "@azure/cosmos";
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
import { Firestore } from "@google-cloud/firestore";
{%- endif %}
import { {{cookiecutter.project_class_name}}Repository } from "@repositories";
import { injectable } from "inversify";

let mockResult{% if cookiecutter.cloud_service == 'GCP Cloud Function' %}: any{% endif %};

@injectable()
class Mock{% if cookiecutter.cloud_service == 'Azure Function App' %}CosmosClient{% elif cookiecutter.cloud_service == 'GCP Cloud Function' %}Firestore{% endif %} {
    {% if cookiecutter.cloud_service == 'Azure Function App' -%}
    public database = jest.fn().mockImplementation(() => ({
        container: jest.fn(() => mockResult),
    }));
    {%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
    public collection = jest.fn().mockImplementation(() => mockResult);
    {%- endif %}
}

describe('{{cookiecutter.project_class_name}}Repository{% if cookiecutter.cloud_service == 'GCP Cloud Function' %} - Firestore{% endif %}', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockClient = new Mock{% if cookiecutter.cloud_service == 'Azure Function App' %}CosmosClient{% elif cookiecutter.cloud_service == 'GCP Cloud Function' %}Firestore{% endif %}() as unknown as {% if cookiecutter.cloud_service == 'Azure Function App' %}CosmosClient{% elif cookiecutter.cloud_service == 'GCP Cloud Function' %}Firestore{% endif %};
    {% if cookiecutter.cloud_service == 'Azure Function App' -%}
    const mock{{cookiecutter.project_class_name}}Repository = new {{cookiecutter.project_class_name}}Repository(mockClient);
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
    {%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}

    it('should initialize with correct collection name', () => {
        const repository = new {{cookiecutter.project_class_name}}Repository(mockClient);
        expect(mockClient.collection).toHaveBeenCalledWith('{{cookiecutter.project_endpoint}}s');
    });

    it('should call create method', async () => {
        const repository = new {{cookiecutter.project_class_name}}Repository(mockClient);
        const createSpy = jest.spyOn(repository, 'create');
        const mockData = { id: '123', name: 'test' };
        
        jest.spyOn(repository as any, 'addRecord').mockResolvedValue(mockData);
        
        await repository.create(mockData as any);
        expect(createSpy).toHaveBeenCalledWith(mockData);
    });

    it('should call get method', async () => {
        const repository = new {{cookiecutter.project_class_name}}Repository(mockClient);
        const getSpy = jest.spyOn(repository, 'get');
        const mockId = '123';
        
        jest.spyOn(repository as any, 'getRecord').mockResolvedValue({ id: mockId });
        
        await repository.get(mockId);
        expect(getSpy).toHaveBeenCalledWith(mockId);
    });

    it('should call list method', async () => {
        const repository = new {{cookiecutter.project_class_name}}Repository(mockClient);
        const listSpy = jest.spyOn(repository, 'list');
        
        jest.spyOn(repository as any, 'getRecords').mockResolvedValue([]);
        
        await repository.list();
        expect(listSpy).toHaveBeenCalled();
    });

    it('should call update method', async () => {
        const repository = new {{cookiecutter.project_class_name}}Repository(mockClient);
        const updateSpy = jest.spyOn(repository, 'update');
        const mockData = { id: '123', name: 'updated' };
        
        jest.spyOn(repository as any, 'updateRecord').mockResolvedValue(mockData);
        
        await repository.update(mockData as any);
        expect(updateSpy).toHaveBeenCalledWith(mockData);
    });

    it('should call delete method', async () => {
        const repository = new {{cookiecutter.project_class_name}}Repository(mockClient);
        const deleteSpy = jest.spyOn(repository, 'delete');
        const mockId = '123';
        
        jest.spyOn(repository as any, 'deleteRecord').mockResolvedValue(undefined);
        
        await repository.delete(mockId);
        expect(deleteSpy).toHaveBeenCalledWith(mockId);
    });
    {%- endif %}
});