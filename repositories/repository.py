from azure.cosmos.aio import ContainerProxy
from azure.cosmos.exceptions import CosmosAccessConditionFailedError


from typing import List, Optional
from models import (
    KittenClaws,
    KittenClawsResponse,
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

class KittenClawsRepository:
    def __init__(self, container_client: ContainerProxy):
        self.container_client = container_client

    async def get_by_id(self, item_id: str) -> Optional[KittenClawsResponse]:
        query = "SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false"
        parameters = [
            { "name": "@id", "value": item_id }
        ]
        async for item in self.container_client.query_items(
            query=query,
            parameters=parameters
        ):
            return KittenClawsResponse.model_validate(item)

        raise NotFoundError()

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[KittenClawsResponse | None]:
        query = f"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {int(limit)}"
        items = [
            KittenClawsResponse.model_validate(item)
            async for item in self.container_client.query_items(query=query)
        ]
        return items

    async def create(self, item: KittenClaws) -> KittenClawsResponse:
        item_dict = item.model_dump(exclude_none=True)
        created_item = await self.container_client.create_item(item_dict)

        return KittenClawsResponse.model_validate(created_item)

    async def update(self, item: KittenClaws) -> Optional[KittenClawsResponse]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = await self.get_by_id(item.id)
        if not previous_item:
            raise NotFoundError()
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        patched_item = {**previous_item_dict,**new_item_dict}
        updated_item = await self.container_client.upsert_item(patched_item)

        return KittenClawsResponse.model_validate(updated_item)

    async def replace(self, item: KittenClaws) -> Optional[KittenClawsResponse]:
        await self.get_by_id(item.id)
        new_item_dict = item.model_dump(exclude_none=True)
        replaced_item = await self.container_client.upsert_item(new_item_dict)

        return KittenClawsResponse.model_validate(replaced_item)

    async def delete(self, item_id: str):
        filter = "from c WHERE c.isDeleted = false"
        operations: list[dict[str, str]] = [
            { 'op': 'replace', 'path': '/isDeleted', 'value': True }
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
