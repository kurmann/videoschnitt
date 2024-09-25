# src/metadata_manager/loader.py

import subprocess
import json
import os
from typing import Dict
import logging

# Liste der benötigten Metadaten
METADATA_KEYS = [
    "FileName", "Directory", "FileSize", "FileModifyDate", "FileType", "MIMEType",
    "CreateDate", "Duration", "AudioFormat", "ImageWidth", "ImageHeight", "CompressorID",
    "CompressorName", "BitDepth", "VideoFrameRate", "Title", "Album", "Description", "Copyright",
    "Author", "Keywords", "AvgBitrate", "Producer", "Studio"
]

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def get_relevant_metadata(file_path: str) -> Dict[str, str]:
    """
    Extrahiert relevante Metadaten aus einer Datei mithilfe von ExifTool und gibt ein gefiltertes Dictionary zurück.

    Args:
        file_path (str): Der Pfad zur Mediendatei.

    Returns:
        dict: Ein Dictionary mit den gefilterten Metadaten als Strings.
    """
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"Die Datei '{file_path}' wurde nicht gefunden.")

    # Der Dateipfad muss in Anführungszeichen gesetzt werden, um Sonderzeichen zu behandeln
    command = f'exiftool -json "{file_path}"'
    try:
        result = subprocess.run(command, shell=True, capture_output=True, text=True, check=True)
        if not result.stdout:
            raise ValueError(f"Keine Ausgabe von ExifTool für '{file_path}'. Möglicherweise enthält die Datei keine Metadaten.")

        metadata_list = json.loads(result.stdout)
        metadata = metadata_list[0]  # Wir nehmen an, dass nur eine Datei übergeben wird

        # Filtern der gewünschten Metadaten und Standardwerte auf leere Strings setzen
        filtered_metadata = {key: metadata.get(key, '') for key in METADATA_KEYS}

        logger.info(f"Relevante Metadaten geladen für: {file_path}")
        return filtered_metadata
    except subprocess.CalledProcessError as e:
        error_message = (
            f"Fehler beim Extrahieren der Metadaten für '{file_path}'.\n"
            f"Exit Code: {e.returncode}\n"
            f"Fehlerausgabe: {e.stderr.strip() if e.stderr else 'Keine Fehlermeldung verfügbar.'}\n"
            f"Vollständiger Befehl: {command}"
        )
        logger.error(error_message)
        raise ValueError(error_message)
    except json.JSONDecodeError:
        error_message = f"Ungültige JSON-Ausgabe von ExifTool für '{file_path}'."
        logger.error(error_message)
        raise ValueError(error_message)