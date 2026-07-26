import 'reflect-metadata';
import { BaseRepository } from '@repositories';
import { Container } from '@azure/cosmos';
import { NotFoundError, ProxyError } from '@errors';


class MockNotFound extends Error {
    public code: number | undefined;

    constructor() {
        super('NotFound');
        this.code = 404;
    }
}

const mockKittenClawsCreateRecord = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mockKittenClaws1',
    kittenClawsGenerationData: {},
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
};
const mockKittenClawsRecords = [
    {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mockKittenClaws1',
        kittenClawsGenerationData: {},
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
        _rid: 'A75OAPmg6JcDAAAAAAAAAA==',
        _self: 'dbs/A75OAA==/colls/A75OAPmg6Jc=/docs/A75OAPmg6JcDAAAAAAAAAA==/',
        _etag: '\'12002ff0-0000-0800-0000-6600ec090000\'',
        _attachments: 'attachments/',
        _ts: 1711336457,
    },
    {
        id: 'cb8b2d40-edcc-4ac7-93ba-207408b23c8a',
        name: 'mockKittenClaws2',
        kittenClawsGenerationData: {},
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
        _rid: 'A75OAPmg6JcDAAAAAAAAAA==',
        _self: 'dbs/A75OAA==/colls/A75OAPmg6Jc=/docs/A75OAPmg6JcDAAAAAAAAAA==/',
        _etag: '\'12002ff0-0000-0800-0000-6600ec090000\'',
        _attachments: 'attachments/',
        _ts: 1711336457,
    }
];
const mockKittenClawsUpdateFetchRecord = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mockKittenClaws1Update',
    kittenClawsGenerationData: {},
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
    createdTimestamp: '2024-03-24T00:00:00.000Z',
    updatedTimestamp: '2024-03-24T00:00:00.000Z',
    isDeleted: false,
    _rid: 'A75OAPmg6JcDAAAAAAAAAA==',
    _self: 'dbs/A75OAA==/colls/A75OAPmg6Jc=/docs/A75OAPmg6JcDAAAAAAAAAA==/',
    _etag: '\'12002ff0-0000-0800-0000-6600ec090000\'',
    _attachments: 'attachments/',
    _ts: 1711336457,
};
const mockKittenClawsUpdate = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mockKittenClaws1Update',
};
const mockKittenClawssRepositoryResponse = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mockKittenClaws1',
    isDeleted: false,
    createdTimestamp: '2024-03-24T00:00:00.000Z',
    updatedTimestamp: '2024-03-24T00:00:00.000Z',
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
};
const mockKittenClawsId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const mockGetRecordQuery = 'SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false';
const mockGetRecordsQuery = 'SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100';
const mockGetRecordParameters = [
    {
        name: '@id',
        value: mockKittenClawsId,
    },
];
const mockDeleteUpdatedTimestamp = '2024-03-24T00:00:00.000Z';
const mockDeleteRecordOperations = [
    { op: 'set', path: '/isDeleted', value: true },
    { op: 'set', path: '/updatedTimestamp', value: mockDeleteUpdatedTimestamp }
];
const mockDeleteRecordCondition = 'FROM c WHERE c.isDeleted = false';

let mockResult;
let mockFetchAll;
let mockReplace;
let mockCreate;
let mockPatch;
let mockContainer;
let mockBaseRepository;

describe('BaseRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
        mockFetchAll = jest.fn().mockImplementation(() => mockResult);
        mockReplace = jest.fn().mockImplementation(() => mockResult);
        mockCreate = jest.fn().mockImplementation(() => mockResult);
        mockPatch = jest.fn().mockImplementation(() => mockResult);
        mockContainer = {
            items: {
                query: () => ({
                    fetchAll: mockFetchAll,
                }),
                create: mockCreate,
            },
            item: () => ({
                replace: mockReplace,
                patch: mockPatch,
            }),
        } as unknown as Container;
        mockBaseRepository = new BaseRepository(mockContainer);
    });

    describe('addRecord', () => {
        it('should successfully call item create', async () => {
            mockCreate.mockReturnValue({ resource: mockKittenClawsRecords[0] });
            await mockBaseRepository.addRecord(mockKittenClawsRecords[0]);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(mockCreate).toHaveBeenCalledWith(mockKittenClawsRecords[0]);
        });

        it('should successfully throw proxy error', async () => {
            mockCreate.mockRejectedValueOnce(new Error('mockError'));
            try {
                await mockBaseRepository.addRecord(mockKittenClawsCreateRecord);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error creating item in database.');
            }
        });
    });

    describe('getRecord', () => {
        it('should successfully call query fetchAll by item id', async () => {
            const query = jest.spyOn(mockContainer.items, 'query');
            mockFetchAll.mockReturnValue({ resources: mockKittenClawsRecords });
            const response = await mockBaseRepository.getRecord(mockKittenClawsId);
            expect(mockFetchAll).toHaveBeenCalledTimes(1);
            expect(query).toHaveBeenCalledWith({
                parameters: mockGetRecordParameters,
                query: mockGetRecordQuery,
            });
            expect(response).toEqual(mockKittenClawsRecords[0]);
        });

        it('should successfully throw not found error', async () => {
            mockFetchAll.mockReturnValueOnce({ resources: [] });
            try {
                await mockBaseRepository.getRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record not found for ID ${mockKittenClawsId}.`);
            }
        });

        it('should successfully throw proxy error', async () => {
            mockFetchAll.mockRejectedValueOnce(new Error('Unknown error'));
            try {
                await mockBaseRepository.getRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error creating item in database.');
            }
        });
    });

    describe('getRecords', () => {
        it('should successfully call query fetchAll', async () => {
            const query = jest.spyOn(mockContainer.items, 'query');
            mockFetchAll.mockReturnValue({ resources: mockKittenClawsRecords });
            const response = await mockBaseRepository.getRecords();
            expect(mockFetchAll).toHaveBeenCalledTimes(1);
            expect(query).toHaveBeenCalledWith({
                query: mockGetRecordsQuery,
            });
            expect(response).toEqual(mockKittenClawsRecords);
        });

        it('should successfully throw proxy error', async () => {
            jest.spyOn(mockContainer.items, 'query');
            mockFetchAll.mockRejectedValueOnce(new Error('Unknown error'));
            try {
                await mockBaseRepository.getRecords();
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error creating item in database.');
            }
        });
    });

    describe('updateRecord', () => {
        it('should successfully get and replace record', async () => {
            const containerItem = jest.spyOn(mockContainer, 'item');
            const getRecord = jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mockKittenClawsUpdateFetchRecord);
            mockReplace.mockReturnValue({ resource: mockKittenClawsRecords[0] });
            await mockBaseRepository.updateRecord(mockKittenClawsUpdate);
            expect(getRecord).toHaveBeenCalledTimes(1);
            expect(mockReplace).toHaveBeenCalledTimes(1);
            expect(containerItem).toHaveBeenCalledWith(mockKittenClawsId);
        });

        it('should successfully throw not found error', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mockKittenClawssRepositoryResponse);
            mockReplace.mockRejectedValueOnce(new NotFoundError('Not Found'));

            try {
                await mockBaseRepository.updateRecord(mockKittenClawsUpdate);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mockKittenClawsId} not found.`);
            }
        });

        it('should successfully throw proxy error', async () => {
            mockReplace.mockReturnValueOnce(new Error('mockError'));
            try {
                await mockBaseRepository.updateRecord(mockKittenClawsUpdate);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual(`Error upserting item with id ${mockKittenClawsId}.`);
            }
        });
    });

    describe('replaceRecord', () => {
        it('should successfully get and replace record', async () => {
            const getRecord = jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mockKittenClawsUpdateFetchRecord);
            mockReplace.mockReturnValue({ resource: mockKittenClawsRecords[0] });
            await mockBaseRepository.replaceRecord(mockKittenClawsUpdate);
            expect(getRecord).toHaveBeenCalledTimes(1);
            expect(mockReplace).toHaveBeenCalledTimes(1);
        });

        it('should rethrow not found error', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockRejectedValue(new NotFoundError('Not Found'));
            try {
                await mockBaseRepository.replaceRecord(mockKittenClawsUpdate);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
            }
        });
    });

    describe('deleteRecord', () => {
        beforeEach(() => {
            jest.resetAllMocks();
        });
        it('should successfully delete record', async () => {
            jest.spyOn(Date.prototype, 'toISOString').mockReturnValue(mockDeleteUpdatedTimestamp);
            const containerItem = jest.spyOn(mockContainer, 'item');
            await mockBaseRepository.deleteRecord(mockKittenClawsId);
            expect(mockPatch).toHaveBeenCalledWith({
                condition: mockDeleteRecordCondition,
                operations: mockDeleteRecordOperations,
            });
            expect(containerItem).toHaveBeenCalledTimes(1);
            expect(containerItem).toHaveBeenCalledWith(mockKittenClawsId, mockKittenClawsId);
            expect(mockPatch).toHaveBeenCalledTimes(1);
        });

        it('should successfully throw not found error', async () => {
            try {
                mockPatch.mockRejectedValueOnce(new MockNotFound());
                await mockBaseRepository.deleteRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mockKittenClawsId} not found.`);
            }
        });

        it('should successfully throw proxy error', async () => {
            try {
                mockPatch.mockRejectedValueOnce(new ProxyError('mockProxyError'))
                await mockBaseRepository.deleteRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual(`Error deleting record with id ${mockKittenClawsId}.`);
            }
        });
    });
});
