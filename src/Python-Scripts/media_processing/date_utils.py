import subprocess
from datetime import datetime
import json
import os

def get_creation_datetime(filepath):
    """
    Extrahiert das Aufnahmedatum aus dem EXIF-Tag "Create Date".
    Falls dieses nicht vorhanden ist, wird das letzte Änderungsdatum der Datei verwendet.
    """
    try:
        # Verwende exiftool, um das "Create Date" zu extrahieren
        cmd = ['exiftool', '-CreateDate', '-json', filepath]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        exif_metadata = result.stdout.strip()

        # Parsing der JSON-Daten
        exif_json = json.loads(exif_metadata)
        if exif_json and len(exif_json) > 0 and "CreateDate" in exif_json[0]:
            create_time_str = exif_json[0]["CreateDate"]

            # Konvertiere das Datum in ein datetime-Objekt
            try:
                create_time = datetime.strptime(create_time_str, '%Y:%m:%d %H:%M:%S')
                return create_time
            except ValueError:
                print(f"Fehler bei der Konvertierung des Datums: {create_time_str}. Verwende das Änderungsdatum der Datei.")

    except Exception as e:
        print(f"Fehler bei der EXIF-Analyse mit exiftool: {e}. Verwende das Änderungsdatum der Datei.")

    # Fallback: Verwende das letzte Änderungsdatum der Datei
    modification_time = datetime.fromtimestamp(os.path.getmtime(filepath))
    return modification_time