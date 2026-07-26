
from google.cloud.firestore import AsyncCollectionReference

from typing import List, Optional
from models import (
    KittenClaws,
    KittenClawsResponse,
)
from errors import NotFoundError

# Default cap on the number of items returned by list endpoints to avoid
# unbounded reads. Callers may request a smaller page via the `limit` argument.
DEFAULT_LIST_LIMIT = 100



class KittenClawsRepository:
    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection

    async def get_by_id(self, item_id: str) -> Optional[KittenClawsResponse]:
        doc = await self.collection.document(item_id).get()
        if not doc.exists:
            raise NotFoundError()

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise NotFoundError()

        return KittenClawsResponse.model_validate(data)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[KittenClawsResponse | None]:
        query = self.collection.where('isDeleted', '==', False).limit(int(limit))
        items = []
        async for doc in query.stream():
            data = doc.to_dict()
            items.append(KittenClawsResponse.model_validate(data))
        return items

    async def create(self, item: KittenClaws) -> KittenClawsResponse:
        item_dict = item.model_dump(exclude_none=True)
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(item_dict)

        return KittenClawsResponse.model_validate(item_dict)

    async def update(self, item: KittenClaws) -> Optional[KittenClawsResponse]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = await self.get_by_id(item.id)
        if not previous_item:
            raise NotFoundError()
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        patched_item = {**previous_item_dict,**new_item_dict}
        doc_ref = self.collection.document(item.id)
        await doc_ref.update(patched_item)

        return KittenClawsResponse.model_validate(patched_item)

    async def replace(self, item: KittenClaws) -> Optional[KittenClawsResponse]:
        await self.get_by_id(item.id)
        new_item_dict = item.model_dump(exclude_none=True)
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(new_item_dict)

        return KittenClawsResponse.model_validate(new_item_dict)

    async def delete(self, item_id: str):
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get()

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()

        await doc_ref.update({'isDeleted': True})
