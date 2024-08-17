import subprocess
import json
import os
from datetime import datetime, timezone

def get_creation_datetime(filepath):
    """
    Extrahiert das Aufnahmedatum aus den EXIF-Daten oder verwendet das Änderungsdatum der Datei als Fallback.
    Gibt ein datetime-Objekt mit Zeitzone zurück.
    """
    possible_formats = [
        '%Y:%m:%d %H:%M:%S%z',  # Standard EXIF-Format mit Zeitzone
        '%Y:%m:%d %H:%M:%S',    # Standard EXIF-Format ohne Zeitzone
        '%Y-%m-%dT%H:%M:%S%z',  # ISO-Format mit Zeitzone
        '%Y-%m-%dT%H:%M:%S',    # ISO-Format ohne Zeitzone
    ]

    try:
        # Verwende exiftool, um die relevanten Datumsfelder für Videos und Bilder auszulesen
        cmd = [
            'exiftool',
            '-CreationDate',        # Für Videos (MOV/MP4 etc.)
            '-ContentCreateDate',   # Für Videos (MOV/MP4 etc.)
            '-DateTimeOriginal',    # Für Bilder (HEIC/JPG etc.)
            '-CreateDate',          # Allgemein
            '-json',
            filepath
        ]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        exif_metadata = result.stdout.strip()

        # Parsing der JSON-Daten
        exif_json = json.loads(exif_metadata)
        if exif_json and len(exif_json) > 0:
            # Priorisierte Reihenfolge der Felder für Videos und Bilder
            preferred_fields = [
                "CreationDate",
                "ContentCreateDate",
                "DateTimeOriginal",
                "CreateDate"
            ]

            for date_key in preferred_fields:
                if date_key in exif_json[0]:
                    creation_time_str = exif_json[0][date_key]

                    # Versuche, das Datum mit den möglichen Formaten zu parsen
                    for date_format in possible_formats:
                        try:
                            creation_time = datetime.strptime(creation_time_str, date_format)
                            # Wenn keine Zeitzone vorhanden ist, füge die lokale Zeitzone hinzu
                            if creation_time.tzinfo is None:
                                creation_time = creation_time.replace(tzinfo=timezone.utc).astimezone()
                            return creation_time
                        except ValueError:
                            continue  # Fahre mit dem nächsten Format fort

            print(f"Fehler bei der Konvertierung des Datums: {creation_time_str}. Verwende das Änderungsdatum der Datei.")

    except Exception as e:
        print(f"Fehler bei der EXIF-Analyse mit exiftool: {e}. Verwende das Änderungsdatum der Datei.")

    # Fallback: Verwende das Änderungsdatum der Datei
    modification_time = os.path.getmtime(filepath)
    return datetime.fromtimestamp(modification_time, tz=timezone.utc).astimezone()