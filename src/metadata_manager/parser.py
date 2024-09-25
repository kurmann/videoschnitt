# src/metadata_manager/parser.py

import re
from datetime import datetime
from typing import Optional
import logging

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def parse_date_from_string(text: str) -> Optional[datetime]:
    """
    Extrahiert ein Datum im ISO-Format (YYYY-MM-DD) aus einem gegebenen String.

    Args:
        text (str): Der Text, aus dem das Datum extrahiert werden soll.

    Returns:
        datetime | None: Das extrahierte Datum als datetime-Objekt, oder None, wenn kein Datum gefunden wurde.
    """
    match = re.search(r"\d{4}-\d{2}-\d{2}", text)
    if match:
        date_str = match.group()
        try:
            parsed_date = datetime.strptime(date_str, "%Y-%m-%d")
            logger.info(f"Datum extrahiert: {parsed_date} aus Text: {text}")
            return parsed_date
        except ValueError as e:
            logger.error(f"Fehler beim Parsen des Datums: {e} in Text: {text}")
            return None
    logger.warning(f"Kein Datum im Text gefunden: {text}")
    return None

def parse_recording_date(metadata: dict) -> Optional[datetime]:
    """
    Extrahiert das Aufnahmedatum aus den Metadaten.

    Args:
        metadata (dict): Das Metadaten-Dictionary.

    Returns:
        datetime | None: Das extrahierte Aufnahmedatum als datetime-Objekt, oder None, wenn kein Datum gefunden wurde.
    """
    title_with_date = metadata.get("Title", "")
    if title_with_date:
        recording_date = parse_date_from_string(title_with_date)
        if recording_date:
            logger.info(f"Aufnahmedatum aus Titel extrahiert: {recording_date}")
            return recording_date

    # Fallback: Datum aus Dateinamen extrahieren
    file_name = metadata.get("FileName", "")
    recording_date = parse_date_from_string(file_name)
    if recording_date:
        logger.info(f"Aufnahmedatum aus Dateinamen extrahiert: {recording_date}")
        return recording_date

    logger.warning(f"Kein Aufnahmedatum gefunden in Metadaten: {metadata}")
    return None
