from azure.cosmos import CosmosClient
from azure.cosmos.exceptions import CosmosAccessConditionFailedError
from typing import List, Optional
from models import Item, ItemResponse, BaseItem
from errors import ProxyError, NotFoundError

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

class ItemRepository:
    def __init__(self, database: Database):
        self.client = CosmosClient(database._endpoint, database._key)
        self.database_client = self.client.get_database_client(database.name)
        self.container_client = self.database_client.get_container_client(database.container_name)

    def get_by_id(self, item_id: str) -> Optional[ItemResponse]:
        query = "SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false"
        parameters = [
            { "name": "@id", "value": item_id }
        ]
        items = self.container_client.query_items(
            query=query,
            parameters=parameters,
            enable_cross_partition_query=True
        )
        for item in items:
            return ItemResponse.model_validate(item)

        raise NotFoundError()

    def get_list(self) -> List[ItemResponse | None]:
        query = "SELECT * FROM c WHERE c.isDeleted = false"
        items = self.container_client.query_items(query, enable_cross_partition_query=True)

        if items:
            return [ItemResponse.model_validate(item) for item in items]
        return []

    def create(self, item: Item) -> ItemResponse:
        item_dict = item.model_dump(exclude_none=True)
        created_item = self.container_client.create_item(item_dict)

        return ItemResponse.model_validate(created_item)

    def update(self, item: BaseItem) -> Optional[ItemResponse]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = self.get_by_id(item.id)
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        if not previous_item:
            raise NotFoundError()
        patched_item = {**previous_item_dict,**new_item_dict}
        updated_item = self.container_client.upsert_item(patched_item)

        return ItemResponse.model_validate(updated_item)

    def delete(self, item_id: str):
        filter = "from c WHERE c.isDeleted = false"
        operations: list[dict[str, str]] = [
            { 'op': 'replace', 'path': '/isDeleted', 'value': True }
        ]
        try:
            self.container_client.patch_item(
                item=item_id,
                partition_key=item_id,
                patch_operations=operations,
                filter_predicate=filter
            )
        except Exception as error:
            if isinstance(error, CosmosAccessConditionFailedError):
                raise NotFoundError()
            print(str(error))