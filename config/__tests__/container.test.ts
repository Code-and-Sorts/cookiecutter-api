import { describe, it, expect } from '@jest/globals';
import {
    kittenClawsController,
    dynamoDbClientConfig,
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
        expect(dynamoDbClientConfig.maxAttempts).toEqual(2);
        expect(dynamoDbClientConfig.requestHandler.throwOnRequestTimeout).toBe(true);
        expect(dynamoDbClientConfig.requestHandler.requestTimeout * dynamoDbClientConfig.maxAttempts).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
    });
});
