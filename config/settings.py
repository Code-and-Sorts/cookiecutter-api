from functools import cache
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    gcp_project_id: str
    firestore_database: str = "(default)"
    firestore_emulator_host: str | None = None
    firestore_collection_animals: str = "animals"

    @property
    def collections(self) -> dict:
        return {
            "animals": self.firestore_collection_animals,
        }


@cache
def get_settings() -> Settings:
    # Lazy, so importing the app (unit tests, function indexing) needs no environment.
    return Settings()
