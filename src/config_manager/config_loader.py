# src/config_manager/config_loader.py

from dotenv import load_dotenv
from pathlib import Path
import logging

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

ENV_FILE_PATH = Path.home() / "Library/Application Support/Kurmann/Videoschnitt/.env"

def load_app_env():
    """
    Lädt die .env Datei aus ~/Library/Application Support/Kurmann/Videoschnitt/.env
    """
    if ENV_FILE_PATH.exists():
        load_dotenv(dotenv_path=ENV_FILE_PATH)
        logger.info(f".env Datei geladen von: {ENV_FILE_PATH}")
        return ENV_FILE_PATH
    else:
        logger.warning(f".env Datei nicht gefunden unter: {ENV_FILE_PATH}")
        return None