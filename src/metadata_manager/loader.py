# src/metadata_manager/loader.py

import subprocess
import json
import os
from typing import Dict, Any
import logging
from metadata_manager.utils import get_video_codec, get_bitrate, is_hevc_a
from metadata_manager.exif import get_creation_datetime

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

def get_metadata_with_exiftool(file_path: str) -> Dict[str, str]:
    """
    Extrahiert relevante Metadaten aus einer Datei mithilfe von ExifTool.
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

        logger.debug(f"Relevante Metadaten geladen für: {file_path}")
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

def aggregate_metadata(file_path: str, include_source: bool = False) -> Dict[str, Any]:
    """
    Aggregiert Metadaten aus mehreren Quellen (ExifTool, FFprobe) und gibt ein kombiniertes Dictionary zurück.
    """
    try:
        # Holen der Metadaten von ExifTool
        exif_metadata = get_metadata_with_exiftool(file_path)

        if include_source:
            aggregated_metadata = {key: {"value": value, "source": "ExifTool"} 
                                  for key, value in exif_metadata.items() if value}
        else:
            aggregated_metadata = exif_metadata.copy()

        # Holen zusätzlicher Metadaten von FFprobe über utils.py
        video_codec = get_video_codec(file_path)
        bitrate = get_bitrate(file_path)
        hevc_a = is_hevc_a(file_path)

        if include_source:
            if video_codec:
                aggregated_metadata["VideoCodec"] = {"value": video_codec, "source": "FFprobe"}
            if bitrate:
                aggregated_metadata["Bitrate"] = {"value": bitrate, "source": "FFprobe"}
            aggregated_metadata["IsHEVCA"] = {"value": hevc_a, "source": "FFprobe"}
        else:
            if video_codec:
                aggregated_metadata["VideoCodec"] = video_codec
            if bitrate:
                aggregated_metadata["Bitrate"] = bitrate
            aggregated_metadata["IsHEVCA"] = hevc_a

        # Holen des Erstellungsdatums über exif.py
        creation_datetime = get_creation_datetime(file_path)
        if creation_datetime:
            if include_source:
                aggregated_metadata["CreationDateTime"] = {
                    "value": creation_datetime.isoformat(), 
                    "source": "ExifTool"
                }
            else:
                aggregated_metadata["CreationDateTime"] = creation_datetime.isoformat()

        return aggregated_metadata

    except Exception as e:
        logger.error(f"Fehler beim Aggregieren der Metadaten: {e}")
        raise