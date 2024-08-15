import subprocess
from datetime import datetime
import json

def get_creation_datetime(filepath):
    """
    Extrahiert das Aufnahmedatum ausschließlich aus dem EXIF-Tag "Creation Date" mit exiftool.
    """
    try:
        # Verwende exiftool, um das "Creation Date" zu extrahieren
        cmd = ['exiftool', '-CreationDate', '-json', filepath]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        exif_metadata = result.stdout.strip()
        
        # Parsing der JSON-Daten
        exif_json = json.loads(exif_metadata)
        if exif_json and len(exif_json) > 0 and "CreationDate" in exif_json[0]:
            creation_time_str = exif_json[0]["CreationDate"]
            # Konvertiere das Datum in ein datetime-Objekt
            creation_time = datetime.strptime(creation_time_str, '%Y:%m:%d %H:%M:%S%z')
            return creation_time

    except Exception as e:
        print(f"Fehler bei der EXIF-Analyse mit exiftool: {e}. Verwende das aktuelle Datum.")
    
    # Falls etwas schiefgeht, verwende das aktuelle Datum als Fallback
    return datetime.now()