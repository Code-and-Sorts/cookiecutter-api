

import aioboto3
from boto3.dynamodb.conditions import Attr
from botocore.exceptions import ClientError
from typing import List
from models import (
    generate_utc_timestamp,
    BaseKittenClaws,
    KittenClaws,
    KittenClawsResponse,
)
from errors import NotFoundError

# Default cap on the number of items returned by list endpoints to avoid
# unbounded reads. Callers may request a smaller page via the `limit` argument.
DEFAULT_LIST_LIMIT = 100



class KittenClawsRepository:
    def __init__(self, session: aioboto3.Session, table_name: str, region: str):
        self.session = session
        self.table_name = table_name
        self.region = region

    async def _get_stored(self, item_id: str) -> dict:
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            response = await table.get_item(Key={"id": item_id})
        item = response.get("Item")

        if not item or item.get("isDeleted", False):
            raise NotFoundError()

        return item

    async def get_by_id(self, item_id: str) -> KittenClawsResponse:
        return KittenClawsResponse.model_validate(await self._get_stored(item_id))

    async def get_list(self, limit: int = DEFAULT_LIST_LIMIT) -> List[KittenClawsResponse | None]:
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
        return [KittenClawsResponse.model_validate(item) for item in items]

    async def create(self, item: KittenClaws) -> KittenClawsResponse:
        now = generate_utc_timestamp()
        item_dict = {
            **item.model_dump(exclude_none=True),
            "isDeleted": False,
            "createdDate": now,
            "updatedDate": now,
        }
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=item_dict)

        return KittenClawsResponse.model_validate(item_dict)

    async def update(self, item: KittenClaws) -> KittenClawsResponse:
        stored_item = await self._get_stored(item.id)
        changes = item.model_dump(include=set(BaseKittenClaws.model_fields), exclude_unset=True)
        patched_item = {
            **stored_item,
            **changes,
            "id": item.id,
            "updatedDate": generate_utc_timestamp(),
        }
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=patched_item)

        return KittenClawsResponse.model_validate(patched_item)

    async def replace(self, item: KittenClaws) -> KittenClawsResponse:
        stored_item = await self._get_stored(item.id)
        now = generate_utc_timestamp()
        replacement = {
            **item.model_dump(include=set(BaseKittenClaws.model_fields), exclude_none=True),
            "id": item.id,
            "isDeleted": False,
            "createdDate": stored_item.get("createdDate", now),
            "updatedDate": now,
        }
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=replacement)

        return KittenClawsResponse.model_validate(replacement)

    async def delete(self, item_id: str):
        try:
            async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
                table = await dynamodb.Table(self.table_name)
                await table.update_item(
                    Key={"id": item_id},
                    UpdateExpression="SET isDeleted = :val, updatedDate = :updated",
                    ConditionExpression="attribute_exists(id) AND (attribute_not_exists(isDeleted) OR isDeleted = :false)",
                    ExpressionAttributeValues={":val": True, ":false": False, ":updated": generate_utc_timestamp()}
                )
        except ClientError as error:
            if error.response["Error"]["Code"] == "ConditionalCheckFailedException":
                raise NotFoundError()
            raise
