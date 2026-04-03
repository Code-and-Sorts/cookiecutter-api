"""
Main entry point for AWS Lambda.
Routes API Gateway events to the appropriate handler.
"""
from blueprints.{{cookiecutter.project_slug}}_api import (
    get_by_id,
    get_list,
    create,
    update,
    delete
)


def lambda_handler(event, context):
    http_method = event.get("httpMethod", "")
    resource = event.get("resource", "")

    if http_method == "GET" and "{item_id}" in resource:
        return get_by_id(event)
    elif http_method == "GET":
        return get_list(event)
    elif http_method == "POST":
        return create(event)
    elif http_method == "PATCH":
        return update(event)
    elif http_method == "DELETE":
        return delete(event)
    else:
        return {
            "statusCode": 404,
            "headers": {"Content-Type": "application/json"},
            "body": '{"message": "Not Found"}'
        }
