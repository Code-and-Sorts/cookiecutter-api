from . import (
    health,
    cat,
    dog,
    visit,
)

BLUEPRINTS = [
    health.bp,
    cat.bp,
    dog.bp,
    visit.bp,
]

__all__ = ["BLUEPRINTS"]
