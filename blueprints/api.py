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
from google.cloud import firestore
from flask import Request

settings = get_settings()

def _build_cat_controller(collection):
    return CatController(CatService(CatRepository(collection)))


def _build_dog_controller(collection):
    return DogController(DogService(DogRepository(collection)))


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




def get_by_id_cat(request: Request):
    """HTTP Cloud Function to get a Cat by ID."""
    logging.info("Get cats by ID processed a request.")
    try:
        item = asyncio.run(_run("animals", _build_cat_controller, lambda c: c.get_by_id(request)))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)


def get_list_cat(request: Request):
    """HTTP Cloud Function to list Cat items."""
    logging.info("Get cats list processed a request.")
    try:
        items = asyncio.run(_run("animals", _build_cat_controller, lambda c: c.get_list(request)))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)


def create_cat(request: Request):
    """HTTP Cloud Function to create a Cat."""
    logging.info("Create cats processed a request.")
    try:
        created_item = asyncio.run(_run("animals", _build_cat_controller, lambda c: c.create(request)))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)


def update_cat(request: Request):
    """HTTP Cloud Function to patch a Cat."""
    logging.info("Patch cats processed a request.")
    try:
        updated_item = asyncio.run(_run("animals", _build_cat_controller, lambda c: c.update(request)))
        return response_generator(updated_item, 200)
    except Exception as error:
        return detect_error(error)


def delete_cat(request: Request):
    """HTTP Cloud Function to soft delete a Cat."""
    logging.info("Delete cats processed a request.")
    try:
        asyncio.run(_run("animals", _build_cat_controller, lambda c: c.soft_delete(request)))
        return ("Cat deleted.", 200)
    except Exception as error:
        return detect_error(error)



def get_by_id_dog(request: Request):
    """HTTP Cloud Function to get a Dog by ID."""
    logging.info("Get dogs by ID processed a request.")
    try:
        item = asyncio.run(_run("animals", _build_dog_controller, lambda c: c.get_by_id(request)))
        return response_generator(item)
    except Exception as error:
        return detect_error(error)


def get_list_dog(request: Request):
    """HTTP Cloud Function to list Dog items."""
    logging.info("Get dogs list processed a request.")
    try:
        items = asyncio.run(_run("animals", _build_dog_controller, lambda c: c.get_list(request)))
        return response_generator(items)
    except Exception as error:
        return detect_error(error)


def create_dog(request: Request):
    """HTTP Cloud Function to create a Dog."""
    logging.info("Create dogs processed a request.")
    try:
        created_item = asyncio.run(_run("animals", _build_dog_controller, lambda c: c.create(request)))
        return response_generator(created_item, 201)
    except Exception as error:
        return detect_error(error)


def replace_dog(request: Request):
    """HTTP Cloud Function to replace a Dog."""
    logging.info("Replace dogs processed a request.")
    try:
        replaced_item = asyncio.run(_run("animals", _build_dog_controller, lambda c: c.replace(request)))
        return response_generator(replaced_item, 200)
    except Exception as error:
        return detect_error(error)


def delete_dog(request: Request):
    """HTTP Cloud Function to soft delete a Dog."""
    logging.info("Delete dogs processed a request.")
    try:
        asyncio.run(_run("animals", _build_dog_controller, lambda c: c.soft_delete(request)))
        return ("Dog deleted.", 200)
    except Exception as error:
        return detect_error(error)

