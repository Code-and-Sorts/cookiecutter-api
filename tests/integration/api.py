"""An HTTP client that speaks in resource operations rather than methods and paths."""

import time
from uuid import uuid4

import httpx

from project import Resource

# operation -> (method, addresses one item)
ROUTES = {
    "list": ("GET", False),
    "create": ("POST", False),
    "get_by_id": ("GET", True),
    "update": ("PATCH", True),
    "replace": ("PUT", True),
    "delete": ("DELETE", True),
}
OPERATIONS = tuple(ROUTES)
ITEM_OPERATIONS = tuple(op for op, (_, addresses_item) in ROUTES.items() if addresses_item)
BODY_OPERATIONS = ("create", "update", "replace")
LIST_MAX = 1000


def unique_name(resource: Resource) -> str:
    return f"{resource.name}-{uuid4().hex[:8]}"


def user_headers(user_id: str | None) -> dict[str, str]:
    return {"X-User-Id": user_id} if user_id is not None else {}


class Api:
    def __init__(self, base_url: str):
        # Generous: sam local starts a function's container on its first request.
        self.http = httpx.Client(base_url=base_url.rstrip("/"), timeout=60)

    def send(
        self,
        operation: str,
        resource: Resource,
        item_id: str | None = None,
        *,
        user_id: str | None = None,
        **kwargs,
    ) -> httpx.Response:
        """Sends a valid body to an operation that takes one unless the caller passes its own."""
        method, addresses_item = ROUTES[operation]
        if operation in BODY_OPERATIONS and "json" not in kwargs and "content" not in kwargs:
            kwargs["json"] = {"name": unique_name(resource)}
        path = f"/{resource.endpoint}"
        if addresses_item:
            path += f"/{item_id or uuid4()}"
        headers = {**user_headers(user_id), **kwargs.pop("headers", {})}
        return self.http.request(method, path, headers=headers, **kwargs)

    def list_ids(self, resource: Resource) -> set[str]:
        response = self.send("list", resource, params={"limit": LIST_MAX})
        assert response.status_code == 200, response.text
        items = response.json()
        # A full page could leave out the record under test; stores list in key order, not insertion order.
        assert len(items) < LIST_MAX, f"{resource.container} holds too many records; reset the emulator"
        return {item["id"] for item in items}

    def wait_until_ready(self, path: str, timeout: float) -> None:
        deadline = time.monotonic() + timeout
        last = "no response"
        while True:
            try:
                response = self.http.get(path)
                if response.status_code == 200:
                    return
                last = f"{response.status_code} {response.text[:200]}"
            except httpx.TransportError as error:
                last = repr(error)
            if time.monotonic() > deadline:
                raise TimeoutError(f"GET {self.http.base_url}{path} did not return 200 within {timeout:.0f}s: {last}")
            time.sleep(2)
