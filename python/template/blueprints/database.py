"""Database client wiring shared by every resource's functions."""
from config import get_settings
{%- if cloud_service == 'Azure Function App' %}
from azure.cosmos.aio import CosmosClient

settings = get_settings()


async def run(container_id, build_controller, operation):
    """Open a Cosmos DB client, build the controller for ``container_id`` and
    run ``operation`` against it."""
    async with CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key) as client:
        database_client = client.get_database_client(settings.cosmos_db_database_name)
        container_client = database_client.get_container_client(settings.container_names[container_id])
        return await operation(build_controller(container_client))
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
from google.cloud import firestore

settings = get_settings()


async def run(container_id, build_controller, operation):
    """Open a Firestore client, build the controller for ``container_id`` and
    run ``operation`` against it."""
    db = firestore.AsyncClient(
        project=settings.gcp_project_id,
        database=settings.firestore_database
    )
    try:
        collection = db.collection(settings.collections[container_id])
        return await operation(build_controller(collection))
    finally:
        db.close()
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
import aioboto3

settings = get_settings()
session = aioboto3.Session()
{%- endif %}
