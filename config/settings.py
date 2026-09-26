from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    cosmos_db_uri: str
    cosmos_db_key: str
    cosmos_db_database_name: str
    container_name_kitty_cats: str = "kitty_cats"

    @property
    def container_names(self) -> dict:
        return {
            "kitty_cats": self.container_name_kitty_cats,
        }




def get_settings() -> Settings:
    return Settings()
