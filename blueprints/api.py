import logging
from controllers import (
    CatController,
    DogController,
)
from services import (
    CatService,
    DogService,
)
from repositories import (
    CatRepository,
    DogRepository,
)
from config import get_settings
from utils import detect_error, response_generator


import asyncio
import aioboto3

settings = get_settings()
session = aioboto3.Session()
_cat_controller = CatController(CatService(CatRepository(
    session, settings.tables["animals"], settings.aws_region
)))
_dog_controller = DogController(DogService(DogRepository(
    session, settings.tables["animals"], settings.aws_region
)))


def health(event):
    """Lambda handler liveness probe."""
    return {
        "statusCode": 200,
        "headers": {"Content-Type": "application/json"},
        "body": '{"status": "ok"}'
    }




def get_by_id_cat(event):
    """Lambda handler to get a Cat by ID."""
    logging.info("Get cats by ID processed a request.")
    try:
        item = asyncio.run(_cat_controller.get_by_id(event))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)


def get_list_cat(event):
    """Lambda handler to list Cat items."""
    logging.info("Get cats list processed a request.")
    try:
        items = asyncio.run(_cat_controller.get_list(event))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)


def create_cat(event):
    """Lambda handler to create a Cat."""
    logging.info("Create cats processed a request.")
    try:
        created_item = asyncio.run(_cat_controller.create(event))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)


def update_cat(event):
    """Lambda handler to patch a Cat."""
    logging.info("Patch cats processed a request.")
    try:
        updated_item = asyncio.run(_cat_controller.update(event))
        return response_generator(updated_item, 200)
    except Exception as error:
        return detect_error(error)


def delete_cat(event):
    """Lambda handler to soft delete a Cat."""
    logging.info("Delete cats processed a request.")
    try:
        asyncio.run(_cat_controller.soft_delete(event))
        return {
            "statusCode": 200,
            "headers": {"Content-Type": "application/json"},
            "body": '{"message": "Cat deleted."}'
        }
    except Exception as error:
        return detect_error(error)



def get_by_id_dog(event):
    """Lambda handler to get a Dog by ID."""
    logging.info("Get dogs by ID processed a request.")
    try:
        item = asyncio.run(_dog_controller.get_by_id(event))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)


def get_list_dog(event):
    """Lambda handler to list Dog items."""
    logging.info("Get dogs list processed a request.")
    try:
        items = asyncio.run(_dog_controller.get_list(event))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)


def create_dog(event):
    """Lambda handler to create a Dog."""
    logging.info("Create dogs processed a request.")
    try:
        created_item = asyncio.run(_dog_controller.create(event))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)


def replace_dog(event):
    """Lambda handler to replace a Dog."""
    logging.info("Replace dogs processed a request.")
    try:
        replaced_item = asyncio.run(_dog_controller.replace(event))
        return response_generator(replaced_item, 200)
    except Exception as error:
        return detect_error(error)


def delete_dog(event):
    """Lambda handler to soft delete a Dog."""
    logging.info("Delete dogs processed a request.")
    try:
        asyncio.run(_dog_controller.soft_delete(event))
        return {
            "statusCode": 200,
            "headers": {"Content-Type": "application/json"},
            "body": '{"message": "Dog deleted."}'
        }
    except Exception as error:
        return detect_error(error)


ROUTES = {
    "cats": {
        ("GET", False): get_list_cat,
        ("GET", True): get_by_id_cat,
        ("POST", False): create_cat,
        ("PATCH", True): update_cat,
        ("DELETE", True): delete_cat,
    },
    "dogs": {
        ("GET", False): get_list_dog,
        ("GET", True): get_by_id_dog,
        ("POST", False): create_dog,
        ("PUT", True): replace_dog,
        ("DELETE", True): delete_dog,
    },
}
