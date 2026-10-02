import asyncio
import sys
import time
import urllib.request
from config import Settings, get_settings

TIMEOUT_SECONDS = 120
MAX_DELAY_SECONDS = 8


class NotEmulatorError(Exception):
    pass


async def bootstrap(settings: Settings) -> None:
    host = settings.firestore_emulator_host
    if not host:
        raise NotEmulatorError("FIRESTORE_EMULATOR_HOST is not set; refusing to run outside the emulator.")
    with urllib.request.urlopen(f"http://{host}/", timeout=5) as response:
        if response.read().strip() != b"Ok":
            raise ConnectionError(f"{host} is not a Firestore emulator.")
    names = ", ".join(sorted(set(settings.collections.values())))
    print(f"Firestore emulator at {host} is ready for project {settings.gcp_project_id} (collections: {names}).")


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
