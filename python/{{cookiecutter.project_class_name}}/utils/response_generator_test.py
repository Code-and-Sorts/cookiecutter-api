from models import {{ cookiecutter.project_class_name }}Response
from .response_generator import response_generator

def describe_response_generator():
    def test_list_item_response_single_item():
        item_response = {{ cookiecutter.project_class_name }}Response(id="935e5045-4a1c-46c9-8e26-9d9d5c2597f3",name="mockName",type="mockType")
        item_response_list = [item_response]
        response = response_generator(item_response_list, 200)

{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        assert response.get_body().decode() == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response.status_code == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        assert response[0] == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response[1] == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        assert response["body"] == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response["statusCode"] == 200
{%- endif %}

    def test_empty_list():
        item_response_list = []
        response = response_generator(item_response_list, 200)

{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        assert response.get_body().decode() == '[]'
        assert response.status_code == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        assert response[0] == '[]'
        assert response[1] == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        assert response["body"] == '[]'
        assert response["statusCode"] == 200
{%- endif %}

    def test_list_item_response_multiple_items():
        item_response = {{ cookiecutter.project_class_name }}Response(id="935e5045-4a1c-46c9-8e26-9d9d5c2597f3",name="mockName",type="mockType")
        item_response_list = [item_response, item_response]
        response = response_generator(item_response_list, 200)

{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        assert response.get_body().decode() == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}, {"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response.status_code == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        assert response[0] == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}, {"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response[1] == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        assert response["body"] == '[{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}, {"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}]'
        assert response["statusCode"] == 200
{%- endif %}

    def test_item_response():
        item_response = {{ cookiecutter.project_class_name }}Response(id="935e5045-4a1c-46c9-8e26-9d9d5c2597f3",name="mockName",type="mockType")
        response = response_generator(item_response, 200)

{%- if cookiecutter.cloud_service == 'Azure Function App' %}
        assert response.get_body().decode() == '{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}'
        assert response.status_code == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'GCP Cloud Function' %}
        assert response[0] == '{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}'
        assert response[1] == 200
{%- endif %}
{%- if cookiecutter.cloud_service == 'AWS Lambda' %}
        assert response["body"] == '{"name": "mockName", "type": "mockType", "id": "935e5045-4a1c-46c9-8e26-9d9d5c2597f3"}'
        assert response["statusCode"] == 200
{%- endif %}
