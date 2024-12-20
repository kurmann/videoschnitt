# src/config_manager/config_loader.py

from dotenv import load_dotenv
from pathlib import Path

ENV_FILE_PATH = Path.home() / "Library/Application Support/Kurmann/Videoschnitt/.env"

def load_app_env():
    """
    Lädt die .env Datei aus ~/Library/Application Support/Kurmann/Videoschnitt/.env
    """
    if ENV_FILE_PATH.exists():
        load_dotenv(dotenv_path=ENV_FILE_PATH)
        print(f".env Datei geladen von: {ENV_FILE_PATH}")
        return ENV_FILE_PATH
    else:
        print(f"Die .env Datei wurde nicht gefunden unter: {ENV_FILE_PATH}")
        return None