import subprocess
from datetime import datetime
import json
import os

def get_creation_datetime(filepath):
    """
    Extrahiert das Aufnahmedatum zuerst aus dem EXIF-Tag "Creation Date" und,
    falls nicht vorhanden, aus dem Tag "Create Date". Falls beide nicht verfügbar sind,
    wird das letzte Änderungsdatum der Datei verwendet.
    """
    try:
        # Verwende exiftool, um das "Creation Date" und "Create Date" zu extrahieren
        cmd = ['exiftool', '-CreationDate', '-CreateDate', '-json', filepath]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        exif_metadata = result.stdout.strip()

        # Parsing der JSON-Daten
        exif_json = json.loads(exif_metadata)
        if exif_json and len(exif_json) > 0:
            # Priorisiere das Tag "Creation Date", falls vorhanden
            if "CreationDate" in exif_json[0]:
                creation_time_str = exif_json[0]["CreationDate"]
            elif "CreateDate" in exif_json[0]:
                creation_time_str = exif_json[0]["CreateDate"]
            else:
                creation_time_str = None

            # Falls ein Datum gefunden wurde, konvertiere es in ein datetime-Objekt
            if creation_time_str:
                try:
                    creation_time = datetime.strptime(creation_time_str, '%Y:%m:%d %H:%M:%S%z')
                    return creation_time
                except ValueError:
                    print(f"Fehler bei der Konvertierung des Datums: {creation_time_str}. Verwende das Änderungsdatum der Datei.")

    except Exception as e:
        print(f"Fehler bei der EXIF-Analyse mit exiftool: {e}. Verwende das Änderungsdatum der Datei.")

    # Fallback: Verwende das letzte Änderungsdatum der Datei
    modification_time = datetime.fromtimestamp(os.path.getmtime(filepath))
    return modification_time