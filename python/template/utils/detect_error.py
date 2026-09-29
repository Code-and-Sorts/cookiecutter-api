import logging
from errors import BaseError
from .response_generator import response_generator

logger = logging.getLogger(__name__)

UNEXPECTED_ERROR_MESSAGE = "An unexpected error occurred."


def detect_error(error: Exception):
    """Turn ``error`` into a JSON ``{"errorMessage": ...}`` response.

    Expected errors (4xx) answer with their own message and are not logged as
    errors. Anything else is logged with its stack trace and answered with a
    generic 500, so no exception text or SDK details reach the client.
    """
    if isinstance(error, BaseError) and error.status_code < 500:
        return response_generator({"errorMessage": str(error)}, error.status_code)

    logger.error("Unexpected error while handling the request.", exc_info=error)
    return response_generator({"errorMessage": UNEXPECTED_ERROR_MESSAGE}, 500)
