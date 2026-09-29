from azure.cosmos.aio import ContainerProxy
from azure.cosmos.exceptions import CosmosAccessConditionFailedError


from typing import List
from models import (
    generate_utc_timestamp,
    BaseCat,
    Cat,
    CatResponse,
    BaseDog,
    Dog,
    DogResponse,
)
from errors import NotFoundError

# Default cap on the number of items returned by list endpoints to avoid
# unbounded reads. Callers may request a smaller page via the `limit` argument.
DEFAULT_LIST_LIMIT = 100

class Database:
    def __init__(
            self,
            endpoint: str,
            key: str,
            name: str,
            container_name: str
        ):
            self._endpoint = endpoint
            self._key = key
            self.name = name
            self.container_name = container_name

class CatRepository:
    def __init__(self, container_client: ContainerProxy):
        self.container_client = container_client

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

        raise NotFoundError()

    async def get_by_id(self, item_id: str) -> CatResponse:
        return CatResponse.model_validate(await self._get_stored(item_id))

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[CatResponse | None]:
        query = f"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {int(limit)}"
        items = [
            CatResponse.model_validate(item)
            async for item in self.container_client.query_items(query=query)
        ]
        return items

    async def create(self, item: Cat) -> CatResponse:
        now = generate_utc_timestamp()
        item_dict = {
            **item.model_dump(exclude_none=True),
            "isDeleted": False,
            "createdDate": now,
            "updatedDate": now,
        }
        created_item = await self.container_client.create_item(item_dict)

        return CatResponse.model_validate(created_item)

    async def update(self, item: Cat) -> CatResponse:
        stored_item = await self._get_stored(item.id)
        changes = item.model_dump(include=set(BaseCat.model_fields), exclude_unset=True)
        patched_item = {
            **stored_item,
            **changes,
            "id": item.id,
            "updatedDate": generate_utc_timestamp(),
        }
        updated_item = await self.container_client.upsert_item(patched_item)

        return CatResponse.model_validate(updated_item)

    async def replace(self, item: Cat) -> CatResponse:
        stored_item = await self._get_stored(item.id)
        now = generate_utc_timestamp()
        replacement = {
            **item.model_dump(include=set(BaseCat.model_fields), exclude_none=True),
            "id": item.id,
            "isDeleted": False,
            "createdDate": stored_item.get("createdDate", now),
            "updatedDate": now,
        }
        replaced_item = await self.container_client.upsert_item(replacement)

        return CatResponse.model_validate(replaced_item)

    async def delete(self, item_id: str):
        filter = "from c WHERE c.isDeleted = false"
        operations: list[dict] = [
            { 'op': 'replace', 'path': '/isDeleted', 'value': True },
            { 'op': 'set', 'path': '/updatedDate', 'value': generate_utc_timestamp() }
        ]
        try:
            await self.container_client.patch_item(
                item=item_id,
                partition_key=item_id,
                patch_operations=operations,
                filter_predicate=filter
            )
        except Exception as error:
            if isinstance(error, CosmosAccessConditionFailedError):
                raise NotFoundError()

class DogRepository:
    def __init__(self, container_client: ContainerProxy):
        self.container_client = container_client

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

        raise NotFoundError()

    async def get_by_id(self, item_id: str) -> DogResponse:
        return DogResponse.model_validate(await self._get_stored(item_id))

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[DogResponse | None]:
        query = f"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {int(limit)}"
        items = [
            DogResponse.model_validate(item)
            async for item in self.container_client.query_items(query=query)
        ]
        return items

    async def create(self, item: Dog) -> DogResponse:
        now = generate_utc_timestamp()
        item_dict = {
            **item.model_dump(exclude_none=True),
            "isDeleted": False,
            "createdDate": now,
            "updatedDate": now,
        }
        created_item = await self.container_client.create_item(item_dict)

        return DogResponse.model_validate(created_item)

    async def update(self, item: Dog) -> DogResponse:
        stored_item = await self._get_stored(item.id)
        changes = item.model_dump(include=set(BaseDog.model_fields), exclude_unset=True)
        patched_item = {
            **stored_item,
            **changes,
            "id": item.id,
            "updatedDate": generate_utc_timestamp(),
        }
        updated_item = await self.container_client.upsert_item(patched_item)

        return DogResponse.model_validate(updated_item)

    async def replace(self, item: Dog) -> DogResponse:
        stored_item = await self._get_stored(item.id)
        now = generate_utc_timestamp()
        replacement = {
            **item.model_dump(include=set(BaseDog.model_fields), exclude_none=True),
            "id": item.id,
            "isDeleted": False,
            "createdDate": stored_item.get("createdDate", now),
            "updatedDate": now,
        }
        replaced_item = await self.container_client.upsert_item(replacement)

        return DogResponse.model_validate(replaced_item)

    async def delete(self, item_id: str):
        filter = "from c WHERE c.isDeleted = false"
        operations: list[dict] = [
            { 'op': 'replace', 'path': '/isDeleted', 'value': True },
            { 'op': 'set', 'path': '/updatedDate', 'value': generate_utc_timestamp() }
        ]
        try:
            await self.container_client.patch_item(
                item=item_id,
                partition_key=item_id,
                patch_operations=operations,
                filter_predicate=filter
            )
        except Exception as error:
            if isinstance(error, CosmosAccessConditionFailedError):
                raise NotFoundError()
