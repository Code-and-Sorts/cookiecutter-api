import { describe, it, expect } from '@jest/globals';
import {
    catController,
    dogController,
    firestoreRetryParams,
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
        expect(firestoreRetryParams.max_rpc_timeout_millis).toEqual(DATABASE_ATTEMPT_TIMEOUT_MS);
        expect(firestoreRetryParams.total_timeout_millis).toBeLessThanOrEqual(DATABASE_DEADLINE_MS);
    });
});
