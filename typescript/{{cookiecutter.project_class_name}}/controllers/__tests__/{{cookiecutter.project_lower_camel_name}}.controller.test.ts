{% if cookiecutter.cloud_service == 'Azure Function App' -%}
process.env.COSMOS_DB_URL = 'https://cosmos-mock.documents.azure.com:443/';
process.env.COSMOS_DB_KEY = 'mock-cosmos-key';
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
process.env.GCP_PROJECT_ID = 'mock-gcp-project';
process.env.FIRESTORE_DATABASE = '(default)';
process.env.FIRESTORE_COLLECTION = '{{cookiecutter.project_endpoint}}';
{%- endif %}

import { {{cookiecutter.project_class_name}}Controller } from '@controllers';
import { {{cookiecutter.project_class_name}}Service } from '@services';
import {
    {{cookiecutter.project_class_name}}RequestSchema,
    {{cookiecutter.project_class_name}}UpdateSchema,
    GuidSchema,
} from '@models';
import { SchemaValidator } from '@services';
import { ValidationError } from '@errors';
import { injectable } from 'inversify';

let mockResult;
const mockGet{{cookiecutter.project_class_name}} = jest.fn().mockImplementation(() => mockResult);
const mockGet{{cookiecutter.project_class_name}}s = jest.fn().mockImplementation(() => mockResult);
const mockCreate{{cookiecutter.project_class_name}} = jest.fn().mockImplementation(() => mockResult);
const mockUpdate{{cookiecutter.project_class_name}} = jest.fn().mockImplementation(() => mockResult);
const mockDelete{{cookiecutter.project_class_name}} = jest.fn().mockImplementation(() => mockResult);

@injectable()
class Mock{{cookiecutter.project_class_name}}Service {
    constructor() { }
    get{{cookiecutter.project_class_name}} = mockGet{{cookiecutter.project_class_name}};
    get{{cookiecutter.project_class_name}}s = mockGet{{cookiecutter.project_class_name}}s;
    create{{cookiecutter.project_class_name}} = mockCreate{{cookiecutter.project_class_name}};
    update{{cookiecutter.project_class_name}} = mockUpdate{{cookiecutter.project_class_name}};
    delete{{cookiecutter.project_class_name}} = mockDelete{{cookiecutter.project_class_name}};
}

describe('{{cookiecutter.project_class_name}}Controller', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockValidGuid = '91ed1c70-5412-449a-b949-80542a4eb3d5';
    const mockInvalidGuid = 'invalid-guid';
    const mock{{cookiecutter.project_class_name}}PostRequest = {
        name: 'mock{{cookiecutter.project_class_name}}',
    };
    const mock{{cookiecutter.project_class_name}}InvalidPostRequest = {
        name: 'mock{{cookiecutter.project_class_name}}',
        invalidProp: 'mockInvalidProp',
    };
    const mock{{cookiecutter.project_class_name}}PutRequest = {
        id: mockValidGuid,
        name: 'mock{{cookiecutter.project_class_name}}',
    };
    const mock{{cookiecutter.project_class_name}}InvalidPutRequest = {
        id: mockInvalidGuid,
        name: 'mock{{cookiecutter.project_class_name}}',
    };
    const mockSchemaValidator = new SchemaValidator();
    const mock{{cookiecutter.project_class_name}}Service = new Mock{{cookiecutter.project_class_name}}Service() as unknown as {{cookiecutter.project_class_name}}Service;
    const mock{{cookiecutter.project_class_name}}Controller = new {{cookiecutter.project_class_name}}Controller(
        mock{{cookiecutter.project_class_name}}Service as {{cookiecutter.project_class_name}}Service,
        mockSchemaValidator as SchemaValidator,
    );

    describe('post', () => {
        it('should successfully call service', async () => {
            await mock{{cookiecutter.project_class_name}}Controller.post(mock{{cookiecutter.project_class_name}}PostRequest);
            expect(mockCreate{{cookiecutter.project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate the {{cookiecutter.project_lower_camel_name}} request', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mock{{cookiecutter.project_class_name}}PostRequest);
            await mock{{cookiecutter.project_class_name}}Controller.post(mock{{cookiecutter.project_class_name}}PostRequest);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}PostRequest, {{cookiecutter.project_class_name}}RequestSchema);
        });

        it('should successfully throw for invalid request', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{cookiecutter.project_class_name}}Controller.post(mock{{cookiecutter.project_class_name}}InvalidPostRequest);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });

    describe('get', () => {
        it('should successfully call service', async () => {
            await mock{{cookiecutter.project_class_name}}Controller.get(mockValidGuid);
            expect(mockGet{{cookiecutter.project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockValidGuid);
            await mock{{cookiecutter.project_class_name}}Controller.get(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{cookiecutter.project_class_name}}Controller.get(mockInvalidGuid);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });

    describe('list', () => {
        it('should successfully call service', async () => {
            await mock{{cookiecutter.project_class_name}}Controller.list();
            expect(mockGet{{cookiecutter.project_class_name}}s).toHaveBeenCalledTimes(1);
        });
    });

    describe('update', () => {
        it('should successfully call service', async () => {
            await mock{{cookiecutter.project_class_name}}Controller.update(mock{{cookiecutter.project_class_name}}PutRequest);
            expect(mockUpdate{{cookiecutter.project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockUpdate{{cookiecutter.project_class_name}});
            await mock{{cookiecutter.project_class_name}}Controller.update(mockUpdate{{cookiecutter.project_class_name}});
            expect(validator).toHaveBeenCalledTimes(2);
            expect(validator).toHaveBeenCalledWith(mockUpdate{{cookiecutter.project_class_name}}, {{cookiecutter.project_class_name}}UpdateSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{cookiecutter.project_class_name}}Controller.update(mock{{cookiecutter.project_class_name}}InvalidPutRequest);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });

    describe('delete', () => {
        it('should successfully call service', async () => {
            await mock{{cookiecutter.project_class_name}}Controller.delete(mockValidGuid);
            expect(mockDelete{{cookiecutter.project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockValidGuid);
            await mock{{cookiecutter.project_class_name}}Controller.delete(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{cookiecutter.project_class_name}}Controller.delete(mockInvalidGuid);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });
});
