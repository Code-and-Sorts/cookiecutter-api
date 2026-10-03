import uuid
from google.api_core.retry_async import AsyncRetry
from google.cloud.firestore import DELETE_FIELD, AsyncCollectionReference, FieldFilter
from typing import ClassVar, List
from pydantic import BaseModel
from models import BaseEntity, generate_utc_timestamp
from errors import NotFoundError

def _user_fields(user_id: str | None, *fields: str) -> dict:
    return {field: user_id for field in fields} if user_id else {}


def _without_nulls(record: dict) -> dict:
    """A field without a value is not stored, in every language and database."""
    return {key: value for key, value in record.items() if value is not None}

# The SDK retries for up to 300 s by default; keep each call inside the request deadline.
FIRESTORE_CALL_OPTIONS = {
    "retry": AsyncRetry(initial=0.1, maximum=1.0, multiplier=2.0, timeout=5.0),
    "timeout": 3.0,
}


class BaseRepository[ResponseT: BaseModel]:
    """Protected so each resource repository exposes only its enabled operations."""

    resource_name: ClassVar[str]
    entity_model: ClassVar[type[BaseEntity]]
    response_model: ClassVar[type[BaseModel]]

    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection

    def _not_found(self, item_id: str) -> NotFoundError:
        return NotFoundError.for_item(self.resource_name, item_id)

    async def _get_stored(self, item_id: str) -> dict:
        doc = await self.collection.document(item_id).get(**FIRESTORE_CALL_OPTIONS)
        if not doc.exists:
            raise self._not_found(item_id)

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise self._not_found(item_id)

        return data

    async def _write(self, record: dict) -> None:
        await self.collection.document(record["id"]).set(record, **FIRESTORE_CALL_OPTIONS)

    async def _get_by_id(self, item_id: str) -> ResponseT:
        return self.response_model.model_validate(await self._get_stored(item_id))

    async def _get_list(self, limit: int) -> List[ResponseT]:
        limit = int(limit)
        query = self.collection.where(filter=FieldFilter("isDeleted", "==", False)).limit(limit)
        items = []
        async for doc in query.stream(**FIRESTORE_CALL_OPTIONS):
            items.append(self.response_model.model_validate(doc.to_dict()))
        return items

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
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get(**FIRESTORE_CALL_OPTIONS)

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise self._not_found(item_id)

        await doc_ref.update(
            {'isDeleted': True, 'updatedTimestamp': generate_utc_timestamp(), 'updatedBy': user_id or DELETE_FIELD},
            **FIRESTORE_CALL_OPTIONS
        )
