import uuid
{%- if cloud_service == 'AWS Lambda' %}
from decimal import Decimal
{%- endif %}
{%- if cloud_service == 'Azure Function App' %}
from azure.cosmos.aio import ContainerProxy
from azure.cosmos.exceptions import CosmosAccessConditionFailedError, CosmosResourceNotFoundError
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
from google.api_core.retry_async import AsyncRetry
from google.cloud.firestore import DELETE_FIELD, AsyncCollectionReference, FieldFilter
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
from contextlib import asynccontextmanager
import aioboto3
from boto3.dynamodb.conditions import Attr
from botocore.config import Config
from botocore.exceptions import ClientError
{%- endif %}
from typing import ClassVar, List
from pydantic import BaseModel
from models import BaseEntity, generate_utc_timestamp
from errors import NotFoundError

def _user_fields(user_id: str | None, *fields: str) -> dict:
    return {field: user_id for field in fields} if user_id else {}


def _without_nulls(record: dict) -> dict:
    """A field without a value is not stored, in every language and database."""
    return {key: value for key, value in record.items() if value is not None}
{%- if cloud_service == 'GCP Cloud Function' %}

# The SDK retries for up to 300 s by default; keep each call inside the request deadline.
FIRESTORE_CALL_OPTIONS = {
    "retry": AsyncRetry(initial=0.1, maximum=1.0, multiplier=2.0, timeout=5.0),
    "timeout": 3.0,
}
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

# Keep each DynamoDB call inside the request deadline.
DYNAMODB_CONFIG = Config(
    connect_timeout=1,
    read_timeout=2,
    retries={"total_max_attempts": 2, "mode": "standard"},
)


def to_dynamodb(value):
    """The boto3 resource rejects float, so numbers travel as Decimal."""
    if isinstance(value, float):
        return Decimal(str(value))
    if isinstance(value, list):
        return [to_dynamodb(item) for item in value]
    if isinstance(value, dict):
        return {key: to_dynamodb(item) for key, item in value.items()}
    return value


def from_dynamodb(value):
    if isinstance(value, Decimal):
        return int(value) if value == value.to_integral_value() else float(value)
    if isinstance(value, list):
        return [from_dynamodb(item) for item in value]
    if isinstance(value, dict):
        return {key: from_dynamodb(item) for key, item in value.items()}
    return value
{%- endif %}


class BaseRepository[ResponseT: BaseModel]:
    """Protected so each resource repository exposes only its enabled operations."""

    resource_name: ClassVar[str]
    entity_model: ClassVar[type[BaseEntity]]
    response_model: ClassVar[type[BaseModel]]
{%- if cloud_service == 'Azure Function App' %}

    def __init__(self, container_client: ContainerProxy):
        self.container_client = container_client
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}

    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}

    def __init__(self, session: aioboto3.Session, table_name: str, region: str):
        self.session = session
        self.table_name = table_name
        self.region = region

    @asynccontextmanager
    async def _table(self):
        async with self.session.resource("dynamodb", region_name=self.region, config=DYNAMODB_CONFIG) as dynamodb:
            yield await dynamodb.Table(self.table_name)
{%- endif %}

    def _not_found(self, item_id: str) -> NotFoundError:
        return NotFoundError.for_item(self.resource_name, item_id)

    async def _get_stored(self, item_id: str) -> dict:
{%- if cloud_service == 'Azure Function App' %}
        query = "SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false"
        parameters = [
            { "name": "@id", "value": item_id }
        ]
        async for item in self.container_client.query_items(
            query=query,
            parameters=parameters
        ):
            return item

        raise self._not_found(item_id)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        doc = await self.collection.document(item_id).get(**FIRESTORE_CALL_OPTIONS)
        if not doc.exists:
            raise self._not_found(item_id)

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise self._not_found(item_id)

        return data
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        async with self._table() as table:
            response = await table.get_item(Key={"id": item_id})
        item = response.get("Item")

        if not item or item.get("isDeleted", False):
            raise self._not_found(item_id)

        return from_dynamodb(item)
{%- endif %}

    async def _write(self, record: dict) -> None:
{%- if cloud_service == 'Azure Function App' %}
        await self.container_client.upsert_item(record)
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        await self.collection.document(record["id"]).set(record, **FIRESTORE_CALL_OPTIONS)
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        async with self._table() as table:
            await table.put_item(Item=to_dynamodb(record))
{%- endif %}

    async def _get_by_id(self, item_id: str) -> ResponseT:
        return self.response_model.model_validate(await self._get_stored(item_id))

    async def _get_list(self, limit: int) -> List[ResponseT]:
        limit = int(limit)
{%- if cloud_service == 'Azure Function App' %}
        query = f"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {limit}"
        items = [
            self.response_model.model_validate(item)
            async for item in self.container_client.query_items(query=query)
        ]
        return items
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        query = self.collection.where(filter=FieldFilter("isDeleted", "==", False)).limit(limit)
        items = []
        async for doc in query.stream(**FIRESTORE_CALL_OPTIONS):
            items.append(self.response_model.model_validate(doc.to_dict()))
        return items
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        # Scan's Limit counts items before the filter applies, so page until enough are found.
        items = []
        filter_exp = Attr("isDeleted").eq(False) | Attr("isDeleted").not_exists()
        async with self._table() as table:
            response = await table.scan(FilterExpression=filter_exp, Limit=limit)
            items.extend(response.get("Items", []))

            while "LastEvaluatedKey" in response and len(items) < limit:
                response = await table.scan(
                    FilterExpression=filter_exp,
                    Limit=limit,
                    ExclusiveStartKey=response["LastEvaluatedKey"]
                )
                items.extend(response.get("Items", []))

        return [self.response_model.model_validate(from_dynamodb(item)) for item in items[:limit]]
{%- endif %}

    async def _create(self, fields: dict, user_id: str | None = None) -> ResponseT:
        """fields holds the body the create accepts; every other client field gets its default."""
        now = generate_utc_timestamp()
        record = _without_nulls({
            **self.entity_model.client_defaults(),
            **fields,
            "id": str(uuid.uuid4()),
            "isDeleted": False,
            "createdTimestamp": now,
            "updatedTimestamp": now,
            **_user_fields(user_id, "createdBy", "updatedBy"),
        })
        await self._write(record)
        return self.response_model.model_validate(record)

    async def _update(self, item_id: str, changes: dict, user_id: str | None = None) -> ResponseT:
        """Merges changes onto the stored record, so fields other resources in the container store are kept."""
        stored = await self._get_stored(item_id)
        stored.pop("updatedBy", None)
        record = _without_nulls({
            **stored,
            **changes,
            "id": item_id,
            "updatedTimestamp": generate_utc_timestamp(),
            **_user_fields(user_id, "updatedBy"),
        })
        await self._write(record)
        return self.response_model.model_validate(record)

    async def _delete(self, item_id: str, user_id: str | None = None) -> None:
{%- if cloud_service == 'Azure Function App' %}
        operations: list[dict] = [
            { 'op': 'set', 'path': '/isDeleted', 'value': True },
            { 'op': 'set', 'path': '/updatedTimestamp', 'value': generate_utc_timestamp() },
            # Remove fails on a missing path, so an anonymous delete sets updatedBy first.
            { 'op': 'set', 'path': '/updatedBy', 'value': user_id or "" },
        ]
        if not user_id:
            operations.append({ 'op': 'remove', 'path': '/updatedBy' })
        try:
            await self.container_client.patch_item(
                item=item_id,
                partition_key=item_id,
                patch_operations=operations,
                filter_predicate="from c WHERE c.isDeleted = false"
            )
        except CosmosAccessConditionFailedError:
            # 412: the filter predicate failed, so the item is already deleted.
            raise self._not_found(item_id) from None
        except CosmosResourceNotFoundError as error:
            # A missing database/container is a 500: it has a sub-status, or read() raises (emulator).
            if error.sub_status:
                raise
            await self.container_client.read()
            raise self._not_found(item_id) from None
{%- endif %}
{%- if cloud_service == 'GCP Cloud Function' %}
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get(**FIRESTORE_CALL_OPTIONS)

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise self._not_found(item_id)

        await doc_ref.update(
            {'isDeleted': True, 'updatedTimestamp': generate_utc_timestamp(), 'updatedBy': user_id or DELETE_FIELD},
            **FIRESTORE_CALL_OPTIONS
        )
{%- endif %}
{%- if cloud_service == 'AWS Lambda' %}
        update = "SET isDeleted = :val, updatedTimestamp = :updated"
        values = {":val": True, ":false": False, ":updated": generate_utc_timestamp()}
        if user_id:
            update += ", updatedBy = :user"
            values[":user"] = user_id
        else:
            update += " REMOVE updatedBy"
        try:
            async with self._table() as table:
                await table.update_item(
                    Key={"id": item_id},
                    UpdateExpression=update,
                    ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                    ExpressionAttributeValues=values
                )
        except ClientError as error:
            if error.response["Error"]["Code"] == "ConditionalCheckFailedException":
                raise self._not_found(item_id) from None
            raise
{%- endif %}
