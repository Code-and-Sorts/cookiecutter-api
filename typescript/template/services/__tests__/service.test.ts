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
import {
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
} from '@services';
import {
{%- for resource in resources %}
    {{ resource.name }}Repository,
{%- endfor %}
} from '@repositories';
import {
{%- for resource in resources %}
    {{ resource.name }}EntitySchema,
{%- endfor %}
} from '@models';
{% for resource in resources %}
{%- set r = resource.name %}
describe('{{ r }}Service', () => {
    const mockGet = jest.fn();
    const mockList = jest.fn();
    const mockCreate = jest.fn();
    const mockUpdate = jest.fn();
    const mockReplace = jest.fn();
    const mockDelete = jest.fn();

    @injectable()
    class Mock{{ r }}Repository {
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

    const mockRepository = new Mock{{ r }}Repository() as unknown as {{ r }}Repository;
    const mockService = new {{ r }}Service(mockRepository);
    const mockId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
    const mockCreateRequest = { name: 'mock{{ r }}1' };
    const mockCreateSchema = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{ r }}1',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
    };
    const mockUpdateRequest = { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mock{{ r }}1-updated' };
    const mockUpdateResponse = { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mock{{ r }}1-updated' };
    const mockResponse = [
        { id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0', name: 'mock{{ r }}1' },
        { id: '8123e7f0-b294-4b55-9bd6-d87734d5ad21', name: 'mock{{ r }}2' },
    ];
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
        },
    ];
    const mockRepositoryUpdateResponse = {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{ r }}1-updated',
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };
{%- if "create" in resource.operations %}

    describe('create', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'create').mockReturnValue(Promise.resolve(mockRepositoryResponse[0]));
            jest.spyOn({{ r }}EntitySchema, 'parse').mockReturnValue(mockCreateSchema);
            const result = await mockService.create(mockCreateRequest);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse[0]);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockCreateSchema);
        });
    });
{%- endif %}
{%- if "get_by_id" in resource.operations %}

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
{%- endif %}
{%- if "list" in resource.operations %}

    describe('list', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'list').mockReturnValue(Promise.resolve(mockRepositoryResponse));
            const result = await mockService.list();
            expect(mockList).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });
{%- endif %}
{%- if "update" in resource.operations %}

    describe('update', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'update').mockReturnValue(Promise.resolve(mockRepositoryUpdateResponse));
            const result = await mockService.update(mockUpdateRequest);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockUpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });
{%- endif %}
{%- if "replace" in resource.operations %}

    describe('replace', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'replace').mockReturnValue(Promise.resolve(mockRepositoryUpdateResponse));
            jest.spyOn({{ r }}EntitySchema, 'parse').mockReturnValue(mockRepositoryUpdateResponse);
            const result = await mockService.replace(mockUpdateRequest);
            expect(mockReplace).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockUpdateResponse);
            expect(repository).toHaveBeenCalledTimes(1);
        });
    });
{%- endif %}
{%- if "delete" in resource.operations %}

    describe('delete', () => {
        it('should successfully call repository', async () => {
            const repository = jest.spyOn(mockRepository, 'delete');
            await mockService.delete(mockId);
            expect(repository).toHaveBeenCalledTimes(1);
            expect(repository).toHaveBeenCalledWith(mockId);
        });
    });
{%- endif %}
});
{% endfor %}