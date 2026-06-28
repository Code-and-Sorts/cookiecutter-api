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

import { {{project_class_name}}Controller } from '@controllers';
import { {{project_class_name}}Service } from '@services';
import {
    {{project_class_name}}RequestSchema,
    {{project_class_name}}UpdateSchema,
    GuidSchema,
} from '@models';
import { SchemaValidator } from '@services';
import { ValidationError } from '@errors';
import { injectable } from 'inversify';

let mockResult;
const mockGet{{project_class_name}} = jest.fn().mockImplementation(() => mockResult);
const mockGet{{project_class_name}}s = jest.fn().mockImplementation(() => mockResult);
const mockCreate{{project_class_name}} = jest.fn().mockImplementation(() => mockResult);
const mockUpdate{{project_class_name}} = jest.fn().mockImplementation(() => mockResult);
const mockDelete{{project_class_name}} = jest.fn().mockImplementation(() => mockResult);

@injectable()
class Mock{{project_class_name}}Service {
    constructor() { }
    get{{project_class_name}} = mockGet{{project_class_name}};
    get{{project_class_name}}s = mockGet{{project_class_name}}s;
    create{{project_class_name}} = mockCreate{{project_class_name}};
    update{{project_class_name}} = mockUpdate{{project_class_name}};
    delete{{project_class_name}} = mockDelete{{project_class_name}};
}

describe('{{project_class_name}}Controller', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockValidGuid = '91ed1c70-5412-449a-b949-80542a4eb3d5';
    const mockInvalidGuid = 'invalid-guid';
    const mock{{project_class_name}}PostRequest = {
        name: 'mock{{project_class_name}}',
    };
    const mock{{project_class_name}}InvalidPostRequest = {
        name: 'mock{{project_class_name}}',
        invalidProp: 'mockInvalidProp',
    };
    const mock{{project_class_name}}PutRequest = {
        id: mockValidGuid,
        name: 'mock{{project_class_name}}',
    };
    const mock{{project_class_name}}InvalidPutRequest = {
        id: mockInvalidGuid,
        name: 'mock{{project_class_name}}',
    };
    const mockSchemaValidator = new SchemaValidator();
    const mock{{project_class_name}}Service = new Mock{{project_class_name}}Service() as unknown as {{project_class_name}}Service;
    const mock{{project_class_name}}Controller = new {{project_class_name}}Controller(
        mock{{project_class_name}}Service as {{project_class_name}}Service,
        mockSchemaValidator as SchemaValidator,
    );

    describe('post', () => {
        it('should successfully call service', async () => {
            await mock{{project_class_name}}Controller.post(mock{{project_class_name}}PostRequest);
            expect(mockCreate{{project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate the {{project_lower_camel_name}} request', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mock{{project_class_name}}PostRequest);
            await mock{{project_class_name}}Controller.post(mock{{project_class_name}}PostRequest);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mock{{project_class_name}}PostRequest, {{project_class_name}}RequestSchema);
        });

        it('should successfully throw for invalid request', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{project_class_name}}Controller.post(mock{{project_class_name}}InvalidPostRequest);
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
            await mock{{project_class_name}}Controller.get(mockValidGuid);
            expect(mockGet{{project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockValidGuid);
            await mock{{project_class_name}}Controller.get(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{project_class_name}}Controller.get(mockInvalidGuid);
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
            await mock{{project_class_name}}Controller.list();
            expect(mockGet{{project_class_name}}s).toHaveBeenCalledTimes(1);
        });

        it('should default the limit when not provided', async () => {
            await mock{{project_class_name}}Controller.list();
            expect(mockGet{{project_class_name}}s).toHaveBeenCalledWith(100);
        });

        it('should coerce and clamp the limit query param', async () => {
            await mock{{project_class_name}}Controller.list('5');
            expect(mockGet{{project_class_name}}s).toHaveBeenCalledWith(5);

            await mock{{project_class_name}}Controller.list('999999');
            expect(mockGet{{project_class_name}}s).toHaveBeenCalledWith(1000);
        });
    });

    describe('update', () => {
        it('should successfully call service', async () => {
            await mock{{project_class_name}}Controller.update(mock{{project_class_name}}PutRequest);
            expect(mockUpdate{{project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockUpdate{{project_class_name}});
            await mock{{project_class_name}}Controller.update(mockUpdate{{project_class_name}});
            expect(validator).toHaveBeenCalledTimes(2);
            expect(validator).toHaveBeenCalledWith(mockUpdate{{project_class_name}}, {{project_class_name}}UpdateSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{project_class_name}}Controller.update(mock{{project_class_name}}InvalidPutRequest);
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
            await mock{{project_class_name}}Controller.delete(mockValidGuid);
            expect(mockDelete{{project_class_name}}).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockValidGuid);
            await mock{{project_class_name}}Controller.delete(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mock{{project_class_name}}Controller.delete(mockInvalidGuid);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });
});
