from datetime import datetime, timezone


def generate_utc_timestamp():
    return str(datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"))
