import { describe, it, expect } from '@jest/globals';
import {
    kittenClawsController,
    firestoreRetryParams,
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
        expect(firestoreRetryParams.max_rpc_timeout_millis).toEqual(DATABASE_ATTEMPT_TIMEOUT_MS);
        expect(firestoreRetryParams.total_timeout_millis).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
    });
});
