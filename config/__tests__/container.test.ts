import { describe, it, expect } from '@jest/globals';
import {
    kittenClawsController,
    cosmosConnectionPolicy,
} from '@config/container';
import {
    KittenClawsController,
} from '@controllers';
import { DATABASE_ATTEMPT_TIMEOUT_MS, DATABASE_DEADLINE_MS } from '@utils';

describe('container', () => {
    it('should resolve KittenClawsController with its dependencies', () => {
        expect(kittenClawsController).toBeInstanceOf(KittenClawsController);
    });

    it('should cap database client timeouts and retries', () => {
        expect(DATABASE_DEADLINE_MS).toBeLessThan(10000);
        expect(cosmosConnectionPolicy.requestTimeout).toEqual(DATABASE_ATTEMPT_TIMEOUT_MS);
        expect(cosmosConnectionPolicy.retryOptions.maxWaitTimeInSeconds * 1000).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
    });
});
