import os
import subprocess
import shutil
import sys
import json

def get_video_codec(filepath):
    # Führt ffprobe aus, um den Codec der Videodatei zu ermitteln
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=codec_name', '-of', 'json', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    
    # JSON-Antwort parsen
    probe = json.loads(result.stdout)
    
    # Überprüfen, ob der Videostream vorhanden ist und den Codec extrahieren
    if 'streams' in probe and len(probe['streams']) > 0 and 'codec_name' in probe['streams'][0]:
        return probe['streams'][0]['codec_name']
    else:
        return None

def organize_files_by_codec(directory):
    # Verzeichnispfade definieren
    prores_dir = os.path.join(directory, "ProRes")
    hevc_dir = os.path.join(directory, "HEVC")
    hevc_a_dir = os.path.join(directory, "HEVC-A")
    other_dir = os.path.join(directory, "Other")

    # Flags, um zu prüfen, ob Verzeichnisse erstellt werden sollen
    prores_created = False
    hevc_created = False
    hevc_a_created = False
    other_created = False

    for filename in os.listdir(directory):
        if filename.startswith('.'):
            # Versteckte Dateien überspringen
            continue

        # Überprüfen, ob die Datei eine MOV-Datei ist, unabhängig von der Großschreibung
        if filename.lower().endswith('.mov'):
            filepath = os.path.join(directory, filename)
            codec = get_video_codec(filepath)

            if codec == 'prores':
                if not prores_created:
                    os.makedirs(prores_dir, exist_ok=True)
                    prores_created = True
                dest_dir = prores_dir
            elif codec == 'hevc':
                if 'HEVC-A' in filename.upper():
                    if not hevc_a_created:
                        os.makedirs(hevc_a_dir, exist_ok=True)
                        hevc_a_created = True
                    dest_dir = hevc_a_dir
                else:
                    if not hevc_created:
                        os.makedirs(hevc_dir, exist_ok=True)
                        hevc_created = True
                    dest_dir = hevc_dir
            else:
                if not other_created:
                    os.makedirs(other_dir, exist_ok=True)
                    other_created = True
                dest_dir = other_dir

            try:
                shutil.move(filepath, os.path.join(dest_dir, filename))
                print(f"Moved {filename} to {dest_dir}")
            except FileNotFoundError:
                print(f"File not found: {filepath}")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python3 organize_videos.py /path/to/directory")
        sys.exit(1)

    directory = sys.argv[1]

    if not os.path.isdir(directory):
        print(f"Error: {directory} is not a valid directory")
        sys.exit(1)

    organize_files_by_codec(directory)