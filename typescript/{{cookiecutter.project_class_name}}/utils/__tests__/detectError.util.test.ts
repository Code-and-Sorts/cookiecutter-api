import { NotFoundError, ProxyError, ValidationError } from '@errors';
import { detectError } from '@utils';

describe('detectError', () => {
    beforeEach(() => {
        jest.resetAllMocks();
    });

    const mockValidationErrorResponse = {
        status: 422,
        body: 'Validation error',
    };
    const mockNotFoundErrorResponse = {
        status: 404,
        body: 'Not found error',
    };
    const mockProxyErrorResponse = {
        status: 502,
        body: 'Proxy error',
    };
    const mockUnknownErrorResponse = {
        status: 500,
        body: 'Unknown error occurred.',
    };

    it('should successfully return error response for ValidationError', () => {
        const response = detectError(new ValidationError('Validation error'));
        expect(response).toEqual(mockValidationErrorResponse);
    });

    it('should successfully return error response for ProxyError', () => {
        const response = detectError(new ProxyError('Proxy error'));
        expect(response).toEqual(mockProxyErrorResponse);
    });

    it('should successfully return error response for NotFoundError', () => {
        const response = detectError(new NotFoundError('Not found error'));
        expect(response).toEqual(mockNotFoundErrorResponse);
    });

    it('should successfully return error response for unknown error', () => {
        const response = detectError(new Error('Unknown error'));
        expect(response).toEqual(mockUnknownErrorResponse);
    });
});
