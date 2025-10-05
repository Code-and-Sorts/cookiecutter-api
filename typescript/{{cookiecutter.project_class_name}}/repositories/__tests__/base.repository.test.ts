{% if cookiecutter.cloud_service == 'Azure Function App' -%}
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
const mockGetRecordsQuery = 'SELECT * FROM c WHERE c.isDeleted = false';
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
{%- elif cookiecutter.cloud_service == 'GCP Cloud Function' -%}
import 'reflect-metadata';
import { BaseRepository } from '@repositories';
import { Firestore } from '@google-cloud/firestore';
import { NotFoundError, ProxyError } from '@errors';

const mock{{cookiecutter.project_class_name}}CreateRecord = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mock{{cookiecutter.project_class_name}}1',
    {{cookiecutter.project_lower_camel_name}}GenerationData: {},
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
    isDeleted: false,
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
    }
];

const mock{{cookiecutter.project_class_name}}Update = {
    id: '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0',
    name: 'mock{{cookiecutter.project_class_name}}1Update',
};

const mock{{cookiecutter.project_class_name}}Id = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';

let mockFirestore: any;
let mockCollection: any;
let mockDoc: any;
let mockBaseRepository: BaseRepository<any>;

describe('BaseRepository - Firestore', () => {
    beforeEach(() => {
        jest.resetAllMocks();
        
        mockDoc = {
            set: jest.fn(),
            get: jest.fn(),
            update: jest.fn(),
        };
        
        mockCollection = {
            doc: jest.fn().mockReturnValue(mockDoc),
            where: jest.fn().mockReturnThis(),
            get: jest.fn(),
        };
        
        mockFirestore = {
            collection: jest.fn().mockReturnValue(mockCollection),
        } as unknown as Firestore;
        
        mockBaseRepository = new BaseRepository(mockFirestore, 'test-collection');
    });

    describe('addRecord', () => {
        it('should successfully add a record', async () => {
            mockDoc.set.mockResolvedValue(undefined);
            const result = await mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}CreateRecord);
            expect(mockCollection.doc).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}CreateRecord.id);
            expect(mockDoc.set).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}CreateRecord);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}CreateRecord);
        });

        it('should throw proxy error on failure', async () => {
            mockDoc.set.mockRejectedValue(new Error('mockError'));
            await expect(mockBaseRepository.addRecord(mock{{cookiecutter.project_class_name}}CreateRecord))
                .rejects.toThrow(ProxyError);
        });
    });

    describe('getRecord', () => {
        it('should successfully get a record by id', async () => {
            mockDoc.get.mockResolvedValue({
                exists: true,
                data: () => mock{{cookiecutter.project_class_name}}Records[0],
            });
            const result = await mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id);
            expect(mockCollection.doc).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Id);
            expect(mockDoc.get).toHaveBeenCalled();
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records[0]);
        });

        it('should throw not found error when document does not exist', async () => {
            mockDoc.get.mockResolvedValue({ exists: false });
            await expect(mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id))
                .rejects.toThrow(NotFoundError);
        });

        it('should throw not found error when document is deleted', async () => {
            mockDoc.get.mockResolvedValue({
                exists: true,
                data: () => ({ ...mock{{cookiecutter.project_class_name}}Records[0], isDeleted: true }),
            });
            await expect(mockBaseRepository.getRecord(mock{{cookiecutter.project_class_name}}Id))
                .rejects.toThrow(NotFoundError);
        });
    });

    describe('getRecords', () => {
        it('should successfully get all non-deleted records', async () => {
            const mockSnapshot = {
                forEach: (callback: any) => {
                    mock{{cookiecutter.project_class_name}}Records.forEach(record => 
                        callback({ data: () => record })
                    );
                },
            };
            mockCollection.get.mockResolvedValue(mockSnapshot);
            const result = await mockBaseRepository.getRecords();
            expect(mockCollection.where).toHaveBeenCalledWith('isDeleted', '==', false);
            expect(result).toEqual(mock{{cookiecutter.project_class_name}}Records);
        });

        it('should throw proxy error on failure', async () => {
            mockCollection.get.mockRejectedValue(new Error('mockError'));
            await expect(mockBaseRepository.getRecords()).rejects.toThrow(ProxyError);
        });
    });

    describe('updateRecord', () => {
        it('should successfully update a record', async () => {
            const getRecordSpy = jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mock{{cookiecutter.project_class_name}}Records[0]);
            mockDoc.update.mockResolvedValue(undefined);
            
            const result = await mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update);
            
            expect(getRecordSpy).toHaveBeenCalledWith(mock{{cookiecutter.project_class_name}}Update.id);
            expect(mockDoc.update).toHaveBeenCalled();
            expect(result.name).toBe(mock{{cookiecutter.project_class_name}}Update.name);
        });

        it('should throw not found error when record does not exist', async () => {
            const getRecordSpy = jest.spyOn(mockBaseRepository, 'getRecord')
                .mockRejectedValue(new NotFoundError('Not found'));
            
            await expect(mockBaseRepository.updateRecord(mock{{cookiecutter.project_class_name}}Update))
                .rejects.toThrow(NotFoundError);
        });
    });

    describe('deleteRecord', () => {
        it('should successfully soft delete a record', async () => {
            mockDoc.get.mockResolvedValue({
                exists: true,
                data: () => mock{{cookiecutter.project_class_name}}Records[0],
            });
            mockDoc.update.mockResolvedValue(undefined);
            
            await mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id);
            
            expect(mockDoc.update).toHaveBeenCalledWith({
                isDeleted: true,
                updatedTimestamp: expect.any(String),
            });
        });

        it('should throw not found error when document does not exist', async () => {
            mockDoc.get.mockResolvedValue({ exists: false });
            await expect(mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id))
                .rejects.toThrow(NotFoundError);
        });

        it('should throw not found error when document is already deleted', async () => {
            mockDoc.get.mockResolvedValue({
                exists: true,
                data: () => ({ ...mock{{cookiecutter.project_class_name}}Records[0], isDeleted: true }),
            });
            await expect(mockBaseRepository.deleteRecord(mock{{cookiecutter.project_class_name}}Id))
                .rejects.toThrow(NotFoundError);
        });
    });
});
{%- endif %}
