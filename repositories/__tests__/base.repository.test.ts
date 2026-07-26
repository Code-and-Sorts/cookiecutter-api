import 'reflect-metadata';
import { BaseRepository } from '@repositories';

import { CollectionReference } from '@google-cloud/firestore';
import { NotFoundError, ProxyError } from '@errors';



const mockKittenClawsId = '28535ae3-2f1b-4e81-ba13-0f46a0c74ea0';
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
            const result = await mockBaseRepository.addRecord(mockKittenClawsRecords[0]);
            expect(mockDoc).toHaveBeenCalledWith(mockKittenClawsId);
            expect(mockSet).toHaveBeenCalledWith(mockKittenClawsRecords[0]);
            expect(result).toEqual(mockKittenClawsRecords[0]);
        });

        it('should throw proxy error on failure', async () => {
            mockSet.mockRejectedValueOnce(new Error('mockError'));
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
        it('should successfully get document by id', async () => {
            mockGet.mockResolvedValue({
                exists: true,
                data: () => mockKittenClawsRecords[0],
            });
            const result = await mockBaseRepository.getRecord(mockKittenClawsId);
            expect(mockDoc).toHaveBeenCalledWith(mockKittenClawsId);
            expect(result).toEqual(mockKittenClawsRecords[0]);
        });

        it('should throw not found error when document does not exist', async () => {
            mockGet.mockResolvedValue({ exists: false });
            try {
                await mockBaseRepository.getRecord(mockKittenClawsId);
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
                await mockBaseRepository.getRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });
    });

    describe('getRecords', () => {
        it('should successfully query non-deleted documents', async () => {
            const mockSnapshot = {
                docs: mockKittenClawsRecords.map((r) => ({ data: () => r })),
            };
            mockWhere.mockReturnValue({ limit: jest.fn().mockReturnValue({ get: jest.fn().mockResolvedValue(mockSnapshot) }) });
            const result = await mockBaseRepository.getRecords();
            expect(mockWhere).toHaveBeenCalledWith('isDeleted', '==', false);
            expect(result).toEqual(mockKittenClawsRecords);
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
                .mockResolvedValue(mockKittenClawsRecords[0]);
            const result = await mockBaseRepository.updateRecord(mockKittenClawsUpdate);
            expect(mockDoc).toHaveBeenCalledWith(mockKittenClawsId);
            expect(mockUpdate).toHaveBeenCalledTimes(1);
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
    });

    describe('replaceRecord', () => {
        it('should successfully replace document', async () => {
            jest.spyOn(mockBaseRepository, 'getRecord')
                .mockResolvedValue(mockKittenClawsRecords[0]);
            const result = await mockBaseRepository.replaceRecord(mockKittenClawsRecords[0]);
            expect(mockDoc).toHaveBeenCalledWith(mockKittenClawsId);
            expect(mockSet).toHaveBeenCalledTimes(1);
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
        it('should successfully soft delete document', async () => {
            mockGet.mockResolvedValue({
                exists: true,
                data: () => mockKittenClawsRecords[0],
            });
            await mockBaseRepository.deleteRecord(mockKittenClawsId);
            expect(mockDoc).toHaveBeenCalledWith(mockKittenClawsId);
            expect(mockUpdate).toHaveBeenCalledWith(
                expect.objectContaining({ isDeleted: true })
            );
        });

        it('should throw not found error when document does not exist', async () => {
            mockGet.mockResolvedValue({ exists: false });
            try {
                await mockBaseRepository.deleteRecord(mockKittenClawsId);
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
                await mockBaseRepository.deleteRecord(mockKittenClawsId);
            } catch (error) {
                expect(error).toBeInstanceOf(NotFoundError);
                expect(error.statusCode).toEqual(404);
            }
        });
    });
});
