import { describe, it, expect } from '@jest/globals';
import {
    kittenClawsController,
} from '@config/container';
import {
    KittenClawsController,
} from '@controllers';

describe('container', () => {
    it('should resolve KittenClawsController with its dependencies', () => {
        expect(kittenClawsController).toBeInstanceOf(KittenClawsController);
    });
});
