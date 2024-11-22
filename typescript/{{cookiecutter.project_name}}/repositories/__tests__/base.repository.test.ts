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
