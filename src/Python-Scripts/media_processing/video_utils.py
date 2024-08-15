import subprocess
from datetime import datetime
import json

def get_video_codec(filepath):
    """
    Führt ffprobe aus, um den Codec der Videodatei zu ermitteln.
    """
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=codec_name', '-of', 'json', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    
    # JSON-Antwort parsen
    probe = json.loads(result.stdout)
    
    # Überprüfen, ob der Videostream vorhanden ist und den Codec extrahieren
    if 'streams' in probe and len(probe['streams']) > 0 and 'codec_name' in probe['streams'][0]:
        return probe['streams'][0]['codec_name']
    else:
        return None

def get_creation_datetime(filepath):
    """
    Extrahiert das Aufnahmedatum zuerst aus den EXIF-Daten mit exiftool, und falls nicht vorhanden, mit ffprobe.
    """
    
    # Versuch, EXIF-Daten mit exiftool zu extrahieren
    try:
        cmd = ['exiftool', '-contentcreatetime', '-creationdate', '-blackmagic-designcameradaterecorded', '-json', filepath]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        exif_metadata = result.stdout.strip()
        
        # Parsing der JSON-Daten
        exif_json = json.loads(exif_metadata)
        if exif_json and len(exif_json) > 0:
            tags = exif_json[0]
            # Priorisierte Reihenfolge der EXIF-Tags
            if "ContentCreateDate" in tags:
                creation_time_str = tags["ContentCreateDate"]
            elif "CreationDate" in tags:
                creation_time_str = tags["CreationDate"]
            elif "BlackmagicDesignCameraDateRecorded" in tags:
                creation_time_str = tags["BlackmagicDesignCameraDateRecorded"]
            else:
                creation_time_str = None
            
            # Falls ein Datum gefunden wurde, konvertiere es in ein datetime-Objekt
            if creation_time_str:
                creation_time = datetime.strptime(creation_time_str, '%Y:%m:%d %H:%M:%S%z')
                return creation_time

    except Exception as e:
        print(f"Warnung: Fehler bei der EXIF-Analyse mit exiftool: {e}. Versuche ffprobe...")

    # Falls EXIF-Daten nicht erfolgreich waren, auf ffprobe zurückgreifen
    try:
        cmd = ['ffprobe', '-v', 'error', '-show_entries', 'format_tags=creation_time:format_tags=com.apple.quicktime.creationdate:format_tags=com.blackmagic-design.camera.dateRecorded:format_tags=content_create_date:format_tags=creation_date', '-of', 'json', filepath]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        metadata = result.stdout.strip()

        # Parsing der JSON-Daten
        meta_json = json.loads(metadata)
        # Überprüfe auf die Tags in folgender Priorität:
        if "content_create_date" in meta_json["format"]["tags"]:
            creation_time_str = meta_json["format"]["tags"]["content_create_date"]
        elif "creation_date" in meta_json["format"]["tags"]:
            creation_time_str = meta_json["format"]["tags"]["creation_date"]
        elif "com.apple.quicktime.creationdate" in meta_json["format"]["tags"]:
            creation_time_str = meta_json["format"]["tags"]["com.apple.quicktime.creationdate"]
        elif "com.blackmagic-design.camera.dateRecorded" in meta_json["format"]["tags"]:
            creation_time_str = meta_json["format"]["tags"]["com.blackmagic-design.camera.dateRecorded"]
        elif "creation_time" in meta_json["format"]["tags"]:
            creation_time_str = meta_json["format"]["tags"]["creation_time"]
        else:
            print(f"Warnung: Kein geeignetes Datum in den Metadaten gefunden für {filepath}. Verwende das aktuelle Datum.")
            return datetime.now()

        # Konvertiere den Zeitstempel in ein datetime-Objekt
        creation_time = datetime.fromisoformat(creation_time_str.replace('Z', '+00:00'))
        return creation_time

    except (KeyError, ValueError, json.JSONDecodeError) as e:
        print(f"Fehler beim Extrahieren des Aufnahmedatums mit ffprobe: {e}. Verwende das aktuelle Datum.")
        return datetime.now()

def file_in_use(filepath):
    """
    Prüft, ob eine Datei in Verwendung ist, indem versucht wird, sie umzubenennen.
    """
    try:
        # Versuche, die Datei umzubenennen
        temp_name = filepath + ".temp_check"
        os.rename(filepath, temp_name)
        os.rename(temp_name, filepath)
        return False
    except OSError:
        return True