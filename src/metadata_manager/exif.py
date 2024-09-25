# src/metadata_manager/exif.py

import subprocess
import json
import os
from datetime import datetime, timezone
from typing import Optional
import logging

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def get_creation_datetime(filepath: str) -> Optional[datetime]:
    """
    Bestimmt das Erstellungsdatum für Videodateien (CreationDate) und Bilddateien (DateTimeOriginal).
    Berücksichtigt Zeitzoneninformationen durch OffsetTimeOriginal.

    Args:
        filepath (str): Der Pfad zur Mediendatei.

    Returns:
        datetime | None: Das Erstellungsdatum als datetime-Objekt, oder None, wenn nicht ermittelt werden konnte.
    """
    try:
        # Erkennung, ob es sich um eine Videodatei handelt
        file_extension = os.path.splitext(filepath)[1].lower()
        is_video = file_extension in ['.mov', '.mp4', '.m4v', '.avi', '.hevc']

        # Verwende exiftool, um relevante Metadaten auszulesen
        cmd = [
            'exiftool',
            '-CreationDate',        # Für Videodateien
            '-ContentCreateDate',   # Für Videodateien
            '-DateTimeOriginal',    # Für Bilddateien
            '-OffsetTimeOriginal',  # Zeitzoneninformation
            '-json',
            filepath
        ]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

        # Debug-Ausgabe der rohen exiftool-Daten
        exif_metadata = result.stdout.strip()
        logger.debug(f"Rohdaten von exiftool: {exif_metadata}")

        exif_json = json.loads(exif_metadata)

        if exif_json and len(exif_json) > 0:
            if is_video:
                # Für Videodateien: Primär CreationDate verwenden
                creation_time_str = exif_json[0].get("CreationDate") or exif_json[0].get("ContentCreateDate")
            else:
                # Für Bilddateien: DateTimeOriginal verwenden
                creation_time_str = exif_json[0].get("DateTimeOriginal")
            
            offset_time_original = exif_json[0].get("OffsetTimeOriginal")

            logger.debug(f"Ausgelesenes Datum: {creation_time_str}, OffsetTimeOriginal: {offset_time_original}")

            if creation_time_str:
                # Versuche verschiedene Formate zu parsen
                datetime_formats = [
                    '%Y:%m:%d %H:%M:%S%z',          # Standardformat mit Zeitzone
                    '%Y:%m:%d %H:%M:%S.%f%z',       # Format mit Millisekunden und Zeitzone
                    '%Y:%m:%d %H:%M:%S',            # Standardformat ohne Zeitzone
                    '%Y:%m:%d %H:%M:%S.%f'          # Format mit Millisekunden ohne Zeitzone
                ]

                for dt_format in datetime_formats:
                    try:
                        if offset_time_original:
                            # Füge Zeitzoneninformationen hinzu, falls vorhanden
                            creation_time_str_with_offset = creation_time_str + offset_time_original
                            datetime_with_timezone = datetime.strptime(creation_time_str_with_offset, dt_format)
                        else:
                            datetime_with_timezone = datetime.strptime(creation_time_str, dt_format)

                        logger.info(f"Geparstes Datum mit Zeitzone: {datetime_with_timezone}")
                        return datetime_with_timezone
                    except ValueError:
                        continue

                logger.error("Fehler beim Parsen des Datums: Kein passendes Format gefunden. Verwende das Änderungsdatum der Datei.")
        
    except Exception as e:
        logger.error(f"Fehler bei der EXIF-Analyse mit exiftool: {e}. Verwende das Änderungsdatum der Datei.")

    # Fallback: Verwende das Änderungsdatum der Datei
    try:
        modification_time = os.path.getmtime(filepath)
        datetime_with_timezone = datetime.fromtimestamp(modification_time, tz=timezone.utc).astimezone()
        logger.info(f"Verwende das Änderungsdatum der Datei für {filepath}: {datetime_with_timezone}")
        return datetime_with_timezone
    except Exception as e:
        logger.error(f"Fehler beim Abrufen des Änderungsdatums der Datei {filepath}: {e}")
        return None
