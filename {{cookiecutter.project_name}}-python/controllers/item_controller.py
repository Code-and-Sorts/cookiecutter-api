from typing import List
import azure.functions as func
from services import ItemService
from models import ItemResponse, Item
from errors import NotFoundError

class ItemController:
    def __init__(self, service: ItemService):
        self.service = service

    def get_by_id(self, req: func.HttpRequest) -> ItemResponse:
        item_id: str = req.route_params.get('item_id')
        item = self.service.get_by_id(item_id)
        if item:
            return item
        raise NotFoundError()

    def get_list(self) -> List[ItemResponse]:
        return self.service.get_list()

    def create(self, req: func.HttpRequest) -> ItemResponse:
        item_json = req.get_json()
        item = Item(**item_json)
        return self.service.create(item)

    def update(self, req: func.HttpRequest) -> ItemResponse:
        item_id = req.route_params.get('item_id')
        item_data = req.get_json()
        item = Item(**item_data)
        item.id = item_id
        return self.service.update(item)

    def soft_delete(self, req: func.HttpRequest):
        item_id = req.route_params.get('item_id')
        self.service.soft_delete(item_id)
        # if success:
        #     return func.HttpResponse(status_code=204)
        # else:
        #     return func.HttpResponse(status_code=404)
