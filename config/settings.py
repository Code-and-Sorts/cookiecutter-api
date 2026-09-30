from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    cosmos_db_uri: str
    cosmos_db_key: str
    cosmos_db_database_name: str
    container_name_animals: str = "animals"

    @property
    def container_names(self) -> dict:
        return {
            "animals": self.container_name_animals,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
