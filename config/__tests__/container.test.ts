import { describe, it, expect } from '@jest/globals';
import {
    catController,
    dogController,
} from '@config/container';
import {
    CatController,
    DogController,
} from '@controllers';

describe('container', () => {
    it('should resolve CatController with its dependencies', () => {
        expect(catController).toBeInstanceOf(CatController);
    });
    it('should resolve DogController with its dependencies', () => {
        expect(dogController).toBeInstanceOf(DogController);
    });
});
