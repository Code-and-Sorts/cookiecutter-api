from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    aws_region: str = "us-east-1"
    aws_endpoint_url_dynamodb: str | None = None
    dynamodb_table_name_animals: str = "animals"

    @property
    def tables(self) -> dict:
        return {
            "animals": self.dynamodb_table_name_animals,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
