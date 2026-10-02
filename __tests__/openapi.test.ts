import { describe, it, expect, beforeAll, jest } from '@jest/globals';
import { readdirSync } from 'node:fs';
import { z } from 'zod';
import { KittenClawsRequestSchema, KittenClawsUpdateSchema } from '@models';
import spec from '../openapi.json' with { type: 'json' };
import { MockFn } from '../test/mocks';

jest.unstable_mockModule('@azure/functions', () => ({ app: { http: jest.fn<MockFn>() } }));

type Registration = { methods: string[]; route: string };
let registrations: Record<string, Registration>;

beforeAll(async () => {
    const { app } = await import('@azure/functions');
    for (const file of readdirSync(new URL('../functions', import.meta.url))) {
        if (file.endsWith('.ts')) {
            await import(`../functions/${file.slice(0, -'.ts'.length)}`);
        }
    }
    registrations = Object.fromEntries((app.http as jest.Mock<MockFn>).mock.calls.map(([name, options]) => [name, options]));
});

const registeredRoutes = (): string[] =>
    Object.values(registrations)
        .flatMap(({ methods, route }) => methods.map((method) => `${method} /${route}`))
        .sort();

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
    it('should list exactly the registered routes', () => {
        expect(registeredRoutes()).toEqual(specRoutes());
    });

    it('should have a request schema per request validator', () => {
        expect(Object.keys(specSchemas).filter((name) => name.endsWith('Request')).sort()).toEqual(Object.keys(requestSchemas).sort());
    });

    it.each(Object.entries(requestSchemas))('should describe %s as its validator does', (name, schema) => {
        expect(jsonSchema(schema)).toEqual(specSchemas[name]);
    });
});
