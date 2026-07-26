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
from google.cloud import firestore
from flask import Request

settings = get_settings()

def _build_kitten_claws_controller(collection):
    return KittenClawsController(KittenClawsService(KittenClawsRepository(collection)))


async def _run(container_id, build_controller, operation):
    db = firestore.AsyncClient(
        project=settings.gcp_project_id,
        database=settings.firestore_database
    )
    try:
        collection = db.collection(settings.collections[container_id])
        return await operation(build_controller(collection))
    finally:
        db.close()



def health(request: Request):
    """HTTP Cloud Function liveness probe."""
    return ('{"status": "ok"}', 200, {'Content-Type': 'application/json'})




def get_by_id_kitten_claws(request: Request):
    """HTTP Cloud Function to get a KittenClaws by ID."""
    logging.info("Get kitties by ID processed a request.")
    try:
        item = asyncio.run(_run("kitty_cats", _build_kitten_claws_controller, lambda c: c.get_by_id(request)))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)


def get_list_kitten_claws(request: Request):
    """HTTP Cloud Function to list KittenClaws items."""
    logging.info("Get kitties list processed a request.")
    try:
        items = asyncio.run(_run("kitty_cats", _build_kitten_claws_controller, lambda c: c.get_list(request)))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)


def create_kitten_claws(request: Request):
    """HTTP Cloud Function to create a KittenClaws."""
    logging.info("Create kitties processed a request.")
    try:
        created_item = asyncio.run(_run("kitty_cats", _build_kitten_claws_controller, lambda c: c.create(request)))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)


def update_kitten_claws(request: Request):
    """HTTP Cloud Function to patch a KittenClaws."""
    logging.info("Patch kitties processed a request.")
    try:
        updated_item = asyncio.run(_run("kitty_cats", _build_kitten_claws_controller, lambda c: c.update(request)))
        return response_generator(updated_item, 200)
    except Exception as error:
        return detect_error(error)


def delete_kitten_claws(request: Request):
    """HTTP Cloud Function to soft delete a KittenClaws."""
    logging.info("Delete kitties processed a request.")
    try:
        asyncio.run(_run("kitty_cats", _build_kitten_claws_controller, lambda c: c.soft_delete(request)))
        return ("KittenClaws deleted.", 200)
    except Exception as error:
        return detect_error(error)

