from . import (
    health,
    kitten_claws,
)

# endpoint (first path segment) -> (HTTP method, path has an item id) -> handler
ROUTES = {
    module.ENDPOINT: module.ROUTES
    for module in (
        health,
        kitten_claws,
    )
}

__all__ = ["ROUTES"]
