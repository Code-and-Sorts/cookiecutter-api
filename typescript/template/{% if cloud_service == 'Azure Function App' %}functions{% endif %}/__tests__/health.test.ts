import { describe, it, expect, beforeAll, jest } from '@jest/globals';
import { HttpResponseInit } from '@azure/functions';

type MockFn = (...args: any[]) => any;

jest.unstable_mockModule('@azure/functions', () => ({ app: { http: jest.fn<MockFn>() } }));

type Registration = { methods: string[]; route: string; authLevel: string; handler: () => Promise<HttpResponseInit> };
let registrations: Record<string, Registration>;

beforeAll(async () => {
    const { app } = await import('@azure/functions');
    await import('../health');
    registrations = Object.fromEntries((app.http as jest.Mock<MockFn>).mock.calls.map(([name, options]) => [name, options]));
});

describe('health', () => {
    it('should register an anonymous health check', async () => {
        expect(registrations.health).toMatchObject({ methods: ['GET'], route: 'health', authLevel: 'anonymous' });
        expect((await registrations.health.handler()).status).toEqual(200);
    });
});
