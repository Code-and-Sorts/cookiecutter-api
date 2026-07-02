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

import {
{%- for resource in resources %}
    {{ resource.name }}Controller,
{%- endfor %}
} from '@controllers';
import {
{%- for resource in resources %}
    {{ resource.name }}Service,
{%- endfor %}
} from '@services';
import {
{%- for resource in resources %}
    {{ resource.name }}RequestSchema,
    {{ resource.name }}UpdateSchema,
{%- endfor %}
    GuidSchema,
} from '@models';
import { SchemaValidator } from '@services';
import { ValidationError } from '@errors';
import { injectable } from 'inversify';
{% for resource in resources %}
{%- set r = resource.name %}
describe('{{ r }}Controller', () => {
    const mockGet = jest.fn();
    const mockList = jest.fn();
    const mockCreate = jest.fn();
    const mockUpdate = jest.fn();
    const mockReplace = jest.fn();
    const mockDelete = jest.fn();

    @injectable()
    class Mock{{ r }}Service {
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

    const mockValidGuid = '91ed1c70-5412-449a-b949-80542a4eb3d5';
    const mockInvalidGuid = 'invalid-guid';
    const mockPostRequest = { name: 'mock{{ r }}' };
    const mockInvalidPostRequest = { name: 'mock{{ r }}', invalidProp: 'mockInvalidProp' };
    const mockPutRequest = { id: mockValidGuid, name: 'mock{{ r }}' };
    const mockInvalidPutRequest = { id: mockInvalidGuid, name: 'mock{{ r }}' };
    const mockSchemaValidator = new SchemaValidator();
    const mockService = new Mock{{ r }}Service() as unknown as {{ r }}Service;
    const mockController = new {{ r }}Controller(mockService, mockSchemaValidator);
{%- if "create" in resource.operations %}

    describe('post', () => {
        it('should successfully call service', async () => {
            await mockController.post(mockPostRequest);
            expect(mockCreate).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate the item request', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate').mockReturnValue(mockPostRequest);
            await mockController.post(mockPostRequest);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockPostRequest, {{ r }}RequestSchema);
        });

        it('should successfully throw for invalid request', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate');
            try {
                await mockController.post(mockInvalidPostRequest);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });
{%- endif %}
{%- if "get_by_id" in resource.operations %}

    describe('get', () => {
        it('should successfully call service', async () => {
            await mockController.get(mockValidGuid);
            expect(mockGet).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate').mockReturnValue(mockValidGuid);
            await mockController.get(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate');
            try {
                await mockController.get(mockInvalidGuid);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });
{%- endif %}
{%- if "list" in resource.operations %}

    describe('list', () => {
        it('should successfully call service', async () => {
            await mockController.list();
            expect(mockList).toHaveBeenCalledTimes(1);
        });

        it('should default the limit when not provided', async () => {
            await mockController.list();
            expect(mockList).toHaveBeenCalledWith(100);
        });

        it('should coerce and clamp the limit query param', async () => {
            await mockController.list('5');
            expect(mockList).toHaveBeenCalledWith(5);

            await mockController.list('999999');
            expect(mockList).toHaveBeenCalledWith(1000);
        });
    });
{%- endif %}
{%- if "update" in resource.operations %}

    describe('update', () => {
        it('should successfully call service', async () => {
            await mockController.update(mockPutRequest);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate').mockReturnValue(mockPutRequest);
            await mockController.update(mockPutRequest);
            expect(validator).toHaveBeenCalledTimes(2);
            expect(validator).toHaveBeenCalledWith(mockPutRequest, {{ r }}UpdateSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate');
            try {
                await mockController.update(mockInvalidPutRequest);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });
{%- endif %}
{%- if "replace" in resource.operations %}

    describe('replace', () => {
        it('should successfully call service', async () => {
            await mockController.replace(mockPutRequest);
            expect(mockReplace).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate the request', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate').mockReturnValue(mockPutRequest);
            await mockController.replace(mockPutRequest);
            expect(validator).toHaveBeenCalledTimes(2);
            expect(validator).toHaveBeenCalledWith(mockPutRequest, {{ r }}UpdateSchema);
        });
    });
{%- endif %}
{%- if "delete" in resource.operations %}

    describe('delete', () => {
        it('should successfully call service', async () => {
            await mockController.delete(mockValidGuid);
            expect(mockDelete).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate').mockReturnValue(mockValidGuid);
            await mockController.delete(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest.spyOn(mockSchemaValidator, 'validate');
            try {
                await mockController.delete(mockInvalidGuid);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });
{%- endif %}
});
{% endfor %}