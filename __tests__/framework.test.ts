import { describe, it, expect, beforeAll, afterAll, jest } from '@jest/globals';
import type { Server } from 'node:http';
import type { AddressInfo } from 'node:net';
import { mockController } from '../test/mocks';

jest.unstable_mockModule('@config/container', () => ({
    catController: mockController(),
    dogController: mockController(),
}));

// Uses the real framework server so bodies pass through its Express parsers, as when deployed.
let server: Server;
let baseUrl: string;

beforeAll(async () => {
    await import('../main');
    const { getTestServer } = await import('@google-cloud/functions-framework/testing');
    server = getTestServer('api');
    await new Promise<void>((resolve) => server.listen(0, '127.0.0.1', resolve));
    baseUrl = `http://127.0.0.1:${(server.address() as AddressInfo).port}`;
});

afterAll(async () => {
    await new Promise<void>((resolve) => server.close(() => resolve()));
});

const expectJson = async (response: Response, status: number, body: unknown) => {
    expect(response.status).toEqual(status);
    expect(response.headers.get('content-type')).toMatch(/^application\/json/);
    expect(await response.json()).toEqual(body);
};

describe('Functions Framework server', () => {
    it('should answer malformed JSON with a JSON 400 before the function runs', async () => {
        const response = await fetch(`${baseUrl}/cats`, {
            method: 'POST',
            headers: { 'content-type': 'application/json' },
            body: '{bad',
        });
        await expectJson(response, 400, { errorMessage: 'Request body must be valid JSON.' });
    });

    it('should answer an unknown path with a JSON 404', async () => {
        await expectJson(await fetch(`${baseUrl}/unknown`), 404, { errorMessage: 'Not found.' });
    });

    it('should answer the health check', async () => {
        await expectJson(await fetch(`${baseUrl}/health`), 200, { status: 'ok' });
    });
});
