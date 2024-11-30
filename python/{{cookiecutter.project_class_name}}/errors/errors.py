class BaseError(Exception):
    def __init__(self, message: str):
        self.status_code: int = 500
        self.type: str = "UnknownError"
        super().__init__(message)

class NotFoundError(BaseError):
    def __init__(self, message: str = "Item Not Found."):
        super().__init__(message)
        self.status_code = 404
        self.type = "NotFoundError"

class ValidationError(BaseError):
    def __init__(self, message: str = "Validation Error."):
        super().__init__(message)
        self.status_code = 422
        self.type = "ValidationError"

class ProxyError(BaseError):
    def __init__(self, message: str = "Proxy Error."):
        super().__init__(message)
        self.status_code = 502
        self.type = "ProxyError"
