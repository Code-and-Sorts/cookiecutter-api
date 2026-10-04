"""Records every request the suite sends, with the stored document before and after each write."""

import json
from pathlib import Path
from urllib.parse import urlsplit

import httpx

from project import Project
from store import Store

WRITES = {"POST", "PATCH", "PUT", "DELETE"}


def _text(content: bytes) -> str | None:
    return content.decode("utf-8", "replace") if content else None


class Recorder:
    def __init__(self, path: Path, project: Project, store: Store, base_url: str):
        self._file = path.open("w")
        self._project = project
        self._store = store
        self._base_path = urlsplit(base_url).path.rstrip("/")
        self.test = None
        self._before = None

    def install(self, client: httpx.Client) -> None:
        client.event_hooks["request"].append(self._on_request)
        client.event_hooks["response"].append(self._on_response)

    def close(self) -> None:
        self._file.close()

    def _target(self, url: httpx.URL) -> tuple[str, str | None] | None:
        """The container and item id a request addresses, or None for a path outside the resources."""
        parts = url.path.removeprefix(self._base_path).strip("/").split("/")
        resource = next((r for r in self._project.resources if r.endpoint == parts[0]), None)
        if resource is None or len(parts) > 2:
            return None
        return resource.container, parts[1] if len(parts) == 2 else None

    def _document(self, container: str, item_id: str) -> dict | str | None:
        try:
            return self._store.get(container, item_id)
        except Exception as error:  # noqa: BLE001 - a read the store refuses (e.g. an id Firestore cannot hold) is reported, not raised.
            return f"<store read failed: {error!r}>"

    def _on_request(self, request: httpx.Request) -> None:
        target = self._target(request.url)
        self._before = None
        if request.method in WRITES and target and target[1]:
            self._before = self._document(*target)

    def _on_response(self, response: httpx.Response) -> None:
        response.read()
        request = response.request
        target = self._target(request.url)
        entry = {
            "test": self.test,
            "method": request.method,
            "url": str(request.url),
            "request_headers": dict(request.headers),
            "request_body": _text(request.content),
            "status": response.status_code,
            "response_headers": dict(response.headers),
            "response_body": response.text,
        }
        if request.method in WRITES and target:
            container, item_id = target
            if item_id is None and request.method == "POST" and response.status_code == 201:
                item_id = response.json().get("id")
            entry["container"] = container
            if target[1]:
                entry["before"] = self._before
            if item_id:
                entry["after"] = self._document(container, item_id)
        self._file.write(json.dumps(entry, default=str) + "\n")
        self._file.flush()
