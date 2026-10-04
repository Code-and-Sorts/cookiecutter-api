from functools import cache
from config import get_settings
from controllers import CatController
from services import CatService
from repositories import CatRepository
from utils.routing import event_body
from utils.user_id import user_id_from
from .database import run, session

ENDPOINT = "cats"
CONTAINER = "animals"


@cache
def _controller() -> CatController:
    settings = get_settings()
    repository = CatRepository(session, settings.tables[CONTAINER], settings.aws_region)
    return CatController(CatService(repository))


def get_list(event: dict, item_id: str | None) -> tuple[int, object]:
    return 200, run(_controller().get_list((event.get("queryStringParameters") or {}).get("limit")))


def get_by_id(event: dict, item_id: str | None) -> tuple[int, object]:
    return 200, run(_controller().get_by_id(item_id))


def create(event: dict, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(event)
    return 201, run(_controller().create(event_body(event), user_id))


def update(event: dict, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(event)
    return 200, run(_controller().update(item_id, event_body(event), user_id))


def delete(event: dict, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(event)
    return 200, run(_controller().delete(item_id, user_id))


# (HTTP method, path has an item id) -> handler
ROUTES = {
    ("GET", False): get_list,
    ("GET", True): get_by_id,
    ("POST", False): create,
    ("PATCH", True): update,
    ("DELETE", True): delete,
}
