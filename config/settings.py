from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    gcp_project_id: str
    firestore_database: str = "(default)"
    firestore_emulator_host: str | None = None
    firestore_collection_cats: str = "cats"
    firestore_collection_dogs: str = "dogs"
    firestore_collection_visits: str = "visits"

    @property
    def collections(self) -> dict:
        return {
            "cats": self.firestore_collection_cats,
            "dogs": self.firestore_collection_dogs,
            "visits": self.firestore_collection_visits,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
