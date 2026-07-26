import logging
from controllers import (
    KittenClawsController,
)
from services import (
    KittenClawsService,
)
from repositories import (
    KittenClawsRepository,
)
from config import get_settings
from utils import detect_error, response_generator


import asyncio
import aioboto3

settings = get_settings()
session = aioboto3.Session()
_kitten_claws_controller = KittenClawsController(KittenClawsService(KittenClawsRepository(
    session, settings.tables["kitty_cats"], settings.aws_region
)))


def health(event):
    """Lambda handler liveness probe."""
    return {
        "statusCode": 200,
        "headers": {"Content-Type": "application/json"},
        "body": '{"status": "ok"}'
    }




def get_by_id_kitten_claws(event):
    """Lambda handler to get a KittenClaws by ID."""
    logging.info("Get kitties by ID processed a request.")
    try:
        item = asyncio.run(_kitten_claws_controller.get_by_id(event))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)


def get_list_kitten_claws(event):
    """Lambda handler to list KittenClaws items."""
    logging.info("Get kitties list processed a request.")
    try:
        items = asyncio.run(_kitten_claws_controller.get_list(event))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)


def create_kitten_claws(event):
    """Lambda handler to create a KittenClaws."""
    logging.info("Create kitties processed a request.")
    try:
        created_item = asyncio.run(_kitten_claws_controller.create(event))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)


def update_kitten_claws(event):
    """Lambda handler to patch a KittenClaws."""
    logging.info("Patch kitties processed a request.")
    try:
        updated_item = asyncio.run(_kitten_claws_controller.update(event))
        return response_generator(updated_item, 200)
    except Exception as error:
        return detect_error(error)


def delete_kitten_claws(event):
    """Lambda handler to soft delete a KittenClaws."""
    logging.info("Delete kitties processed a request.")
    try:
        asyncio.run(_kitten_claws_controller.soft_delete(event))
        return {
            "statusCode": 200,
            "headers": {"Content-Type": "application/json"},
            "body": '{"message": "KittenClaws deleted."}'
        }
    except Exception as error:
        return detect_error(error)


ROUTES = {
    "kitties": {
        ("GET", False): get_list_kitten_claws,
        ("GET", True): get_by_id_kitten_claws,
        ("POST", False): create_kitten_claws,
        ("PATCH", True): update_kitten_claws,
        ("DELETE", True): delete_kitten_claws,
    },
}
