import { describe, it, expect, beforeEach, jest } from '@jest/globals';
import { DogController } from '@controllers';
import { DogService, SchemaValidator } from '@services';
import { NotFoundError, ValidationError } from '@errors';
import { MockFn } from '../../test/mocks';

describe('DogController', () => {
    const mockGet = jest.fn<MockFn>();
    const mockList = jest.fn<MockFn>();
    const mockCreate = jest.fn<MockFn>();
    const mockUpdate = jest.fn<MockFn>();
    const mockReplace = jest.fn<MockFn>();
    const mockDelete = jest.fn<MockFn>();

    class MockDogService {
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
    const mockBody = { name: 'mockDog' };
    const mockService = new MockDogService() as unknown as DogService;
    const mockController = new DogController(mockService, new SchemaValidator());

    const expectBadRequest = async (call: Promise<unknown>, message: string) => {
        await expect(call).rejects.toBeInstanceOf(ValidationError);
        await expect(call).rejects.toMatchObject({ statusCode: 400, message });
    };

    const expectNotFound = async (call: Promise<unknown>, id: string) => {
        await expect(call).rejects.toBeInstanceOf(NotFoundError);
        await expect(call).rejects.toMatchObject({ statusCode: 404, message: `Dog with id ${id} was not found.` });
    };

    const systemFields = {
        id: mockValidGuid,
        isDeleted: false,
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
    };

    const expectUnknownFieldsRejected = async (send: (body: object) => Promise<unknown>, service: jest.Mock<MockFn>) => {
        await expectBadRequest(send({ ...mockBody, invalidProp: 'x' }), 'Unknown field: invalidProp.');
        for (const [field, value] of Object.entries(systemFields)) {
            await expectBadRequest(send({ ...mockBody, [field]: value }), `Unknown field: ${field}.`);
        }
        expect(service).not.toHaveBeenCalled();
    };

    describe('post', () => {
        it('should pass the validated body and user id to the service', async () => {
            mockCreate.mockResolvedValue({ id: mockValidGuid, ...mockBody });
            expect(await mockController.post(mockBody, 'mockUser')).toEqual({ id: mockValidGuid, ...mockBody });
            expect(mockCreate).toHaveBeenCalledWith(mockBody, 'mockUser');
        });

        it('should return 400 when name is missing', async () => {
            await expectBadRequest(mockController.post({}), 'name is required.');
            expect(mockCreate).not.toHaveBeenCalled();
        });

        it('should return 400 when name is not a string', async () => {
            await expectBadRequest(mockController.post({ name: 42 }), 'name must be a string.');
            await expectBadRequest(mockController.post({ name: true }), 'name must be a string.');
        });

        it('should return 400 when name is empty', async () => {
            await expectBadRequest(mockController.post({ name: '' }), 'name must not be empty.');
        });

        it('should return 400 for unknown fields, including id and system fields', async () => {
            await expectUnknownFieldsRejected((body) => mockController.post(body), mockCreate);
            await expectBadRequest(
                mockController.post({ ...mockBody, isDeleted: false, createdBy: 'x' }),
                'Unknown fields: isDeleted, createdBy.',
            );
            expect(mockCreate).not.toHaveBeenCalled();
        });

        it('should return 400 for a body that is not an object', async () => {
            await expectBadRequest(mockController.post([mockBody]), 'Request body must be a JSON object.');
            await expectBadRequest(mockController.post('mock'), 'Request body must be a JSON object.');
        });
    });

    describe('get', () => {
        it('should call the service with the id', async () => {
            await mockController.get(mockValidGuid);
            expect(mockGet).toHaveBeenCalledWith(mockValidGuid);
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.get(mockInvalidGuid), mockInvalidGuid);
            expect(mockGet).not.toHaveBeenCalled();
        });
    });

    describe('list', () => {
        it('should default the limit when not provided', async () => {
            await mockController.list();
            expect(mockList).toHaveBeenCalledWith(100);
        });

        it('should coerce and clamp the limit query param', async () => {
            await mockController.list('5');
            expect(mockList).toHaveBeenCalledWith(5);

            await mockController.list('999999');
            expect(mockList).toHaveBeenCalledWith(1000);

            await mockController.list('abc');
            expect(mockList).toHaveBeenCalledWith(100);
        });
    });

    describe('replace', () => {
        it('should pass the path id, validated body and user id to the service', async () => {
            await mockController.replace(mockValidGuid, mockBody, 'mockUser');
            expect(mockReplace).toHaveBeenCalledWith(mockValidGuid, mockBody, 'mockUser');
        });

        it('should return 400 when name is missing', async () => {
            await expectBadRequest(mockController.replace(mockValidGuid, {}), 'name is required.');
            expect(mockReplace).not.toHaveBeenCalled();
        });

        it('should return 400 for unknown fields, including id and system fields', async () => {
            await expectUnknownFieldsRejected((body) => mockController.replace(mockValidGuid, body), mockReplace);
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.replace(mockInvalidGuid, mockBody), mockInvalidGuid);
            expect(mockReplace).not.toHaveBeenCalled();
        });
    });

    describe('delete', () => {
        it('should call the service and return the confirmation message', async () => {
            expect(await mockController.delete(mockValidGuid, 'mockUser')).toEqual({
                message: `Dog with id ${mockValidGuid} was deleted successfully.`,
            });
            expect(mockDelete).toHaveBeenCalledWith(mockValidGuid, 'mockUser');
        });

        it('should return 404 for an id that is not a UUID', async () => {
            await expectNotFound(mockController.delete(mockInvalidGuid), mockInvalidGuid);
            expect(mockDelete).not.toHaveBeenCalled();
        });
    });
});
