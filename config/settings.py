from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    gcp_project_id: str
    firestore_database: str = "(default)"
    firestore_emulator_host: str | None = None
    firestore_collection_kittenclaws: str = "kittenclaws"

    @property
    def collections(self) -> dict:
        return {
            "kittenclaws": self.firestore_collection_kittenclaws,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
