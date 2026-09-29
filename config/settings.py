from pydantic_settings import BaseSettings, SettingsConfigDict




class Settings(BaseSettings):
    """Validated application configuration loaded from environment variables.

    Missing required values raise a clear ``ValidationError`` at startup
    instead of failing later with an opaque ``None`` access.
    """

    model_config = SettingsConfigDict(case_sensitive=False, extra="ignore")

    aws_region: str = "us-east-1"
    dynamodb_table_name_animals: str = "animals"

    @property
    def tables(self) -> dict:
        return {
            "animals": self.dynamodb_table_name_animals,
        }


def get_settings() -> Settings:
    return Settings()
