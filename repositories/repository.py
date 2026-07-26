
from google.cloud.firestore import AsyncCollectionReference

from typing import List, Optional
from models import (
    Cat,
    CatResponse,
    Dog,
    DogResponse,
)
from errors import NotFoundError

# Default cap on the number of items returned by list endpoints to avoid
# unbounded reads. Callers may request a smaller page via the `limit` argument.
DEFAULT_LIST_LIMIT = 100



class CatRepository:
    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection

    async def get_by_id(self, item_id: str) -> Optional[CatResponse]:
        doc = await self.collection.document(item_id).get()
        if not doc.exists:
            raise NotFoundError()

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise NotFoundError()

        return CatResponse.model_validate(data)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[CatResponse | None]:
        query = self.collection.where('isDeleted', '==', False).limit(int(limit))
        items = []
        async for doc in query.stream():
            data = doc.to_dict()
            items.append(CatResponse.model_validate(data))
        return items

    async def create(self, item: Cat) -> CatResponse:
        item_dict = item.model_dump(exclude_none=True)
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(item_dict)

        return CatResponse.model_validate(item_dict)

    async def update(self, item: Cat) -> Optional[CatResponse]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = await self.get_by_id(item.id)
        if not previous_item:
            raise NotFoundError()
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        patched_item = {**previous_item_dict,**new_item_dict}
        doc_ref = self.collection.document(item.id)
        await doc_ref.update(patched_item)

        return CatResponse.model_validate(patched_item)

    async def replace(self, item: Cat) -> Optional[CatResponse]:
        await self.get_by_id(item.id)
        new_item_dict = item.model_dump(exclude_none=True)
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(new_item_dict)

        return CatResponse.model_validate(new_item_dict)

    async def delete(self, item_id: str):
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get()

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()

        await doc_ref.update({'isDeleted': True})

class DogRepository:
    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection

    async def get_by_id(self, item_id: str) -> Optional[DogResponse]:
        doc = await self.collection.document(item_id).get()
        if not doc.exists:
            raise NotFoundError()

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise NotFoundError()

        return DogResponse.model_validate(data)

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[DogResponse | None]:
        query = self.collection.where('isDeleted', '==', False).limit(int(limit))
        items = []
        async for doc in query.stream():
            data = doc.to_dict()
            items.append(DogResponse.model_validate(data))
        return items

    async def create(self, item: Dog) -> DogResponse:
        item_dict = item.model_dump(exclude_none=True)
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(item_dict)

        return DogResponse.model_validate(item_dict)

    async def update(self, item: Dog) -> Optional[DogResponse]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = await self.get_by_id(item.id)
        if not previous_item:
            raise NotFoundError()
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        patched_item = {**previous_item_dict,**new_item_dict}
        doc_ref = self.collection.document(item.id)
        await doc_ref.update(patched_item)

        return DogResponse.model_validate(patched_item)

    async def replace(self, item: Dog) -> Optional[DogResponse]:
        await self.get_by_id(item.id)
        new_item_dict = item.model_dump(exclude_none=True)
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(new_item_dict)

        return DogResponse.model_validate(new_item_dict)

    async def delete(self, item_id: str):
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get()

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()

        await doc_ref.update({'isDeleted': True})
