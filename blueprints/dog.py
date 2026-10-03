from functools import cache
from config import get_settings
from controllers import DogController
from services import DogService
from repositories import DogRepository
from utils.routing import event_body
from utils.user_id import user_id_from
from .database import run, session

ENDPOINT = "dogs"
CONTAINER = "animals"


@cache
def _controller() -> DogController:
    settings = get_settings()
    repository = DogRepository(session, settings.tables[CONTAINER], settings.aws_region)
    return DogController(DogService(repository))


def get_list(event: dict, item_id: str | None) -> tuple[int, object]:
    return 200, run(_controller().get_list((event.get("queryStringParameters") or {}).get("limit")))


def get_by_id(event: dict, item_id: str | None) -> tuple[int, object]:
    return 200, run(_controller().get_by_id(item_id))


def create(event: dict, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(event)
    return 201, run(_controller().create(event_body(event), user_id))


def replace(event: dict, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(event)
    return 200, run(_controller().replace(item_id, event_body(event), user_id))


def delete(event: dict, item_id: str | None) -> tuple[int, object]:
    user_id = user_id_from(event)
    return 200, run(_controller().delete(item_id, user_id))


# (HTTP method, path has an item id) -> handler
ROUTES = {
    ("GET", False): get_list,
    ("GET", True): get_by_id,
    ("POST", False): create,
    ("PUT", True): replace,
    ("DELETE", True): delete,
}
