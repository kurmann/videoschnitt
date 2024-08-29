import os
import subprocess
import json

def run_cleanup(hevc_a_dir, prores_dir=None, verbose=False):
    """
    Findet und löscht alle ProRes-Dateien im prores_dir (oder im gesamten Verzeichnisbaum, wenn prores_dir nicht angegeben ist),
    die ein entsprechendes HEVC-A-Pendant im hevc_a_dir haben.
    """
    if verbose:
        print("Starting to find and delete matching ProRes files...")
    
    hevc_a_files = {}

    if prores_dir is None:
        prores_dir = hevc_a_dir

    for root, _, files in os.walk(hevc_a_dir):
        for filename in files:
            if filename.lower().endswith(('.mov', '.MOV')) and 'HEVC-A' in filename.upper() and not filename.startswith('.'):
                filepath = os.path.join(root, filename)
                codec = get_video_codec(filepath)
                if codec == 'hevc':
                    base_name = filename.replace('-HEVC-A', '').replace('.mov', '').replace('.MOV', '')
                    hevc_a_files[base_name] = os.path.join(root, filename)

    for root, _, files in os.walk(prores_dir):
        for filename in files:
            if filename.lower().endswith(('.mov', '.MOV')) and not filename.startswith('.'):
                filepath = os.path.join(root, filename)
                codec = get_video_codec(filepath)
                base_name = filename.replace('.mov', '').replace('.MOV', '')
                if codec == 'prores' and base_name in hevc_a_files:
                    try:
                        os.remove(filepath)
                        if verbose:
                            print(f"Deleted ProRes file: {filename}")
                    except FileNotFoundError:
                        if verbose:
                            print(f"File not found: {filepath}")
                    except Exception as e:
                        print(f"Error deleting file {filepath}: {e}")

def get_video_codec(filepath):
    """
    Führt ffprobe aus, um den Codec der Videodatei zu ermitteln.
    """
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=codec_name', '-of', 'json', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    
    probe = json.loads(result.stdout)
    
    if 'streams' in probe and len(probe['streams']) > 0 and 'codec_name' in probe['streams'][0]:
        return probe['streams'][0]['codec_name']
    else:
        return None