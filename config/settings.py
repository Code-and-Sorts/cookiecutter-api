from pydantic_settings import BaseSettings, SettingsConfigDict



class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    gcp_project_id: str
    firestore_database: str = "(default)"
    firestore_collection_kitty_cats: str = "kitty_cats"

    @property
    def collections(self) -> dict:
        return {
            "kitty_cats": self.firestore_collection_kitty_cats,
        }



def get_settings() -> Settings:
    return Settings()
