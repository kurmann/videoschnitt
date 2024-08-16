import os
import subprocess
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

def get_bitrate(filepath):
    """
    Führt ffprobe aus, um die Bitrate der Videodatei zu ermitteln.
    """
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=bit_rate', '-of', 'json', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    
    # JSON-Antwort parsen
    probe = json.loads(result.stdout)
    
    # Überprüfen, ob der Videostream vorhanden ist und die Bitrate extrahieren
    if 'streams' in probe and len(probe['streams']) > 0 and 'bit_rate' in probe['streams'][0]:
        return int(probe['streams'][0]['bit_rate'])
    else:
        return None

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