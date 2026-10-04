import uuid
from azure.core import MatchConditions
from azure.cosmos.aio import ContainerProxy
from azure.cosmos.exceptions import CosmosAccessConditionFailedError, CosmosResourceNotFoundError
from typing import ClassVar, List
from models import BaseResponse, generate_utc_timestamp
from errors import NotFoundError

_CREATION_FIELDS = ("createdTimestamp", "createdBy")


def _user_fields(user_id: str | None, *fields: str) -> dict:
    return {field: user_id for field in fields} if user_id else {}


class BaseRepository[ResponseT: BaseResponse]:
    """Protected so each resource repository exposes only its enabled operations."""

    resource_name: ClassVar[str]
    response_model: ClassVar[type[BaseResponse]]

    def __init__(self, container_client: ContainerProxy):
        self.container_client = container_client

    def _not_found(self, item_id: str) -> NotFoundError:
        return NotFoundError.for_item(self.resource_name, item_id)

    async def _get_stored(self, item_id: str) -> dict:
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

    async def _write(self, record: dict, read: dict | None = None) -> None:
        """read is the record the write was built from, so a store can refuse a write that races another."""
        if read is None:
            await self.container_client.upsert_item(record)
        else:
            await self.container_client.replace_item(
                record["id"], record, etag=read["_etag"], match_condition=MatchConditions.IfNotModified
            )

    async def _get_by_id(self, item_id: str) -> ResponseT:
        return self.response_model.model_validate(await self._get_stored(item_id))

    async def _get_list(self, limit: int) -> List[ResponseT]:
        limit = int(limit)
        query = f"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {limit}"
        items = [
            self.response_model.model_validate(item)
            async for item in self.container_client.query_items(query=query)
        ]
        return items

    async def _create(self, fields: dict, user_id: str | None = None) -> ResponseT:
        now = generate_utc_timestamp()
        record = {
            "id": str(uuid.uuid4()),
            **fields,
            "isDeleted": False,
            "createdTimestamp": now,
            "updatedTimestamp": now,
            **_user_fields(user_id, "createdBy", "updatedBy"),
        }
        await self._write(record)
        return self.response_model.model_validate(record)

    async def _update(self, item_id: str, changes: dict, user_id: str | None = None) -> ResponseT:
        stored = await self._get_stored(item_id)
        stored.pop("updatedBy", None)
        record = {
            **stored,
            **changes,
            "id": item_id,
            "updatedTimestamp": generate_utc_timestamp(),
            **_user_fields(user_id, "updatedBy"),
        }
        await self._write(record, stored)
        return self.response_model.model_validate(record)

    async def _replace(self, item_id: str, fields: dict, user_id: str | None = None) -> ResponseT:
        stored = await self._get_stored(item_id)
        now = generate_utc_timestamp()
        record = {
            "id": item_id,
            **fields,
            "isDeleted": False,
            "createdTimestamp": now,
            **{field: stored[field] for field in _CREATION_FIELDS if stored.get(field)},
            "updatedTimestamp": now,
            **_user_fields(user_id, "updatedBy"),
        }
        await self._write(record, stored)
        return self.response_model.model_validate(record)

    async def _delete(self, item_id: str, user_id: str | None = None) -> None:
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
