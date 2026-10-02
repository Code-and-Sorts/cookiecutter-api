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
BODY_OPERATIONS = ("create", "update", "replace")


def unique_name(resource: Resource) -> str:
    return f"{resource.name}-{uuid4().hex[:8]}"


def user_headers(user_id: str | None) -> dict[str, str]:
    return {"X-User-Id": user_id} if user_id is not None else {}


class Api:
    def __init__(self, base_url: str):
        # Generous: sam local starts a container for a request when none is warm.
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
        response = self.send("list", resource, params={"limit": 1000})
        assert response.status_code == 200, response.text
        return {item["id"] for item in response.json()}

    def wait_until_ready(self, path: str, timeout: float) -> None:
        """Waits for app code to answer, which also starts a sam local container."""
        deadline = time.monotonic() + timeout
        while True:
            try:
                if self.http.get(path).status_code < 500:
                    return
            except httpx.TransportError:
                pass
            if time.monotonic() > deadline:
                raise TimeoutError(f"GET {self.http.base_url}{path} did not answer within {timeout:.0f}s")
            time.sleep(2)
