import 'reflect-metadata';
import { BaseRepository } from '@repositories';

import { NotFoundError, ProxyError } from '@errors';



const mockKittenClawsId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
const mockTableName = 'test-table';
const mockKittenClawsCreateRecord = {
    id: mockKittenClawsId,
    name: 'mockKittenClaws1',
    createdBy: 'mockUser',
    updatedBy: 'mockUser',
};
const mockKittenClawsRecords = [
    {
        id: mockKittenClawsId,
        name: 'mockKittenClaws1',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
    },
    {
        id: 'cb8b2d40-edcc-4ac7-93ba-207408b23c8a',
        name: 'mockKittenClaws2',
        createdBy: 'mockUser',
        updatedBy: 'mockUser',
        createdTimestamp: '2024-03-24T00:00:00.000Z',
        updatedTimestamp: '2024-03-24T00:00:00.000Z',
        isDeleted: false,
    }
];
const mockKittenClawsUpdate = {
    id: mockKittenClawsId,
    name: 'mockKittenClaws1Update',
};
const mockDeletedRecord = {
    id: mockKittenClawsId,
    name: 'mockKittenClaws1',
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
            const result = await mockBaseRepository.addRecord(mockKittenClawsRecords[0]);
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockKittenClawsRecords[0]);
        });

        it('should throw proxy error on failure', async () => {
            mockSend.mockRejectedValueOnce(new Error('mockError'));
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
        it('should successfully get item by id', async () => {
            mockSend.mockResolvedValue({ Item: mockKittenClawsRecords[0] });
            const result = await mockBaseRepository.getRecord(mockKittenClawsId);
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockKittenClawsRecords[0]);
        });

        it('should throw not found error when item does not exist', async () => {
            mockSend.mockResolvedValue({ Item: undefined });
            try {
                await mockBaseRepository.getRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });

        it('should throw not found error when item is soft deleted', async () => {
            mockSend.mockResolvedValue({ Item: mockDeletedRecord });
            try {
                await mockBaseRepository.getRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });

        it('should throw proxy error on failure', async () => {
            mockSend.mockRejectedValueOnce(new Error('Unknown error'));
            try {
                await mockBaseRepository.getRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual('Error retrieving item from database.');
            }
        });
    });

    describe('getRecords', () => {
        it('should successfully scan non-deleted items', async () => {
            mockSend.mockResolvedValue({ Items: mockKittenClawsRecords });
            const result = await mockBaseRepository.getRecords();
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockKittenClawsRecords);
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
                .mockResolvedValue(mockKittenClawsRecords[0]);
            mockSend.mockResolvedValue({});
            const result = await mockBaseRepository.updateRecord(mockKittenClawsUpdate);
            expect(getRecord).toHaveBeenCalledTimes(1);
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result.name).toEqual('mockKittenClaws1Update');
        });

        it('should throw not found error when record does not exist', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockRejectedValue(new NotFoundError('Not found'));
            try {
                await mockBaseRepository.updateRecord(mockKittenClawsUpdate);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
            }
        });

        it('should throw proxy error on failure', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mockKittenClawsRecords[0]);
            mockSend.mockRejectedValueOnce(new Error('mockError'));
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
        it('should successfully get and put replaced record', async () => {
            const getRecord = jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mockKittenClawsRecords[0]);
            mockSend.mockResolvedValue({});
            const result = await mockBaseRepository.replaceRecord(mockKittenClawsRecords[0]);
            expect(getRecord).toHaveBeenCalledTimes(1);
            expect(mockSend).toHaveBeenCalledTimes(1);
            expect(result).toEqual(mockKittenClawsRecords[0]);
        });

        it('should rethrow not found error', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockRejectedValue(new NotFoundError('Not found'));
            try {
                await mockBaseRepository.replaceRecord(mockKittenClawsRecords[0]);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
            }
        });
    });

    describe('deleteRecord', () => {
        it('should successfully soft delete item', async () => {
            jest.spyOn(Date.prototype, 'toISOString').mockReturnValue(mockDeleteUpdatedTimestamp);
            mockSend
                .mockResolvedValueOnce({ Item: mockKittenClawsRecords[0] })
                .mockResolvedValueOnce({});
            await mockBaseRepository.deleteRecord(mockKittenClawsId);
            expect(mockSend).toHaveBeenCalledTimes(2);
        });

        it('should throw not found error when item does not exist', async () => {
            mockSend.mockResolvedValue({ Item: undefined });
            try {
                await mockBaseRepository.deleteRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mockKittenClawsId} not found.`);
            }
        });

        it('should throw not found error when item is already deleted', async () => {
            mockSend.mockResolvedValue({ Item: mockDeletedRecord });
            try {
                await mockBaseRepository.deleteRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
                expect(error.message).toEqual(`Record with id ${mockKittenClawsId} not found.`);
            }
        });

        it('should throw proxy error on failure', async () => {
            mockSend.mockRejectedValueOnce(new ProxyError('mockProxyError'));
            try {
                await mockBaseRepository.deleteRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(ProxyError);
                expect(error.statusCode).toEqual(502);
                expect(error.message).toEqual(`Error deleting record with id ${mockKittenClawsId}.`);
            }
        });
    });
});
