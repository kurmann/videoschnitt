import subprocess
import json
from datetime import datetime

def get_video_codec(filepath):
    """Ermittelt den Codec einer Videodatei."""
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=codec_name', '-of', 'default=noprint_wrappers=1:nokey=1', filepath]
    try:
        result = subprocess.check_output(cmd, text=True)
        return result.strip()
    except subprocess.CalledProcessError:
        return None

def get_creation_datetime(filepath):
    """Extrahiert das Aufnahmedatum aus den Metadaten der Videodatei."""
    cmd = ['ffprobe', '-v', 'error', '-show_entries', 'format_tags=creation_time', '-of', 'default=noprint_wrappers=1:nokey=1', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    creation_time_str = result.stdout.strip()
    
    if creation_time_str:
        creation_time = datetime.fromisoformat(creation_time_str.replace('Z', '+00:00'))
        return creation_time
    else:
        print(f"Warnung: Konnte kein Aufnahmedatum für {filepath} extrahieren. Verwende das aktuelle Datum.")
        return datetime.now()