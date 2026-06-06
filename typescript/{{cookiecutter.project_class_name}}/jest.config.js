const { pathsToModuleNameMapper } = require('ts-jest');
const { compilerOptions } = require('./tsconfig');

module.exports = {
    preset: 'ts-jest',
    testEnvironment: 'node',
    rootDir: '.',
    testMatch: ['**/__tests__/*.test.ts'],
    coveragePathIgnorePatterns: ['/node_modules/'],
    coverageThreshold: {
        global: {
            statements: 85,
            branches: 75,
            functions: 70,
            lines: 90,
        },
    },
    moduleNameMapper: pathsToModuleNameMapper(compilerOptions.paths),
    modulePaths: ['<rootDir>']
};
