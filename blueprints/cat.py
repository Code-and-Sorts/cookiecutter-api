from flask import Request
from controllers import CatController
from services import CatService
from repositories import CatRepository
from utils.user_id import user_id_from
from .database import run

ENDPOINT = "cats"
CONTAINER = "animals"


def _build_controller(collection) -> CatController:
    return CatController(CatService(CatRepository(collection)))


def get_list(request: Request, item_id: str | None) -> tuple[int, object]:
    return 200, run(CONTAINER, _build_controller, lambda c: c.get_list(request.args.get("limit")))


def get_by_id(request: Request, item_id: str | None) -> tuple[int, object]:
    return 200, run(CONTAINER, _build_controller, lambda c: c.get_by_id(item_id))


def create(request: Request, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(request)
    return 201, run(CONTAINER, _build_controller, lambda c: c.create(request.get_data(), user_id))


def update(request: Request, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(request)
    return 200, run(CONTAINER, _build_controller, lambda c: c.update(item_id, request.get_data(), user_id))


def delete(request: Request, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(request)
    return 200, run(CONTAINER, _build_controller, lambda c: c.delete(item_id, user_id))


# (HTTP method, path has an item id) -> handler
ROUTES = {
    ("GET", False): get_list,
    ("GET", True): get_by_id,
    ("POST", False): create,
    ("PATCH", True): update,
    ("DELETE", True): delete,
}
