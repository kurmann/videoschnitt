# metadata_manager.py

"""
Das 'metadata_manager' Modul ist verantwortlich für das Auslesen von Metadaten aus Dateien.
Es stellt Funktionen zur Verfügung, um Metadaten mithilfe von ExifTool zu extrahieren und
das Aufnahmedatum aus Dateinamen zu parsen.
"""

import os
import re
import subprocess
import json
from datetime import datetime

# Liste der benötigten Metadaten
METADATA_KEYS = [
    "FileName", "Directory", "FileSize", "FileModificationDateTime", "FileType", "MIMEType", 
    "CreateDate", "Duration", "AudioFormat", "ImageWidth", "ImageHeight", "CompressorID",
    "CompressorName", "BitDepth", "VideoFrameRate", "Title", "Album", "Description", "Copyright", 
    "Author", "Keywords", "AvgBitrate", "Producer", "Studio"
]
def get_metadata(file_path: str) -> dict:
    """
    Extrahiert die Metadaten aus einer Datei mithilfe von ExifTool und gibt ein strukturiertes Dictionary zurück.

    Args:
        file_path (str): Der Pfad zur Datei, aus der die Metadaten extrahiert werden sollen.

    Returns:
        dict: Ein Dictionary, das die relevanten Metadaten enthält.

    Raises:
        FileNotFoundError: Wenn die Datei nicht gefunden wird.
        ValueError: Wenn keine Metadaten extrahiert werden können.
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

        # Filtern der gewünschten Metadaten
        # Standardwert von 'N/A' zu '' geändert
        filtered_metadata = {key: metadata.get(key, '') for key in METADATA_KEYS}

        return filtered_metadata
    except subprocess.CalledProcessError as e:
        error_message = (
            f"Fehler beim Extrahieren der Metadaten für '{file_path}'.\n"
            f"Exit Code: {e.returncode}\n"
            f"Fehlerausgabe: {e.stderr.strip() if e.stderr else 'Keine Fehlermeldung verfügbar.'}\n"
            f"Vollständiger Befehl: {command}"
        )
        raise ValueError(error_message)
    
def parse_recording_date(file_path: str) -> datetime | None:
    """
    Extrahiert das Aufnahmedatum aus dem Dateinamen.

    Der Dateiname muss im Format 'YYYY-MM-DD <Rest des Dateinamens>' vorliegen.
    Wenn kein Datum im Dateinamen gefunden wird, wird None zurückgegeben.

    Args:
        file_path (str): Pfad zur Datei, deren Dateiname das Datum enthalten soll.

    Returns:
        datetime | None: Das extrahierte Datum als datetime-Objekt, oder None, wenn kein Datum gefunden wurde.
    """
    file_name = os.path.basename(file_path)
    match = re.search(r"\d{4}-\d{2}-\d{2}", file_name)
    if match:
        date_str = match.group()
        return datetime.strptime(date_str, "%Y-%m-%d")
    return None