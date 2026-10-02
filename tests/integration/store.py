"""Direct access to the emulator, to read stored records and seed containers no resource can create in."""

from datetime import datetime, timezone
from typing import Protocol
from uuid import uuid4

import boto3
import httpx
from azure.cosmos import CosmosClient
from azure.cosmos.exceptions import CosmosResourceNotFoundError

from project import Project

# The vNext emulator's gateway and Microsoft's published well-known key, the same for every language.
COSMOS_ENDPOINT = "http://localhost:8081/"
FIRESTORE_VALUE_TYPES = {str: "stringValue", bool: "booleanValue"}
COSMOS_KEY = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="


class Store(Protocol):
    def get(self, container: str, item_id: str) -> dict | None: ...

    def put(self, container: str, record: dict) -> None: ...


def timestamp() -> str:
    return datetime.now(timezone.utc).isoformat(timespec="milliseconds").replace("+00:00", "Z")


def new_record(name: str) -> dict:
    now = timestamp()
    return {"id": str(uuid4()), "name": name, "isDeleted": False, "createdTimestamp": now, "updatedTimestamp": now}


def open_store(project: Project) -> Store:
    return {"azure": CosmosStore, "gcp": FirestoreStore, "aws": DynamoStore}[project.cloud](project.env)


class CosmosStore:
    def __init__(self, env: dict[str, str]):
        self._client = CosmosClient(COSMOS_ENDPOINT, COSMOS_KEY, enable_endpoint_discovery=False)
        self._containers = {}

    def _container(self, name: str):
        # Languages name the database differently; the emulator only holds this project's.
        if name not in self._containers:
            for database in self._client.list_databases():
                database_client = self._client.get_database_client(database["id"])
                if any(c["id"] == name for c in database_client.list_containers()):
                    self._containers[name] = database_client.get_container_client(name)
                    break
            else:
                raise LookupError(f"No Cosmos DB container '{name}'; did the bootstrap run?")
        return self._containers[name]

    def get(self, container: str, item_id: str) -> dict | None:
        try:
            item = self._container(container).read_item(item_id, partition_key=item_id)
        except CosmosResourceNotFoundError:
            return None
        return {key: value for key, value in item.items() if not key.startswith("_")}

    def put(self, container: str, record: dict) -> None:
        self._container(container).upsert_item(record)


class FirestoreStore:
    def __init__(self, env: dict[str, str]):
        project = env["GCP_PROJECT_ID"]
        database = env.get("FIRESTORE_DATABASE", "(default)")
        self._client = httpx.Client(
            base_url=f"http://{env['FIRESTORE_EMULATOR_HOST']}/v1/projects/{project}/databases/{database}/documents",
            # The emulator's owner token bypasses security rules.
            headers={"Authorization": "Bearer owner"},
        )

    def get(self, container: str, item_id: str) -> dict | None:
        response = self._client.get(f"/{container}/{item_id}")
        if response.status_code == 404:
            return None
        response.raise_for_status()
        return {key: next(iter(value.values())) for key, value in response.json().get("fields", {}).items()}

    def put(self, container: str, record: dict) -> None:
        fields = {key: {FIRESTORE_VALUE_TYPES[type(value)]: value} for key, value in record.items()}
        self._client.patch(f"/{container}/{record['id']}", json={"fields": fields}).raise_for_status()


class DynamoStore:
    def __init__(self, env: dict[str, str]):
        self._dynamodb = boto3.resource(
            "dynamodb",
            endpoint_url=env["AWS_ENDPOINT_URL_DYNAMODB"],
            region_name=env["AWS_REGION"],
            aws_access_key_id=env["AWS_ACCESS_KEY_ID"],
            aws_secret_access_key=env["AWS_SECRET_ACCESS_KEY"],
        )

    def get(self, container: str, item_id: str) -> dict | None:
        return self._dynamodb.Table(container).get_item(Key={"id": item_id}).get("Item")

    def put(self, container: str, record: dict) -> None:
        self._dynamodb.Table(container).put_item(Item=record)
