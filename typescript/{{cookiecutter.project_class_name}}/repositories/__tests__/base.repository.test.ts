import 'reflect-metadata';
import { BaseRepository } from '@repositories';
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import { Container } from '@azure/cosmos';
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
import { CollectionReference } from '@google-cloud/firestore';
{%- endif %}
import { NotFoundError, ProxyError } from '@errors';


{% if cookiecutter.cloud_service == 'Azure Function App' -%}
class MockNotFound extends Error {
    public code: number | undefined;

    constructor() {
        super('NotFound');
        this.code = 404;
    }
}

const mock{{cookiecutter.project_class_name}}CreateRecord = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mock{{cookiecutter.project_class_name}}1',
    {{cookiecutter.project_lower_camel_name}}GenerationData: {},
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
};
const mock{{cookiecutter.project_class_name}}Records = [
    {
        id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
        name: 'mock{{cookiecutter.project_class_name}}1',
        {{cookiecutter.project_lower_camel_name}}GenerationData: {},
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
        name: 'mock{{cookiecutter.project_class_name}}2',
        {{cookiecutter.project_lower_camel_name}}GenerationData: {},
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
const mock{{cookiecutter.project_class_name}}UpdateFetchRecord = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mock{{cookiecutter.project_class_name}}1Update',
    {{cookiecutter.project_lower_camel_name}}GenerationData: {},
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
const mock{{cookiecutter.project_class_name}}Update = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mock{{cookiecutter.project_class_name}}1Update',
};
const mock{{cookiecutter.project_class_name}}sRepositoryResponse = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mock{{cookiecutter.project_class_name}}1',
    isDeleted: false,
    createdTimestamp: '2024-03-24T00:00:00.000Z',
    updatedTimestamp: '2024-03-24T00:00:00.000Z',
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
};
const mock{{cookiecutter.project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const mockGetRecordQuery = 'SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false';
const mockGetRecordsQuery = 'SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT 100';
const mockGetRecordParameters = [
    {
        name: '@id',
        value: mock{{cookiecutter.project_class_name}}Id,
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
            mockCreate.mockReturnValue({ resource: mock{{cookiecutter.project_class_name}}Records[0] });
            await mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}Records[0]);
            expect(mockCreate).toHaveBeenCalledTimes(1);
            expect(mockCreate).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Records[0]);
        });

        it('should successfully throw proxy error', async () => {
            mockCreate.mockRejectedValueOnce(new Error('mockError'));
            try {
                await mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}CreateRecord);
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
            mockFetchAll.mockReturnValue({ resources: mock{{cookiecutter.project_class_name}}Records });
            const response = await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            expect(mockFetchAll).toHaveBeenCalledTimes(1);
            expect(query).toHaveBeenCalledWith({
                parameters: mockGetRecordParameters,
                query: mockGetRecordQuery,
            });
            expect(response).toEqual(mock{{cookiecutter.project_class_name}}Records[0]);
        });

        it('should successfully throw not found error', async () => {
            mockFetchAll.mockReturnValueOnce({ resources: [] });
            try {
                await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record not found for ID ${mock{{cookiecutter.project_class_name}}Id}.`);
            }
        });

        it('should successfully throw proxy error', async () => {
            mockFetchAll.mockRejectedValueOnce(new Error('Unknown error'));
            try {
                await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
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
            mockFetchAll.mockReturnValue({ resources: mock{{cookiecutter.project_class_name}}Records });
            const response = await mockBaseRepository.getRecords();
            expect(mockFetchAll).toHaveBeenCalledTimes(1);
            expect(query).toHaveBeenCalledWith({
                query: mockGetRecordsQuery,
            });
            expect(response).toEqual(mock{{cookiecutter.project_class_name}}Records);
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
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}UpdateFetchRecord);
            mockReplace.mockReturnValue({ resource: mock{{cookiecutter.project_class_name}}Records[0] });
            await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            expect(getRecord).toHaveBeenCalledTimes(1);
            expect(mockReplace).toHaveBeenCalledTimes(1);
            expect(containerItem).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
        });

        it('should successfully throw not found error', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}sRepositoryResponse);
            mockReplace.mockRejectedValueOnce(new NotFoundError('Not Found'));

            try {
                await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mock{{cookiecutter.project_class_name}}Id} not found.`);
            }
        });

        it('should successfully throw proxy error', async () => {
            mockReplace.mockReturnValueOnce(new Error('mockError'));
            try {
                await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual(`Error upserting item with id ${mock{{cookiecutter.project_class_name}}Id}.`);
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
            await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            expect(mockPatch).toHaveBeenCalledWith({
                condition: mockDeleteRecordCondition,
                operations: mockDeleteRecordOperations,
            });
            expect(containerItem).toHaveBeenCalledTimes(1);
            expect(containerItem).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id, mock{{cookiecutter.project_class_name}}Id);
            expect(mockPatch).toHaveBeenCalledTimes(1);
        });

        it('should successfully throw not found error', async () => {
            try {
                mockPatch.mockRejectedValueOnce(new MockNotFound());
                await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mock{{cookiecutter.project_class_name}}Id} not found.`);
            }
        });

        it('should successfully throw proxy error', async () => {
            try {
                mockPatch.mockRejectedValueOnce(new ProxyError('mockProxyError'))
                await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual(`Error deleting record with id ${mock{{cookiecutter.project_class_name}}Id}.`);
            }
        });
    });
});
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
const mock{{cookiecutter.project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const mock{{cookiecutter.project_class_name}}CreateRecord = {
    id: mock{{cookiecutter.project_class_name}}Id,
    name: 'mock{{cookiecutter.project_class_name}}1',
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
};
const mock{{cookiecutter.project_class_name}}Records = [
    {
        id: mock{{cookiecutter.project_class_name}}Id,
        name: 'mock{{cookiecutter.project_class_name}}1',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
    },
    {
        id: 'cb8b2d40-edcc-4ac7-93ba-207408b23c8a',
        name: 'mock{{cookiecutter.project_class_name}}2',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
    }
];
const mock{{cookiecutter.project_class_name}}Update = {
    id: mock{{cookiecutter.project_class_name}}Id,
    name: 'mock{{cookiecutter.project_class_name}}1Update',
};
const mockDeletedRecord = {
    id: mock{{cookiecutter.project_class_name}}Id,
    name: 'mock{{cookiecutter.project_class_name}}1',
    isDeleted: true,
};

let mockSet: jest.Mock;
let mockGet: jest.Mock;
let mockUpdate: jest.Mock;
let mockWhere: jest.Mock;
let mockStream: jest.Mock;
let mockDoc: jest.Mock;
let mockCollection: unknown;
let mockBaseRepository: BaseRepository<any>;

describe('BaseRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
        mockSet = jest.fn().mockResolvedValue(undefined);
        mockGet = jest.fn();
        mockUpdate = jest.fn().mockResolvedValue(undefined);
        mockStream = jest.fn();
        mockWhere = jest.fn().mockReturnValue({ get: jest.fn() });
        mockDoc = jest.fn().mockReturnValue({
            set: mockSet,
            get: mockGet,
            update: mockUpdate,
        });
        mockCollection = {
            doc: mockDoc,
            where: mockWhere,
        } as unknown as CollectionReference;
        mockBaseRepository = new BaseRepository(mockCollection as CollectionReference);
    });

    describe('addRecord', () => {
        it('should successfully set document in collection', async () => {
            const result = await mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}Records[0]);
            expect(mockDoc).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
            expect(mockSet).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Records[0]);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records[0]);
        });

        it('should throw proxy error on failure', async () => {
            mockSet.mockRejectedValueOnce(new Error('mockError'));
            try {
                await mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}CreateRecord);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error creating item in database.');
            }
        });
    });

    describe('getRecord', () => {
        it('should successfully get document by id', async () => {
            mockGet.mockResolvedValue({
                exists: true,
                data: () => mock{{cookiecutter.project_class_name}}Records[0],
            });
            const result = await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            expect(mockDoc).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records[0]);
        });

        it('should throw not found error when document does not exist', async () => {
            mockGet.mockResolvedValue({ exists: false });
            try {
                await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });

        it('should throw not found error when document is soft deleted', async () => {
            mockGet.mockResolvedValue({
                exists: true,
                data: () => mockDeletedRecord,
            });
            try {
                await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });
    });

    describe('getRecords', () => {
        it('should successfully query non-deleted documents', async () => {
            const mockSnapshot = {
                docs: mock{{cookiecutter.project_class_name}}Records.map((r) => ({ data: () => r })),
            };
            mockWhere.mockReturnValue({ limit: jest.fn().mockReturnValue({ get: jest.fn().mockResolvedValue(mockSnapshot) }) });
            const result = await mockBaseRepository.getRecords();
            expect(mockWhere).toHaveBeenCalledWith('isDeleted', '==', false);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records);
        });

        it('should throw proxy error on failure', async () => {
            mockWhere.mockReturnValue({
                limit: jest.fn().mockReturnValue({
                    get: jest.fn().mockRejectedValue(new Error('Unknown error')),
                }),
            });
            try {
                await mockBaseRepository.getRecords();
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
            }
        });
    });

    describe('updateRecord', () => {
        it('should successfully update document', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}Records[0]);
            const result = await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            expect(mockDoc).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
            expect(result.name).toEqual('mock{{cookiecutter.project_class_name}}1Update');
        });

        it('should throw not found error when record does not exist', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockRejectedValue(new NotFoundError('Not found'));
            try {
                await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
            }
        });
    });

    describe('deleteRecord', () => {
        it('should successfully soft delete document', async () => {
            mockGet.mockResolvedValue({
                exists: true,
                data: () => mock{{cookiecutter.project_class_name}}Records[0],
            });
            await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            expect(mockDoc).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
            expect(mockUpdate).toHaveBeenCalledWith(
                expect.objectContaining({ isDeleted: true })
            );
        });

        it('should throw not found error when document does not exist', async () => {
            mockGet.mockResolvedValue({ exists: false });
            try {
                await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });

        it('should throw not found error when document is already deleted', async () => {
            mockGet.mockResolvedValue({
                exists: true,
                data: () => mockDeletedRecord,
            });
            try {
                await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });
    });
});
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
const mock{{cookiecutter.project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const mockTableName = 'test-table';
const mock{{cookiecutter.project_class_name}}CreateRecord = {
    id: mock{{cookiecutter.project_class_name}}Id,
    name: 'mock{{cookiecutter.project_class_name}}1',
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
};
const mock{{cookiecutter.project_class_name}}Records = [
    {
        id: mock{{cookiecutter.project_class_name}}Id,
        name: 'mock{{cookiecutter.project_class_name}}1',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
    },
    {
        id: 'cb8b2d40-edcc-4ac7-93ba-207408b23c8a',
        name: 'mock{{cookiecutter.project_class_name}}2',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
    }
];
const mock{{cookiecutter.project_class_name}}Update = {
    id: mock{{cookiecutter.project_class_name}}Id,
    name: 'mock{{cookiecutter.project_class_name}}1Update',
};
const mockDeletedRecord = {
    id: mock{{cookiecutter.project_class_name}}Id,
    name: 'mock{{cookiecutter.project_class_name}}1',
    isDeleted: true,
};
const mockDeleteUpdatedTimestamp = '2024-03-24T00:00:00.000Z';

let mockSend: jest.Mock;
let mockDocClient: unknown;
let mockBaseRepository: BaseRepository<any>;

describe('BaseRepository', () => {
    beforeEach(() => {
        jest.resetAllMocks();
        mockSend = jest.fn();
        mockDocClient = {
            send: mockSend,
        };
        mockBaseRepository = new BaseRepository(mockDocClient as any, mockTableName);
    });

    describe('addRecord', () => {
        it('should successfully put item in table', async () => {
            mockSend.mockResolvedValue({});
            const result = await mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}Records[0]);
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records[0]);
        });

        it('should throw proxy error on failure', async () => {
            mockSend.mockRejectedValueOnce(new Error('mockError'));
            try {
                await mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}CreateRecord);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error creating item in database.');
            }
        });
    });

    describe('getRecord', () => {
        it('should successfully get item by id', async () => {
            mockSend.mockResolvedValue({ Item: mock{{cookiecutter.project_class_name}}Records[0] });
            const result = await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records[0]);
        });

        it('should throw not found error when item does not exist', async () => {
            mockSend.mockResolvedValue({ Item: undefined });
            try {
                await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });

        it('should throw not found error when item is soft deleted', async () => {
            mockSend.mockResolvedValue({ Item: mockDeletedRecord });
            try {
                await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });

        it('should throw proxy error on failure', async () => {
            mockSend.mockRejectedValueOnce(new Error('Unknown error'));
            try {
                await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error retrieving item from database.');
            }
        });
    });

    describe('getRecords', () => {
        it('should successfully scan non-deleted items', async () => {
            mockSend.mockResolvedValue({ Items: mock{{cookiecutter.project_class_name}}Records });
            const result = await mockBaseRepository.getRecords();
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records);
        });

        it('should return empty array when no items found', async () => {
            mockSend.mockResolvedValue({ Items: undefined });
            const result = await mockBaseRepository.getRecords();
            expect(result).toEqual([]);
        });

        it('should throw proxy error on failure', async () => {
            mockSend.mockRejectedValueOnce(new Error('Unknown error'));
            try {
                await mockBaseRepository.getRecords();
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error retrieving items from database.');
            }
        });
    });

    describe('updateRecord', () => {
        it('should successfully get and put updated record', async () => {
            const getRecord = jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}Records[0]);
            mockSend.mockResolvedValue({});
            const result = await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            expect(getRecord).toHaveBeenCalledTimes(1);
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result.name).toEqual('mock{{cookiecutter.project_class_name}}1Update');
        });

        it('should throw not found error when record does not exist', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockRejectedValue(new NotFoundError('Not found'));
            try {
                await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
            }
        });

        it('should throw proxy error on failure', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}Records[0]);
            mockSend.mockRejectedValueOnce(new Error('mockError'));
            try {
                await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual(`Error upserting item with id ${mock{{cookiecutter.project_class_name}}Id}.`);
            }
        });
    });

    describe('deleteRecord', () => {
        it('should successfully soft delete item', async () => {
            jest.spyOn(Date.prototype, 'toISOString').mockReturnValue(mockDeleteUpdatedTimestamp);
            mockSend
                .mockResolvedValueOnce({ Item: mock{{cookiecutter.project_class_name}}Records[0] })
                .mockResolvedValueOnce({});
            await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            expect(mockSend).toHaveBeenCalledTimes(2);
        });

        it('should throw not found error when item does not exist', async () => {
            mockSend.mockResolvedValue({ Item: undefined });
            try {
                await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mock{{cookiecutter.project_class_name}}Id} not found.`);
            }
        });

        it('should throw not found error when item is already deleted', async () => {
            mockSend.mockResolvedValue({ Item: mockDeletedRecord });
            try {
                await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mock{{cookiecutter.project_class_name}}Id} not found.`);
            }
        });

        it('should throw proxy error on failure', async () => {
            mockSend.mockRejectedValueOnce(new ProxyError('mockProxyError'));
            try {
                await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual(`Error deleting record with id ${mock{{cookiecutter.project_class_name}}Id}.`);
            }
        });
    });
});
{%- endif %}
