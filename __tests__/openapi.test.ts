import { describe, it, expect, beforeAll, jest } from '@jest/globals';
import { APIGatewayProxyEvent } from 'aws-lambda';
import { z } from 'zod';
import { CatRequestSchema, CatUpdateSchema, DogRequestSchema } from '@models';
import spec from '../openapi.json' with { type: 'json' };
import { MockFn, mockController } from '../test/mocks';

jest.unstable_mockModule('@config/container', () => ({
    catController: mockController(),
    dogController: mockController(),
}));

const mockId = '91ed1c70-5412-449a-b949-80542a4eb3d5';
let handler: (event: APIGatewayProxyEvent) => Promise<{ statusCode: number; body: string }>;
let routes: Map<string, unknown>;

beforeAll(async () => {
    ({ handler, routes } = (await import('../lambda')) as unknown as { handler: typeof handler; routes: typeof routes });
});

const send = async (method: string, endpoint: string, id?: string) => {
    const body = JSON.stringify({ name: 'mockName' });
    const event = {
        httpMethod: method,
        resource: id === undefined ? `/${endpoint}` : `/${endpoint}/{id}`,
        path: id === undefined ? `/${endpoint}` : `/${endpoint}/${id}`,
        pathParameters: id === undefined ? null : { id },
        queryStringParameters: null,
        body,
        isBase64Encoded: false,
    } as unknown as APIGatewayProxyEvent;
    const result = await handler(event);
    return { status: result.statusCode, body: JSON.parse(result.body) as { errorMessage?: string } };
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
    CatCreateRequest: CatRequestSchema,
    CatUpdateRequest: CatUpdateSchema,
    DogCreateRequest: DogRequestSchema,
    DogReplaceRequest: DogRequestSchema,
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
