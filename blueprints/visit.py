from flask import Request
from controllers import VisitController
from services import VisitService
from repositories import VisitRepository
from utils.user_id import user_id_from
from .database import run

ENDPOINT = "visits"
CONTAINER = "visits"


def _build_controller(collection) -> VisitController:
    return VisitController(VisitService(VisitRepository(collection)))


def get_by_id(request: Request, item_id: str | None) -> tuple[int, object]:
    return 200, run(CONTAINER, _build_controller, lambda c: c.get_by_id(item_id))


def create(request: Request, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(request)
    return 201, run(CONTAINER, _build_controller, lambda c: c.create(request.get_data(), user_id))


def update(request: Request, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(request)
    return 200, run(CONTAINER, _build_controller, lambda c: c.update(item_id, request.get_data(), user_id))


# (HTTP method, path has an item id) -> handler
ROUTES = {
    ("GET", True): get_by_id,
    ("POST", False): create,
    ("PATCH", True): update,
}
