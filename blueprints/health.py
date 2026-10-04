ENDPOINT = "health"
BODY = {"status": "ok"}


def health(request, item_id: str | None) -> tuple[int, dict]:
    return 200, BODY


# (HTTP method, path has an item id) -> handler
ROUTES = {
    ("GET", False): health,
}
