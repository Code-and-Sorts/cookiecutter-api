{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.cosmos import ContainerProxy
from azure.cosmos.exceptions import CosmosAccessConditionFailedError
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
from google.cloud import firestore
from google.cloud.firestore_v1.base_query import FieldFilter
{%- endif %}
from typing import List, Optional
from models import {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}Response
from errors import NotFoundError

{% if cookiecutter.cloud_service == 'Azure Function App' -%}
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

class {{ cookiecutter.project_class_name }}Repository:
    def __init__(self, container_client: ContainerProxy):
        self.container_client = container_client

    def get_by_id(self, item_id: str) -> Optional[{{ cookiecutter.project_class_name }}Response]:
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
            return {{ cookiecutter.project_class_name }}Response.model_validate(item)

        raise NotFoundError()

    def get_list(self) -> List[{{ cookiecutter.project_class_name }}Response | None]:
        query = "SELECT * FROM c WHERE c.isDeleted = false"
        items = self.container_client.query_items(query=query, enable_cross_partition_query=True)

        if items:
            return [{{ cookiecutter.project_class_name }}Response.model_validate(item) for item in items]
        return []

    def create(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        item_dict = item.model_dump(exclude_none=True)
        created_item = self.container_client.create_item(item_dict)

        return {{ cookiecutter.project_class_name }}Response.model_validate(created_item)

    def update(self, item: {{ cookiecutter.project_class_name }}) -> Optional[{{ cookiecutter.project_class_name }}Response]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = self.get_by_id(item.id)
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        if not previous_item:
            raise NotFoundError()
        patched_item = {**previous_item_dict,**new_item_dict}
        updated_item = self.container_client.upsert_item(patched_item)

        return {{ cookiecutter.project_class_name }}Response.model_validate(updated_item)

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
{%- elif cookiecutter.cloud_service == 'Google Cloud Function' -%}
class Database:
    def __init__(
            self,
            project_id: str,
            collection_name: str
        ):
            self.project_id = project_id
            self.collection_name = collection_name

class {{ cookiecutter.project_class_name }}Repository:
    def __init__(self, db: firestore.Client, collection_name: str):
        self.db = db
        self.collection_name = collection_name

    def get_by_id(self, item_id: str) -> Optional[{{ cookiecutter.project_class_name }}Response]:
        doc_ref = self.db.collection(self.collection_name).document(item_id)
        doc = doc_ref.get()
        
        if doc.exists:
            data = doc.to_dict()
            if not data.get('isDeleted', False):
                return {{ cookiecutter.project_class_name }}Response.model_validate(data)
        
        raise NotFoundError()

    def get_list(self) -> List[{{ cookiecutter.project_class_name }}Response | None]:
        docs = self.db.collection(self.collection_name).where(
            filter=FieldFilter("isDeleted", "==", False)
        ).stream()
        
        items = []
        for doc in docs:
            data = doc.to_dict()
            items.append({{ cookiecutter.project_class_name }}Response.model_validate(data))
        
        return items

    def create(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        item_dict = item.model_dump(exclude_none=True)
        item_dict['isDeleted'] = False
        
        doc_ref = self.db.collection(self.collection_name).document(item_dict['id'])
        doc_ref.set(item_dict)
        
        return {{ cookiecutter.project_class_name }}Response.model_validate(item_dict)

    def update(self, item: {{ cookiecutter.project_class_name }}) -> Optional[{{ cookiecutter.project_class_name }}Response]:
        # First get the existing item
        previous_item = self.get_by_id(item.id)
        if not previous_item:
            raise NotFoundError()
        
        # Merge the changes
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        patched_item = {**previous_item_dict, **new_item_dict}
        
        # Update in Firestore
        doc_ref = self.db.collection(self.collection_name).document(item.id)
        doc_ref.update(patched_item)
        
        return {{ cookiecutter.project_class_name }}Response.model_validate(patched_item)

    def delete(self, item_id: str):
        doc_ref = self.db.collection(self.collection_name).document(item_id)
        doc = doc_ref.get()
        
        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()
        
        doc_ref.update({'isDeleted': True})
{%- endif %}
