# src/metadata_manager/loader.py

import subprocess
import json
import os
from typing import Dict
import logging

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def load_metadata(file_path: str) -> Dict:
    """
    Lädt die Metadaten einer Datei mithilfe von ExifTool und gibt sie als Dictionary zurück.

    Args:
        file_path (str): Der Pfad zur Datei, aus der die Metadaten geladen werden sollen.

    Returns:
        dict: Ein Dictionary mit den Metadaten der Datei.

    Raises:
        FileNotFoundError: Wenn die Datei nicht existiert.
        ValueError: Wenn keine Metadaten extrahiert werden konnten.
    """
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"Die Datei '{file_path}' wurde nicht gefunden.")

    command = ['exiftool', '-json', file_path]
    try:
        result = subprocess.run(command, check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        metadata_list = json.loads(result.stdout)
        if not metadata_list:
            raise ValueError(f"Keine Metadaten von ExifTool für '{file_path}' erhalten.")
        metadata = metadata_list[0]
        logger.info(f"Metadaten geladen für: {file_path}")
        return metadata
    except subprocess.CalledProcessError as e:
        error_message = (
            f"Fehler beim Ausführen von ExifTool für '{file_path}'.\n"
            f"Rückgabecode: {e.returncode}\n"
            f"Fehlerausgabe: {e.stderr.strip()}"
        )
        logger.error(error_message)
        raise ValueError(error_message)
    except json.JSONDecodeError:
        error_message = f"Ungültige JSON-Ausgabe von ExifTool für '{file_path}'."
        logger.error(error_message)
        raise ValueError(error_message)

def load_exif_data(file_path: str) -> Dict:
    """
    Lädt spezifische Exif-Daten einer Datei.

    Args:
        file_path (str): Der Pfad zur Datei.

    Returns:
        dict: Ein Dictionary mit spezifischen Exif-Daten.

    Raises:
        FileNotFoundError: Wenn die Datei nicht existiert.
        ValueError: Wenn keine Exif-Daten extrahiert werden konnten.
    """
    metadata = load_metadata(file_path)
    # Hier kannst du spezifische Exif-Daten extrahieren, falls benötigt
    # Zum Beispiel:
    exif_data = {
        "FileName": metadata.get("FileName", ""),
        "Directory": metadata.get("Directory", ""),
        "FileSize": metadata.get("FileSize", ""),
        "CreateDate": metadata.get("CreateDate", ""),
        "DateTimeOriginal": metadata.get("DateTimeOriginal", ""),
        # Füge weitere benötigte Felder hinzu
    }
    logger.info(f"Spezifische Exif-Daten extrahiert für: {file_path}")
    return exif_data
