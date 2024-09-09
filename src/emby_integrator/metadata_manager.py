import subprocess
import json
import os

# Liste der benötigten Metadaten
METADATA_KEYS = [
    "FileName", "Directory", "FileSize", "FileModificationDateTime", "FileType", "MIMEType", 
    "CreateDate", "Duration", "AudioFormat", "ImageWidth", "ImageHeight", "CompressorName", 
    "BitDepth", "VideoFrameRate", "Title", "Album", "Description", "Copyright", 
    "Author", "Keywords", "AvgBitrate"
]

def get_metadata(file_path: str) -> dict:
    """
    Extrahiert die Metadaten aus einer Datei mithilfe des Exif-Tools und gibt ein strukturiertes Dictionary zurück.

    Argumente:
    - file_path: Der Pfad zur Datei, aus der die Metadaten extrahiert werden sollen.

    Rückgabewert:
    - Ein Dictionary, das die relevanten Metadaten enthält.
    
    Raises:
    - FileNotFoundError: Wenn die Datei nicht gefunden wird.
    - ValueError: Wenn keine Metadaten extrahiert werden können.
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
        filtered_metadata = {key: metadata.get(key, "N/A") for key in METADATA_KEYS}
        
        return filtered_metadata
    except subprocess.CalledProcessError as e:
        error_message = (
            f"Fehler beim Extrahieren der Metadaten für '{file_path}'.\n"
            f"Exit Code: {e.returncode}\n"
            f"Fehlerausgabe: {e.stderr.strip() if e.stderr else 'Keine Fehlermeldung verfügbar.'}\n"
            f"Vollständiger Befehl: {command}"
        )
        raise ValueError(error_message)