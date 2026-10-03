from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    cosmos_db_uri: str
    cosmos_db_key: str
    cosmos_db_database_name: str
    cosmos_db_emulator: bool = False
    container_name_cats: str = "cats"
    container_name_dogs: str = "dogs"
    container_name_visits: str = "visits"

    @property
    def container_names(self) -> dict:
        return {
            "cats": self.container_name_cats,
            "dogs": self.container_name_dogs,
            "visits": self.container_name_visits,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
