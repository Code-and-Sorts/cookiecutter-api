from pydantic_settings import BaseSettings, SettingsConfigDict



class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    gcp_project_id: str
    firestore_database: str = "(default)"
    firestore_collection_animals: str = "animals"

    @property
    def collections(self) -> dict:
        return {
            "animals": self.firestore_collection_animals,
        }



def get_settings() -> Settings:
    return Settings()
