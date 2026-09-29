export class BaseError extends Error {
    public statusCode: number | undefined;

    constructor(message: string, asserter?: Function) {
        super(message);
        Object.setPrototypeOf(this, new.target.prototype); // restore prototype chain
        Error.captureStackTrace?.(this, asserter || this.constructor);
    }
}
