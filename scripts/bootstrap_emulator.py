import asyncio
import sys
import time
import aioboto3
from botocore.exceptions import ClientError
from config import Settings, get_settings

TIMEOUT_SECONDS = 120
MAX_DELAY_SECONDS = 8


class NotEmulatorError(Exception):
    pass


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
