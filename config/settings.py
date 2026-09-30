from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    aws_region: str = "us-east-1"
    dynamodb_table_name_kittenclaws: str = "kittenclaws"

    @property
    def tables(self) -> dict:
        return {
            "kittenclaws": self.dynamodb_table_name_kittenclaws,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
