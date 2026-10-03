import { describe, it, expect } from '@jest/globals';
import {
    catController,
    dogController,
    cosmosClientOptions,
    cosmosConnectionPolicy,
} from '@config/container';
import {
    CatController,
    DogController,
} from '@controllers';
import { DATABASE_ATTEMPT_TIMEOUT_MS, DATABASE_DEADLINE_MS } from '@utils';

describe('container', () => {
    it('should resolve CatController with its dependencies', () => {
        expect(catController).toBeInstanceOf(CatController);
    });
    it('should resolve DogController with its dependencies', () => {
        expect(dogController).toBeInstanceOf(DogController);
    });

    it('should cap database client timeouts and retries', () => {
        expect(DATABASE_DEADLINE_MS).toBeLessThan(10000);
        expect(cosmosConnectionPolicy.requestTimeout).toEqual(DATABASE_ATTEMPT_TIMEOUT_MS);
        expect(cosmosConnectionPolicy.retryOptions.maxWaitTimeInSeconds * 1000).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
    });

    describe('cosmosClientOptions', () => {
        const settings = (url: string, emulator: boolean) => ({ COSMOS_DB_URL: url, COSMOS_DB_KEY: 'key', COSMOS_DB_EMULATOR: emulator });

        it('should configure the client as in production without the emulator flag', () => {
            const options = cosmosClientOptions(settings('https://account.documents.azure.com:443/', false));

            expect(options).toEqual({ endpoint: 'https://account.documents.azure.com:443/', key: 'key', connectionPolicy: cosmosConnectionPolicy });
        });

        it('should keep the configured endpoint for the emulator', () => {
            const options = cosmosClientOptions(settings('http://localhost:8081/', true));

            expect(options.connectionPolicy).toEqual({ ...cosmosConnectionPolicy, enableEndpointDiscovery: false });
            expect(options.agent).toBeUndefined();
        });

        it('should skip certificate checks only for an https emulator', () => {
            const options = cosmosClientOptions(settings('https://localhost:8081/', true));

            expect((options.agent as unknown as { options: { rejectUnauthorized: boolean } }).options.rejectUnauthorized).toBe(false);
        });
    });
});
