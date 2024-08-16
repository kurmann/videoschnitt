import subprocess
from datetime import datetime
import json
import os

def get_creation_datetime(filepath):
    """
    Extrahiert das Aufnahmedatum aus den EXIF-Daten oder verwendet das Änderungsdatum der Datei als Fallback.
    Unterstützt verschiedene Datumsformate.
    """
    possible_formats = [
        '%Y:%m:%d %H:%M:%S%z',  # Standard EXIF-Format mit Zeitzone
        '%Y:%m:%d %H:%M:%S',    # Standard EXIF-Format ohne Zeitzone
        '%Y-%m-%dT%H:%M:%S%z',  # ISO-Format mit Zeitzone
        '%Y-%m-%dT%H:%M:%S',    # ISO-Format ohne Zeitzone
    ]
    
    try:
        # Verwende exiftool, um das "Create Date" zu extrahieren
        cmd = ['exiftool', '-CreateDate', '-json', filepath]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        exif_metadata = result.stdout.strip()
        
        # Parsing der JSON-Daten
        exif_json = json.loads(exif_metadata)
        if exif_json and len(exif_json) > 0 and "CreateDate" in exif_json[0]:
            creation_time_str = exif_json[0]["CreateDate"]
            
            # Versuche, das Datum mit den möglichen Formaten zu parsen
            for date_format in possible_formats:
                try:
                    creation_time = datetime.strptime(creation_time_str, date_format)
                    return creation_time
                except ValueError:
                    continue  # Fahre mit dem nächsten Format fort

            print(f"Fehler bei der Konvertierung des Datums: {creation_time_str}. Verwende das Änderungsdatum der Datei.")

    except Exception as e:
        print(f"Fehler bei der EXIF-Analyse mit exiftool: {e}. Verwende das Änderungsdatum der Datei.")

    # Fallback: Verwende das Änderungsdatum der Datei
    modification_time = os.path.getmtime(filepath)
    return datetime.fromtimestamp(modification_time)