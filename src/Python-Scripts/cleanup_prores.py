import os
import sys
import subprocess
import json
import shutil
import re

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

def find_matching_prores_files(hevc_a_dir, prores_dir=None):
    """
    Findet und löscht alle ProRes-Dateien im prores_dir (oder im gesamten Verzeichnisbaum, wenn prores_dir None ist),
    die ein entsprechendes HEVC-A-Pendant im hevc_a_dir haben.
    """
    print("Starting to find and delete matching ProRes files...")

    hevc_a_files = {}

    # Wenn kein zweites Verzeichnis angegeben ist, durchsuche das gesamte Verzeichnis
    if prores_dir is None:
        prores_dir = hevc_a_dir

    # Suche nach allen HEVC-A Dateien im hevc_a_dir
    for root, _, files in os.walk(hevc_a_dir):
        for filename in files:
            if filename.lower().endswith(('.mov', '.MOV')) and 'HEVC-A' in filename.upper() and not filename.startswith('.'):
                filepath = os.path.join(root, filename)
                codec = get_video_codec(filepath)
                if codec == 'hevc':
                    # Entferne "-HEVC-A" aus dem Dateinamen, um den Basisnamen zu erhalten
                    base_name = filename.replace('-HEVC-A', '').replace('.mov', '').replace('.MOV', '')
                    hevc_a_files[base_name] = os.path.join(root, filename)

    # Suche nach entsprechenden ProRes-Dateien im prores_dir und lösche sie, falls ein HEVC-A-Pendant existiert
    for root, _, files in os.walk(prores_dir):
        for filename in files:
            if filename.lower().endswith(('.mov', '.MOV')) and not filename.startswith('.'):
                filepath = os.path.join(root, filename)
                codec = get_video_codec(filepath)
                base_name = filename.replace('.mov', '').replace('.MOV', '')
                if codec == 'prores':
                    # Prüfe, ob ein HEVC-A-Pendant existiert
                    if base_name in hevc_a_files:
                        try:
                            os.remove(filepath)
                            print(f"Deleted ProRes file: {filename}")
                        except FileNotFoundError:
                            print(f"File not found: {filepath}")
                        except Exception as e:
                            print(f"Error deleting file {filepath}: {e}")

def delete_transcoder_directories(directory):
    """
    Findet und löscht alle Verzeichnisse, die mit '(TranscoderService-Dokument sichern)' beginnen,
    inklusive aufnummerierter Verzeichnisse wie '(TranscoderService-Dokument sichern 2)'.
    """
    print("Starting to find and delete TranscoderService-Dokument sichern directories...")

    pattern = re.compile(r'^\(TranscoderService-Dokument sichern.*\)$')

    for root, dirs, _ in os.walk(directory):
        for dirname in dirs:
            if pattern.match(dirname):
                dirpath = os.path.join(root, dirname)
                try:
                    shutil.rmtree(dirpath)
                    print(f"Deleted directory: {dirpath}")
                except Exception as e:
                    print(f"Error deleting directory {dirpath}: {e}")

if __name__ == "__main__":
    if len(sys.argv) not in [2, 3]:
        print("Usage: python3 cleanup_prores_recursive.py /path/to/HEVC-A-dir [/path/to/ProRes-dir]")
        print("\nThis script finds and deletes all ProRes files in the specified ProRes directory (or the entire directory tree if no second directory is specified) that have a corresponding HEVC-A file in the specified HEVC-A directory.")
        print("\nAdditionally, it will delete all directories starting with '(TranscoderService-Dokument sichern)' within the specified directory tree, including numbered directories.")
        print("\nA HEVC-A file is defined as a .mov file with the HEVC codec and 'HEVC-A' in its filename.")
        print("\nA ProRes file is defined as a .mov file with the ProRes codec.")
        print("\nA ProRes file is considered to have a corresponding HEVC-A file if the filenames match, except for the '-HEVC-A' suffix in the HEVC-A file.")
        print("\nIf only one directory is specified, the script searches the entire directory tree for matching files.")
        sys.exit(1)

    hevc_a_dir = sys.argv[1]
    prores_dir = sys.argv[2] if len(sys.argv) == 3 else None

    if not os.path.isdir(hevc_a_dir):
        print(f"Error: {hevc_a_dir} is not a valid directory")
        sys.exit(1)

    if prores_dir and not os.path.isdir(prores_dir):
        print(f"Error: {prores_dir} is not a valid directory")
        sys.exit(1)

    find_matching_prores_files(hevc_a_dir, prores_dir)
    delete_transcoder_directories(hevc_a_dir)