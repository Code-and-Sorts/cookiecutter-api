import { createDefaultEsmPreset, pathsToModuleNameMapper } from 'ts-jest';
import tsconfig from './tsconfig.json' with { type: 'json' };

export default {
    ...createDefaultEsmPreset(),
    testEnvironment: 'node',
    rootDir: '.',
    testMatch: ['**/__tests__/*.test.ts'],
    setupFiles: ['<rootDir>/jest.setup.ts'],
    coveragePathIgnorePatterns: ['/node_modules/', '<rootDir>/test/'],
    coverageThreshold: {
        global: {
            statements: 85,
            branches: 75,
            functions: 70,
            lines: 90,
        },
    },
    moduleNameMapper: pathsToModuleNameMapper(tsconfig.compilerOptions.paths, { prefix: '<rootDir>/' }),
};
