from typing import List
{% if cookiecutter.cloud_service == 'Azure Function App' -%}
import azure.functions as func
{%- endif %}
{% if cookiecutter.cloud_service == 'GCP Cloud Function' -%}
from flask import Request
{%- endif %}
{% if cookiecutter.cloud_service == 'AWS Lambda' -%}
import json
{%- endif %}
from services import {{ cookiecutter.project_class_name }}Service
from models import {{ cookiecutter.project_class_name }}Response, {{ cookiecutter.project_class_name }}, {{ cookiecutter.project_class_name }}IdValidation

class {{ cookiecutter.project_class_name }}Controller:
    def __init__(self, service: {{ cookiecutter.project_class_name }}Service):
        self.service = service
{%- if cookiecutter.cloud_service == 'Azure Function App' %}

    def get_by_id(self, req: func.HttpRequest) -> {{ cookiecutter.project_class_name }}Response:
        item_id: str = req.route_params.get('item_id')
        {{ cookiecutter.project_class_name }}IdValidation(id=item_id)
        return self.service.get_by_id(item_id)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}

    def get_by_id(self, request: Request) -> {{ cookiecutter.project_class_name }}Response:
        # For GCP Cloud Functions, extract item_id from path
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        {{ cookiecutter.project_class_name }}IdValidation(id=item_id)
        return self.service.get_by_id(item_id)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}

    def get_by_id(self, event: dict) -> {{ cookiecutter.project_class_name }}Response:
        item_id: str = event.get("pathParameters", {}).get("item_id")
        {{ cookiecutter.project_class_name }}IdValidation(id=item_id)
        return self.service.get_by_id(item_id)
{%- endif %}

    def get_list(self) -> List[{{ cookiecutter.project_class_name }}Response]:
        return self.service.get_list()
{%- if cookiecutter.cloud_service == 'Azure Function App' %}

    def create(self, req: func.HttpRequest) -> {{ cookiecutter.project_class_name }}Response:
        item_json: dict = req.get_json()
        item = {{ cookiecutter.project_class_name }}(**item_json)
        return self.service.create(item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}

    def create(self, request: Request) -> {{ cookiecutter.project_class_name }}Response:
        item_json: dict = request.get_json()
        item = {{ cookiecutter.project_class_name }}(**item_json)
        return self.service.create(item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}

    def create(self, event: dict) -> {{ cookiecutter.project_class_name }}Response:
        item_json: dict = json.loads(event.get("body", "{}"))
        item = {{ cookiecutter.project_class_name }}(**item_json)
        return self.service.create(item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'Azure Function App' %}

    def update(self, req: func.HttpRequest) -> {{ cookiecutter.project_class_name }}Response:
        item_id: str = req.route_params.get('item_id')
        item_data: dict = req.get_json()
        item = {{ cookiecutter.project_class_name }}(**item_data)
        item.id = item_id
        return self.service.update(item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}

    def update(self, request: Request) -> {{ cookiecutter.project_class_name }}Response:
        # For GCP Cloud Functions, extract item_id from path
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        item_data: dict = request.get_json()
        item = {{ cookiecutter.project_class_name }}(**item_data)
        item.id = item_id
        return self.service.update(item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}

    def update(self, event: dict) -> {{ cookiecutter.project_class_name }}Response:
        item_id: str = event.get("pathParameters", {}).get("item_id")
        item_data: dict = json.loads(event.get("body", "{}"))
        item = {{ cookiecutter.project_class_name }}(**item_data)
        item.id = item_id
        return self.service.update(item)
{%- endif %}
{%- if cookiecutter.cloud_service == 'Azure Function App' %}

    def soft_delete(self, req: func.HttpRequest) -> None:
        item_id: str = req.route_params.get('item_id')
        self.service.soft_delete(item_id)
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}

    def soft_delete(self, request: Request) -> None:
        # For GCP Cloud Functions, extract item_id from path
        path_parts = request.path.strip('/').split('/')
        item_id: str = path_parts[-1] if len(path_parts) > 0 else None
        self.service.soft_delete(item_id)
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}

    def soft_delete(self, event: dict) -> None:
        item_id: str = event.get("pathParameters", {}).get("item_id")
        self.service.soft_delete(item_id)
{%- endif %}
