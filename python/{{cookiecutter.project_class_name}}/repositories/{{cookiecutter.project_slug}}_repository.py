{% if cookiecutter.cloud_service == 'Azure Function App' -%}
from azure.cosmos.aio import ContainerProxy
from azure.cosmos.exceptions import CosmosAccessConditionFailedError
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
from google.cloud.firestore import AsyncCollectionReference
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
import aioboto3
from boto3.dynamodb.conditions import Attr
from botocore.exceptions import ClientError
{%- endif %}
from typing import List, Optional
from models import {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}Response
from errors import NotFoundError

# Default cap on the number of items returned by list endpoints to avoid
# unbounded reads. Callers may request a smaller page via the `limit` argument.
DEFAULT_LIST_LIMIT = 100

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
    def __init__(self, collection: AsyncCollectionReference):
        self.collection = collection
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
    def __init__(self, session: aioboto3.Session, table_name: str, region: str):
        self.session = session
        self.table_name = table_name
        self.region = region
{%- endif %}

    async def get_by_id(self, item_id: str) -> Optional[{{ cookiecutter.project_class_name }}Response]:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        query = "SELECT * FROM c WHERE c.id = @id AND c.isDeleted = false"
        parameters = [
            { "name": "@id", "value": item_id }
        ]
        async for item in self.container_client.query_items(
            query=query,
            parameters=parameters
        ):
            return {{ cookiecutter.project_class_name }}Response.model_validate(item)

        raise NotFoundError()
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc = await self.collection.document(item_id).get()
        if not doc.exists:
            raise NotFoundError()

        data = doc.to_dict()
        if data.get('isDeleted', False):
            raise NotFoundError()

        return {{ cookiecutter.project_class_name }}Response.model_validate(data)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            response = await table.get_item(Key={"id": item_id})
        item = response.get("Item")

        if not item or item.get("isDeleted", False):
            raise NotFoundError()

        return {{ cookiecutter.project_class_name }}Response.model_validate(item)
{%- endif %}

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[{{ cookiecutter.project_class_name }}Response | None]:
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        query = f"SELECT * FROM c WHERE c.isDeleted = false OFFSET 0 LIMIT {int(limit)}"
        items = [
            {{ cookiecutter.project_class_name }}Response.model_validate(item)
            async for item in self.container_client.query_items(query=query)
        ]
        return items
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        query = self.collection.where('isDeleted', '==', False).limit(int(limit))
        items = []
        async for doc in query.stream():
            data = doc.to_dict()
            items.append({{ cookiecutter.project_class_name }}Response.model_validate(data))
        return items
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        items = []
        filter_exp = Attr("isDeleted").eq(False) | Attr("isDeleted").not_exists()
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            response = await table.scan(FilterExpression=filter_exp, Limit=int(limit))
            items.extend(response.get("Items", []))

            while "LastEvaluatedKey" in response and len(items) < limit:
                response = await table.scan(
                    FilterExpression=filter_exp,
                    Limit=int(limit),
                    ExclusiveStartKey=response["LastEvaluatedKey"]
                )
                items.extend(response.get("Items", []))

        items = items[:limit]
        return [{{ cookiecutter.project_class_name }}Response.model_validate(item) for item in items]
{%- endif %}

    async def create(self, item: {{ cookiecutter.project_class_name }}) -> {{ cookiecutter.project_class_name }}Response:
        item_dict = item.model_dump(exclude_none=True)
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        created_item = await self.container_client.create_item(item_dict)

        return {{ cookiecutter.project_class_name }}Response.model_validate(created_item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc_ref = self.collection.document(item.id)
        await doc_ref.set(item_dict)

        return {{ cookiecutter.project_class_name }}Response.model_validate(item_dict)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=item_dict)

        return {{ cookiecutter.project_class_name }}Response.model_validate(item_dict)
{%- endif %}

    async def update(self, item: {{ cookiecutter.project_class_name }}) -> Optional[{{ cookiecutter.project_class_name }}Response]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = await self.get_by_id(item.id)
        if not previous_item:
            raise NotFoundError()
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        patched_item = {**previous_item_dict,**new_item_dict}
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        updated_item = await self.container_client.upsert_item(patched_item)

        return {{ cookiecutter.project_class_name }}Response.model_validate(updated_item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc_ref = self.collection.document(item.id)
        await doc_ref.update(patched_item)

        return {{ cookiecutter.project_class_name }}Response.model_validate(patched_item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=patched_item)

        return {{ cookiecutter.project_class_name }}Response.model_validate(patched_item)
{%- endif %}

    async def delete(self, item_id: str):
{%- if cookiecutter.cloud_service == 'Azure Function App' %}
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
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        doc_ref = self.collection.document(item_id)
        doc = await doc_ref.get()

        if not doc.exists or doc.to_dict().get('isDeleted', False):
            raise NotFoundError()

        await doc_ref.update({'isDeleted': True})
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        try:
            async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
                table = await dynamodb.Table(self.table_name)
                await table.update_item(
                    Key={"id": item_id},
                    UpdateExpression="SET isDeleted = :val",
                    ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                    ExpressionAttributeValues={":val": True, ":false": False}
                )
        except ClientError as error:
            if error.response["Error"]["Code"] == "ConditionalCheckFailedException":
                raise NotFoundError()
            raise
{%- endif %}
