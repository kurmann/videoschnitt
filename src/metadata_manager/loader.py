# src/metadata_manager/loader.py

import subprocess
import json
import os
from pathlib import Path
from typing import Optional, Dict
import logging

from src.metadata_manager.parser import parse_recording_date

from .utils import get_video_codec
from .exif import get_creation_datetime  # Import aus exif.py
from .models import Metadata

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

def get_metadata(file_path: str) -> Metadata:
    """
    Lädt und parst die Metadaten einer Mediendatei und gibt ein Metadata-Objekt zurück.

    Args:
        file_path (str): Der Pfad zur Mediendatei.

    Returns:
        Metadata: Ein Metadata-Objekt mit den geladenen Metadaten.
    """
    metadata_dict = load_metadata(file_path)
    recording_date = parse_recording_date(metadata_dict)
    metadata = Metadata(
        file_name=metadata_dict.get("FileName", ""),
        directory=metadata_dict.get("Directory", ""),
        file_size=int(metadata_dict.get("FileSize", 0)),
        file_modification_datetime=metadata_dict.get("FileModificationDateTime", ""),
        file_type=metadata_dict.get("FileType", ""),
        mime_type=metadata_dict.get("MIMEType", ""),
        create_date=metadata_dict.get("CreateDate"),
        duration=metadata_dict.get("Duration"),
        audio_format=metadata_dict.get("AudioFormat"),
        image_width=int(metadata_dict.get("ImageWidth", 0)),
        image_height=int(metadata_dict.get("ImageHeight", 0)),
        compressor_id=metadata_dict.get("CompressorID"),
        compressor_name=metadata_dict.get("CompressorName"),
        bit_depth=int(metadata_dict.get("BitDepth", 0)) if metadata_dict.get("BitDepth") else None,
        video_frame_rate=metadata_dict.get("VideoFrameRate"),
        title=metadata_dict.get("Title"),
        album=metadata_dict.get("Album"),
        description=metadata_dict.get("Description"),
        copyright=metadata_dict.get("Copyright"),
        author=metadata_dict.get("Author"),
        keywords=metadata_dict.get("Keywords"),
        avg_bitrate=int(metadata_dict.get("AvgBitrate", 0)) if metadata_dict.get("AvgBitrate") else None,
        producer=metadata_dict.get("Producer"),
        studio=metadata_dict.get("Studio"),
        producers=metadata_dict.get("Producers", []),
        directors=metadata_dict.get("Directors", []),
        published=recording_date
    )
    return metadata