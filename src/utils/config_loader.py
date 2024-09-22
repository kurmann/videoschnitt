# src/utils/config_loader.py

from dotenv import load_dotenv
from pathlib import Path
import os

def load_app_env():
    """
    Lädt die .env Datei aus ~/Library/Application Support/Kurmann/Videoschnitt/.env
    """
    env_path = Path.home() / "Library/Application Support/Kurmann/Videoschnitt/.env"
    if env_path.exists():
        load_dotenv(dotenv_path=env_path)
        return env_path
    else:
        return None