from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    aws_region: str = "us-east-1"
    aws_endpoint_url_dynamodb: str | None = None
    dynamodb_table_name_cats: str = "cats"
    dynamodb_table_name_dogs: str = "dogs"
    dynamodb_table_name_visits: str = "visits"

    @property
    def tables(self) -> dict:
        return {
            "cats": self.dynamodb_table_name_cats,
            "dogs": self.dynamodb_table_name_dogs,
            "visits": self.dynamodb_table_name_visits,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
