{%- set prefix, mapping = {
    'Azure Function App': ('container_name', 'container_names'),
    'GCP Cloud Function': ('firestore_collection', 'collections'),
    'AWS Lambda': ('dynamodb_table_name', 'tables'),
}[cloud_service] -%}
{%- set containers = path_resources | unique(attribute='container') | list -%}
from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")
{% if cloud_service == 'Azure Function App' %}
    cosmos_db_uri: str
    cosmos_db_key: str
    cosmos_db_database_name: str
{%- elif cloud_service == 'GCP Cloud Function' %}
    gcp_project_id: str
    firestore_database: str = "(default)"
{%- else %}
    aws_region: str = "us-east-1"
{%- endif %}
{%- for c in containers %}
    {{ prefix }}_{{ c.container_key | lower }}: str = "{{ c.container }}"
{%- endfor %}

    @property
    def {{ mapping }}(self) -> dict:
        return {
{%- for c in containers %}
            "{{ c.container }}": self.{{ prefix }}_{{ c.container_key | lower }},
{%- endfor %}
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
