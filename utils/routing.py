from collections.abc import Callable
from errors import MethodNotAllowedError, NotFoundError

# One resource's routes: (HTTP method, path has an item id) -> handler.
type Routes = dict[tuple[str, bool], Callable]


def resolve(table: dict[str, Routes], method: str, path: str) -> tuple[Callable, str | None]:
    segments = path.strip("/").split("/")
    routes = table.get(segments[0]) if len(segments) <= 2 else None
    has_id = len(segments) == 2
    if not routes or not any(route_has_id == has_id for _, route_has_id in routes):
        raise NotFoundError()
    handler = routes.get((method.upper(), has_id))
    if handler is None:
        raise MethodNotAllowedError()
    return handler, segments[1] if has_id else None
