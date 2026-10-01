import asyncio
import sys
import time
{%- if cloud_service == 'Azure Function App' %}
from azure.cosmos import PartitionKey
from azure.cosmos.aio import CosmosClient
from blueprints.database import cosmos_client_options
{%- elif cloud_service == 'GCP Cloud Function' %}
import urllib.request
{%- else %}
import aioboto3
from botocore.exceptions import ClientError
{%- endif %}
from config import Settings, get_settings

TIMEOUT_SECONDS = 120
MAX_DELAY_SECONDS = 8


class NotEmulatorError(Exception):
    pass
{%- if cloud_service == 'Azure Function App' %}


async def bootstrap(settings: Settings) -> None:
    if not settings.cosmos_db_emulator:
        raise NotEmulatorError("Cosmos_Db_Emulator is not true; refusing to create containers outside the emulator.")
    async with CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key, **cosmos_client_options(settings)) as client:
        database = await client.create_database_if_not_exists(settings.cosmos_db_database_name)
        for name in sorted(set(settings.container_names.values())):
            await database.create_container_if_not_exists(name, PartitionKey(path="/id"))
            print(f"Container {settings.cosmos_db_database_name}/{name} is ready.")
{%- elif cloud_service == 'GCP Cloud Function' %}


async def bootstrap(settings: Settings) -> None:
    host = settings.firestore_emulator_host
    if not host:
        raise NotEmulatorError("FIRESTORE_EMULATOR_HOST is not set; refusing to run outside the emulator.")
    with urllib.request.urlopen(f"http://{host}/", timeout=5) as response:
        if response.read().strip() != b"Ok":
            raise ConnectionError(f"{host} is not a Firestore emulator.")
    names = ", ".join(sorted(set(settings.collections.values())))
    print(f"Firestore emulator at {host} is ready for project {settings.gcp_project_id} (collections: {names}).")
{%- else %}


async def bootstrap(settings: Settings) -> None:
    if not settings.aws_endpoint_url_dynamodb:
        raise NotEmulatorError("AWS_ENDPOINT_URL_DYNAMODB is not set; refusing to create tables outside the emulator.")
    async with aioboto3.Session().client("dynamodb", region_name=settings.aws_region) as client:
        for name in sorted(set(settings.tables.values())):
            try:
                await client.create_table(
                    TableName=name,
                    AttributeDefinitions=[{"AttributeName": "id", "AttributeType": "S"}],
                    KeySchema=[{"AttributeName": "id", "KeyType": "HASH"}],
                    BillingMode="PAY_PER_REQUEST",
                )
            except ClientError as error:
                if error.response["Error"]["Code"] != "ResourceInUseException":
                    raise
            await client.get_waiter("table_exists").wait(TableName=name, WaiterConfig={"Delay": 1, "MaxAttempts": 30})
            print(f"Table {name} is ready.")
{%- endif %}


def main() -> int:
    settings = get_settings()
    deadline = time.monotonic() + TIMEOUT_SECONDS
    delay = 1
    while True:
        try:
            asyncio.run(bootstrap(settings))
            return 0
        except NotEmulatorError as error:
            print(error, file=sys.stderr)
            return 1
        except Exception as error:
            if time.monotonic() + delay > deadline:
                print(f"The emulator was not ready within {TIMEOUT_SECONDS} s: {error!r}", file=sys.stderr)
                return 1
            print(f"Waiting for the emulator ({error.__class__.__name__}); retrying in {delay} s.", file=sys.stderr)
            time.sleep(delay)
            delay = min(delay * 2, MAX_DELAY_SECONDS)


if __name__ == "__main__":
    sys.exit(main())
