from pydantic_settings import BaseSettings, SettingsConfigDict


{% if cloud_service == 'Azure Function App' -%}
class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    cosmos_db_uri: str
    cosmos_db_key: str
    cosmos_db_database_name: str
    cosmos_db_container_name: str
{%- endif %}
{% if cloud_service == 'GCP Cloud Function' -%}
class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    gcp_project_id: str
    firestore_database: str = "(default)"
    firestore_collection: str = "{{ project_slug }}"
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    aws_region: str = "us-east-1"
    dynamodb_table_name: str = "{{ project_slug }}"
{%- endif %}


def get_settings() -> Settings:
    return Settings()
