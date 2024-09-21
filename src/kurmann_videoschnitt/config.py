# src/kurmann_videoschnitt/config.py

from dotenv import load_dotenv
from pathlib import Path
import os

def get_application_support_path():
    """
    Gibt den Pfad zum Application Support Verzeichnis für Kurmann Videoschnitt zurück.
    """
    return Path.home() / "Library" / "Application Support" / "Kurmann" / "Videoschnitt"

# Pfad zur .env-Datei unter Application Support
APP_SUPPORT_DIR = get_application_support_path()
ENV_FILE = APP_SUPPORT_DIR / ".env"

# Finde und lade die .env-Datei
if ENV_FILE.exists():
    load_dotenv(str(ENV_FILE))

# Original Media Integrator Konfiguration
ORIGINAL_MEDIA_SOURCE_DIR = os.getenv('original_media_source_dir')
ORIGINAL_MEDIA_DESTINATION_DIR = os.getenv('original_media_destination_dir')
ORIGINAL_MEDIA_COMPRESSION_DIR = os.getenv('original_media_compression_dir')
ORIGINAL_MEDIA_KEEP_ORIGINAL_PRORES = os.getenv('original_media_keep_original_prores', 'False').lower() == 'true'

# Weitere Konfigurationswerte können hier hinzugefügt werden