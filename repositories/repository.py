

import aioboto3
from boto3.dynamodb.conditions import Attr
from botocore.exceptions import ClientError
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
    def __init__(self, session: aioboto3.Session, table_name: str, region: str):
        self.session = session
        self.table_name = table_name
        self.region = region

    async def get_by_id(self, item_id: str) -> Optional[KittenClawsResponse]:
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            response = await table.get_item(Key={"id": item_id})
        item = response.get("Item")

        if not item or item.get("isDeleted", False):
            raise NotFoundError()

        return KittenClawsResponse.model_validate(item)

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
        item_dict = item.model_dump(exclude_none=True)
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=item_dict)

        return KittenClawsResponse.model_validate(item_dict)

    async def update(self, item: KittenClaws) -> Optional[KittenClawsResponse]:
        new_item_dict = item.model_dump(exclude_none=True)
        previous_item = await self.get_by_id(item.id)
        if not previous_item:
            raise NotFoundError()
        previous_item_dict = previous_item.model_dump(exclude_none=True)
        patched_item = {**previous_item_dict,**new_item_dict}
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=patched_item)

        return KittenClawsResponse.model_validate(patched_item)

    async def replace(self, item: KittenClaws) -> Optional[KittenClawsResponse]:
        await self.get_by_id(item.id)
        new_item_dict = item.model_dump(exclude_none=True)
        async with self.session.resource("dynamodb", region_name=self.region) as dynamodb:
            table = await dynamodb.Table(self.table_name)
            await table.put_item(Item=new_item_dict)

        return KittenClawsResponse.model_validate(new_item_dict)

    async def delete(self, item_id: str):
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
