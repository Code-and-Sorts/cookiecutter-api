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

import { ItemController } from '@controllers';
import { ItemService } from '@services';
import {
    ItemRequestSchema,
    ItemUpdateSchema,
    GuidSchema,
} from '@models';
import { SchemaValidator } from '@services';
import { ValidationError } from '@errors';
import { injectable } from 'inversify';

let mockResult;
const mockGet = jest.fn().mockImplementation(() => mockResult);
const mockList = jest.fn().mockImplementation(() => mockResult);
const mockCreate = jest.fn().mockImplementation(() => mockResult);
const mockUpdate = jest.fn().mockImplementation(() => mockResult);
const mockReplace = jest.fn().mockImplementation(() => mockResult);
const mockDelete = jest.fn().mockImplementation(() => mockResult);

@injectable()
class MockItemService {
    constructor() { }
    get = mockGet;
    list = mockList;
    create = mockCreate;
    update = mockUpdate;
    replace = mockReplace;
    delete = mockDelete;
}

describe('ItemController', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockValidGuid = '91ed1c70-5412-449a-b949-80542a4eb3d5';
    const mockInvalidGuid = 'invalid-guid';
    const mockItemPostRequest = {
        name: 'mockItem',
    };
    const mockItemInvalidPostRequest = {
        name: 'mockItem',
        invalidProp: 'mockInvalidProp',
    };
    const mockItemPutRequest = {
        id: mockValidGuid,
        name: 'mockItem',
    };
    const mockItemInvalidPutRequest = {
        id: mockInvalidGuid,
        name: 'mockItem',
    };
    const mockSchemaValidator = new SchemaValidator();
    const mockItemService = new MockItemService() as unknown as ItemService;
    const mockItemController = new ItemController(
        mockItemService as ItemService,
        mockSchemaValidator as SchemaValidator,
    );

    describe('post', () => {
        it('should successfully call service', async () => {
            await mockItemController.post(mockItemPostRequest);
            expect(mockCreate).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate the item request', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockItemPostRequest);
            await mockItemController.post(mockItemPostRequest);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockItemPostRequest, ItemRequestSchema);
        });

        it('should successfully throw for invalid request', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mockItemController.post(mockItemInvalidPostRequest);
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
            await mockItemController.get(mockValidGuid);
            expect(mockGet).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockValidGuid);
            await mockItemController.get(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mockItemController.get(mockInvalidGuid);
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
            await mockItemController.list();
            expect(mockList).toHaveBeenCalledTimes(1);
        });

        it('should default the limit when not provided', async () => {
            await mockItemController.list();
            expect(mockList).toHaveBeenCalledWith(100);
        });

        it('should coerce and clamp the limit query param', async () => {
            await mockItemController.list('5');
            expect(mockList).toHaveBeenCalledWith(5);

            await mockItemController.list('999999');
            expect(mockList).toHaveBeenCalledWith(1000);
        });
    });

    describe('update', () => {
        it('should successfully call service', async () => {
            await mockItemController.update(mockItemPutRequest);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockItemPutRequest);
            await mockItemController.update(mockItemPutRequest);
            expect(validator).toHaveBeenCalledTimes(2);
            expect(validator).toHaveBeenCalledWith(mockItemPutRequest, ItemUpdateSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mockItemController.update(mockItemInvalidPutRequest);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });

    describe('replace', () => {
        it('should successfully call service', async () => {
            await mockItemController.replace(mockItemPutRequest);
            expect(mockReplace).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate the request', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockItemPutRequest);
            await mockItemController.replace(mockItemPutRequest);
            expect(validator).toHaveBeenCalledTimes(2);
            expect(validator).toHaveBeenCalledWith(mockItemPutRequest, ItemUpdateSchema);
        });
    });

    describe('delete', () => {
        it('should successfully call service', async () => {
            await mockItemController.delete(mockValidGuid);
            expect(mockDelete).toHaveBeenCalledTimes(1);
        });

        it('should successfully validate a valid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate')
                .mockReturnValue(mockValidGuid);
            await mockItemController.delete(mockValidGuid);
            expect(validator).toHaveBeenCalledTimes(1);
            expect(validator).toHaveBeenCalledWith(mockValidGuid, GuidSchema);
        });

        it('should successfully throw validation error for invalid ID', async () => {
            const validator = jest
                .spyOn(mockSchemaValidator, 'validate');
            try {
                await mockItemController.delete(mockInvalidGuid);
            } catch (error) {
                expect(validator).toHaveBeenCalledTimes(1);
                expect(error).toBeInstanceOf(ValidationError);
                expect(error.statusCode).toEqual(422);
                expect(error.message).toEqual('Failed schema validation');
            }
        });
    });
});
