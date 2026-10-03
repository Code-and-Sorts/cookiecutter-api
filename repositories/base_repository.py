import uuid
from decimal import Decimal
from contextlib import asynccontextmanager
import aioboto3
from boto3.dynamodb.conditions import Attr
from botocore.config import Config
from botocore.exceptions import ClientError
from typing import ClassVar, List
from pydantic import BaseModel
from models import BaseEntity, generate_utc_timestamp
from errors import NotFoundError

def _user_fields(user_id: str | None, *fields: str) -> dict:
    return {field: user_id for field in fields} if user_id else {}


def _without_nulls(record: dict) -> dict:
    """A field without a value is not stored, in every language and database."""
    return {key: value for key, value in record.items() if value is not None}

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


class BaseRepository[ResponseT: BaseModel]:
    """Protected so each resource repository exposes only its enabled operations."""

    resource_name: ClassVar[str]
    entity_model: ClassVar[type[BaseEntity]]
    response_model: ClassVar[type[BaseModel]]

    def __init__(self, session: aioboto3.Session, table_name: str, region: str):
        self.session = session
        self.table_name = table_name
        self.region = region

    @asynccontextmanager
    async def _table(self):
        async with self.session.resource("dynamodb", region_name=self.region, config=DYNAMODB_CONFIG) as dynamodb:
            yield await dynamodb.Table(self.table_name)

    def _not_found(self, item_id: str) -> NotFoundError:
        return NotFoundError.for_item(self.resource_name, item_id)

    async def _get_stored(self, item_id: str) -> dict:
        async with self._table() as table:
            response = await table.get_item(Key={"id": item_id})
        item = response.get("Item")

        if not item or item.get("isDeleted", False):
            raise self._not_found(item_id)

        return from_dynamodb(item)

    async def _write(self, record: dict) -> None:
        async with self._table() as table:
            await table.put_item(Item=to_dynamodb(record))

    async def _get_by_id(self, item_id: str) -> ResponseT:
        return self.response_model.model_validate(await self._get_stored(item_id))

    async def _get_list(self, limit: int) -> List[ResponseT]:
        limit = int(limit)
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
