import { jest } from '@jest/globals';

export type MockFn = (...args: any[]) => any;

export type Mocked<K extends string> = Record<K, jest.Mock<MockFn>>;

export const mockController = (): Mocked<'list' | 'get' | 'post' | 'update' | 'replace' | 'delete'> => ({
    list: jest.fn<MockFn>().mockResolvedValue([]),
    get: jest.fn<MockFn>().mockResolvedValue({}),
    post: jest.fn<MockFn>().mockResolvedValue({}),
    update: jest.fn<MockFn>().mockResolvedValue({}),
    replace: jest.fn<MockFn>().mockResolvedValue({}),
    delete: jest.fn<MockFn>().mockResolvedValue({ message: 'deleted' }),
});

export const mockStore = (): Mocked<'read' | 'query' | 'create' | 'write' | 'softDelete'> => ({
    read: jest.fn<MockFn>(),
    query: jest.fn<MockFn>(),
    create: jest.fn<MockFn>(),
    write: jest.fn<MockFn>(),
    softDelete: jest.fn<MockFn>(),
});
