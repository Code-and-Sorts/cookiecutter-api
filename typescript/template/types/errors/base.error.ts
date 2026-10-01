export class BaseError extends Error {
    public statusCode: number | undefined;

    constructor(message: string, asserter?: Function, options?: ErrorOptions) {
        super(message, options);
        Object.setPrototypeOf(this, new.target.prototype); // restore prototype chain
        this.name = new.target.name; // named before capture so logged stacks read "NotFoundError: ..."
        Error.captureStackTrace?.(this, asserter || this.constructor);
    }
}
