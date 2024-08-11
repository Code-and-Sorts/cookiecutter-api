
from typing import List
from repositories import ItemRepository
from models import Item, ItemResponse


class ItemService:
    def __init__(self, repository: ItemRepository):
        self.repository = repository

    def get_by_id(self, item_id: str) -> ItemResponse:
        return self.repository.get_by_id(item_id)

    def get_list(self) -> List[ItemResponse]:
        return self.repository.get_list()

    def create(self, item: Item) -> ItemResponse:
        return self.repository.create(item)

    def update(self, item: Item) -> ItemResponse:
        return self.repository.update(item)

    def soft_delete(self, item_id: str):
        self.repository.delete(item_id)
