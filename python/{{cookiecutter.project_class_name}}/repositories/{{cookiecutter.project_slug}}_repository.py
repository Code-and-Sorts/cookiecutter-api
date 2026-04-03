{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.cosmos import ContainerProxy
from azure.cosmos.exceptions import CosmosAccessConditionFailedError
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
from google.cloud.firestore import Client as FirestoreClient, CollectionReference
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
from boto3.dynamodb.conditions import Attr
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
{%- endif %}

class {{ cookiecutter.project_class_name }}Repository:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
    def __init__(self, container_client: ContainerProxy):
        self.container_client = container_client
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
    def __init__(self, collection: CollectionReference):
        self.collection = collection
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
    def __init__(self, table):
        self.table = table
{%- endif %}

    def get_by_id(self, item_id: str) -> Optional[{{ cookiecutter.project_class_name }}Response]:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
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
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc = self.collection.document(item_id).get()
        if not doc.exists:
            raise NotFoundError()

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise NotFoundError()

        return {{ cookiecutter.project_class_name }}Response.model_validate(data)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        response = self.table.get_item(Key={"id": item_id})
        item = response.get("Item")

        if not item or item.get("isDeleted", False):
            raise NotFoundError()

        return {{ cookiecutter.project_class_name }}Response.model_validate(item)
{%- endif %}

    def get_list(self) -> List[{{ cookiecutter.project_class_name }}Response | None]:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        query = "SELECT * FROM c WHERE c.isDeleted = false"
        items = self.container_client.query_items(query=query, enable_cross_partition_query=True)

        if items:
            return [{{ cookiecutter.project_class_name }}Response.model_validate(item) for item in items]
        return []
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        docs = self.collection.where('isDeleted', '==', False).stream()
        items = []
        for doc in docs:
            data = doc.to_dict()
            items.append({{ cookiecutter.project_class_name }}Response.model_validate(data))
        return items
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        items = []
        filter_exp = Attr("isDeleted").eq(False) | Attr("isDeleted").not_exists()
        response = self.table.scan(FilterExpression=filter_exp)
        items.extend(response.get("Items", []))

        while "LastEvaluatedKey" in response:
            response = self.table.scan(
                FilterExpression=filter_exp,
                ExclusiveStartKey=response["LastEvaluatedKey"]
            )
            items.extend(response.get("Items", []))

        return [{{ cookiecutter.project_class_name }}Response.model_validate(item) for item in items]
{%- endif %}

    def create(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        item_dict = item.model_dump(exclude_none=True)
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        created_item = self.container_client.create_item(item_dict)

        return {{ cookiecutter.project_class_name }}Response.model_validate(created_item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc_ref = self.collection.document(item.id)
        doc_ref.set(item_dict)

        return {{ cookiecutter.project_class_name }}Response.model_validate(item_dict)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        self.table.put_item(Item=item_dict)

        return {{ cookiecutter.project_class_name }}Response.model_validate(item_dict)
{%- endif %}

    def update(self, item: {{ cookiecutter.project_class_name }}) -> Optional[{{ cookiecutter.project_class_name }}Response]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = self.get_by_id(item.id)
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        if not previous_item:
            raise NotFoundError()
        patched_item = {**previous_item_dict,**new_item_dict}
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        updated_item = self.container_client.upsert_item(patched_item)

        return {{ cookiecutter.project_class_name }}Response.model_validate(updated_item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc_ref = self.collection.document(item.id)
        doc_ref.update(patched_item)

        return {{ cookiecutter.project_class_name }}Response.model_validate(patched_item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        self.table.put_item(Item=patched_item)

        return {{ cookiecutter.project_class_name }}Response.model_validate(patched_item)
{%- endif %}

    def delete(self, item_id: str):
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
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
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc_ref = self.collection.document(item_id)
        doc = doc_ref.get()

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()

        doc_ref.update({'isDeleted': True})
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        response = self.table.get_item(Key={"id": item_id})
        item = response.get("Item")

        if not item or item.get("isDeleted", False):
            raise NotFoundError()

        self.table.update_item(
            Key={"id": item_id},
            UpdateExpression="SET isDeleted = :val",
            ExpressionAttributeValues={":val": True}
        )
{%- endif %}
