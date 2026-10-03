import asyncio
import sys
import time
from azure.cosmos import PartitionKey
from azure.cosmos.aio import CosmosClient
from blueprints.database import cosmos_client_options
from config import Settings, get_settings

TIMEOUT_SECONDS = 120
MAX_DELAY_SECONDS = 8


class NotEmulatorError(Exception):
    pass


async def bootstrap(settings: Settings) -> None:
    if not settings.cosmos_db_emulator:
        raise NotEmulatorError("Cosmos_Db_Emulator is not true; refusing to create containers outside the emulator.")
    async with CosmosClient(settings.cosmos_db_uri, settings.cosmos_db_key, **cosmos_client_options(settings)) as client:
        database = await client.create_database_if_not_exists(settings.cosmos_db_database_name)
        for name in sorted(set(settings.container_names.values())):
            await database.create_container_if_not_exists(name, PartitionKey(path="/id"))
            print(f"Container {settings.cosmos_db_database_name}/{name} is ready.")


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
