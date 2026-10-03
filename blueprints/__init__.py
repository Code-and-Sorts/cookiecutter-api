from . import (
    health,
    cat,
    dog,
)

BLUEPRINTS = [
    health.bp,
    cat.bp,
    dog.bp,
]

__all__ = ["BLUEPRINTS"]
