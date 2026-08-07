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
{%- for container in resources | map(attribute='container') | unique %}
    container_name_{{ container | replace('-', '_') | lower }}: str = "{{ container }}"
{%- endfor %}

    @property
    def container_names(self) -> dict:
        return {
{%- for container in resources | map(attribute='container') | unique %}
            "{{ container }}": self.container_name_{{ container | replace('-', '_') | lower }},
{%- endfor %}
        }
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
{%- for container in resources | map(attribute='container') | unique %}
    firestore_collection_{{ container | replace('-', '_') | lower }}: str = "{{ container }}"
{%- endfor %}

    @property
    def collections(self) -> dict:
        return {
{%- for container in resources | map(attribute='container') | unique %}
            "{{ container }}": self.firestore_collection_{{ container | replace('-', '_') | lower }},
{%- endfor %}
        }
{%- endif %}
{% if cloud_service == 'AWS Lambda' -%}
class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    aws_region: str = "us-east-1"
{%- for container in resources | map(attribute='container') | unique %}
    dynamodb_table_name_{{ container | replace('-', '_') | lower }}: str = "{{ container }}"
{%- endfor %}

    @property
    def tables(self) -> dict:
        return {
{%- for container in resources | map(attribute='container') | unique %}
            "{{ container }}": self.dynamodb_table_name_{{ container | replace('-', '_') | lower }},
{%- endfor %}
        }
{%- endif %}


def get_settings() -> Settings:
    return Settings()
