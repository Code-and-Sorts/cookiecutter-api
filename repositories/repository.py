
from google.cloud.firestore import AsyncCollectionReference

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



class CatRepository:
    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection

    async def _get_stored(self, item_id: str) -> dict:
        doc = await self.collection.document(item_id).get()
        if not doc.exists:
            raise NotFoundError()

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise NotFoundError()

        return data

    async def get_by_id(self, item_id: str) -> CatResponse:
        return CatResponse.model_validate(await self._get_stored(item_id))

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[CatResponse | None]:
        query = self.collection.where('isDeleted', '==', False).limit(int(limit))
        items = []
        async for doc in query.stream():
            data = doc.to_dict()
            items.append(CatResponse.model_validate(data))
        return items

    async def create(self, item: Cat) -> CatResponse:
        now = generate_utc_timestamp()
        item_dict = {
            **item.model_dump(exclude_none=True),
            "isDeleted": False,
            "createdDate": now,
            "updatedDate": now,
        }
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(item_dict)

        return CatResponse.model_validate(item_dict)

    async def update(self, item: Cat) -> CatResponse:
        stored_item = await self._get_stored(item.id)
        changes = item.model_dump(include=set(BaseCat.model_fields), exclude_unset=True)
        patched_item = {
            **stored_item,
            **changes,
            "id": item.id,
            "updatedDate": generate_utc_timestamp(),
        }
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(patched_item)

        return CatResponse.model_validate(patched_item)

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
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(replacement)

        return CatResponse.model_validate(replacement)

    async def delete(self, item_id: str):
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get()

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()

        await doc_ref.update({'isDeleted': True, 'updatedDate': generate_utc_timestamp()})

class DogRepository:
    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection

    async def _get_stored(self, item_id: str) -> dict:
        doc = await self.collection.document(item_id).get()
        if not doc.exists:
            raise NotFoundError()

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise NotFoundError()

        return data

    async def get_by_id(self, item_id: str) -> DogResponse:
        return DogResponse.model_validate(await self._get_stored(item_id))

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[DogResponse | None]:
        query = self.collection.where('isDeleted', '==', False).limit(int(limit))
        items = []
        async for doc in query.stream():
            data = doc.to_dict()
            items.append(DogResponse.model_validate(data))
        return items

    async def create(self, item: Dog) -> DogResponse:
        now = generate_utc_timestamp()
        item_dict = {
            **item.model_dump(exclude_none=True),
            "isDeleted": False,
            "createdDate": now,
            "updatedDate": now,
        }
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(item_dict)

        return DogResponse.model_validate(item_dict)

    async def update(self, item: Dog) -> DogResponse:
        stored_item = await self._get_stored(item.id)
        changes = item.model_dump(include=set(BaseDog.model_fields), exclude_unset=True)
        patched_item = {
            **stored_item,
            **changes,
            "id": item.id,
            "updatedDate": generate_utc_timestamp(),
        }
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(patched_item)

        return DogResponse.model_validate(patched_item)

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
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(replacement)

        return DogResponse.model_validate(replacement)

    async def delete(self, item_id: str):
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get()

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()

        await doc_ref.update({'isDeleted': True, 'updatedDate': generate_utc_timestamp()})
