import { describe, it, expect, beforeAll, jest } from '@jest/globals';
import { EventEmitter } from 'node:events';
import { z } from 'zod';
import { KittenClawsRequestSchema, KittenClawsUpdateSchema } from '@models';
import spec from '../openapi.json' with { type: 'json' };
import { MockFn, mockController } from '../test/mocks';

jest.unstable_mockModule('@google-cloud/functions-framework', () => ({ http: jest.fn<MockFn>() }));
jest.unstable_mockModule('@config/container', () => ({
    kittenClawsController: mockController(),
}));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
let api: (req: unknown, res: unknown) => Promise<void>;
let routes: Map<string, unknown>;

beforeAll(async () => {
    ({ api, routes } = (await import('../main')) as unknown as { api: typeof api; routes: typeof routes });
});

const send = async (method: string, endpoint: string, id?: string) => {
    const body = JSON.stringify({ name: 'mockName' });
    const res = Object.assign(new EventEmitter(), {
        headersSent: false,
        writableEnded: false,
        status: jest.fn<MockFn>().mockReturnThis(),
        json: jest.fn<MockFn>().mockReturnThis(),
    });
    const path = id === undefined ? `/${endpoint}` : `/${endpoint}/${id}`;
    await api({ method, path, rawBody: Buffer.from(body), query: {}, headers: {} }, res);
    return { status: res.status.mock.calls[0][0] as number, body: res.json.mock.calls[0][0] as { errorMessage?: string } };
};

const registeredRoutes = async (): Promise<string[]> => {
    const found: string[] = [];
    for (const endpoint of routes.keys()) {
        for (const id of [undefined, mockId]) {
            for (const method of ['GET', 'POST', 'PUT', 'PATCH', 'DELETE']) {
                const { status, body } = await send(method, endpoint, id);
                if (status !== 405 && !(status === 404 && body.errorMessage === 'Not found.')) {
                    found.push(`${method} /${endpoint}${id === undefined ? '' : '/{id}'}`);
                }
            }
        }
    }
    return found.sort();
};

const specRoutes = (): string[] =>
    Object.entries(spec.paths as Record<string, Record<string, unknown>>)
        .flatMap(([path, item]) =>
            Object.keys(item)
                .filter((key) => key !== 'parameters')
                .map((method) => `${method.toUpperCase()} ${path}`),
        )
        .sort();

const specSchemas = spec.components.schemas as Record<string, unknown>;

const requestSchemas: Record<string, z.ZodType> = {
    KittenClawsCreateRequest: KittenClawsRequestSchema,
    KittenClawsUpdateRequest: KittenClawsUpdateSchema,
};

const jsonSchema = (schema: z.ZodType): Record<string, unknown> => {
    const { $schema, ...rest } = z.toJSONSchema(schema) as Record<string, unknown>;
    return rest;
};

describe('openapi.json', () => {
    it('should list exactly the registered routes', async () => {
        expect(await registeredRoutes()).toEqual(specRoutes());
    });

    it('should have a request schema per request validator', () => {
        expect(Object.keys(specSchemas).filter((name) => name.endsWith('Request')).sort()).toEqual(Object.keys(requestSchemas).sort());
    });

    it.each(Object.entries(requestSchemas))('should describe %s as its validator does', (name, schema) => {
        expect(jsonSchema(schema)).toEqual(specSchemas[name]);
    });
});
