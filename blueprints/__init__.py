from . import (
    health,
    cat,
    dog,
)

# endpoint (first path segment) -> (HTTP method, path has an item id) -> handler
ROUTES = {
    module.ENDPOINT: module.ROUTES
    for module in (
        health,
        cat,
        dog,
    )
}

__all__ = ["ROUTES"]
