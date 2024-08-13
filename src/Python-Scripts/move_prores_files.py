import os
import shutil
import subprocess
import sys
from pync import Notifier

def send_notification(title, message):
    """
    Sendet eine macOS-Benachrichtigung.
    """
    Notifier.notify(message, title=title)

def get_video_codec(filepath):
    """
    Führt ffprobe aus, um den Codec der Videodatei zu ermitteln.
    """
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=codec_name', '-of', 'default=noprint_wrappers=1:nokey=1', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    return result.stdout.strip()

def move_prores_files(source_dir, target_dir):
    """
    Verschiebt ProRes-Dateien aus dem Quellverzeichnis in das Zielverzeichnis.
    """
    send_notification("Script für ProRes-Verschiebung", "Das Skript wurde gestartet.")

    if not os.path.isdir(source_dir):
        send_notification("Fehler", f"Das Quellverzeichnis {source_dir} ist ungültig.")
        sys.exit(1)

    if not os.path.isdir(target_dir):
        send_notification("Fehler", f"Das Zielverzeichnis {target_dir} ist ungültig.")
        sys.exit(1)

    files_moved = 0

    for root, _, files in os.walk(source_dir):
        for filename in files:
            if filename.lower().endswith('.mov'):
                filepath = os.path.join(root, filename)
                codec = get_video_codec(filepath)

                if codec == 'prores':
                    target_path = os.path.join(target_dir, filename)
                    shutil.move(filepath, target_path)
                    send_notification("Datei verschoben", f"Die Datei {filename} wurde nach {target_dir} verschoben.")
                    files_moved += 1

    if files_moved == 0:
        send_notification("Script für ProRes-Verschiebung", "Es wurden keine ProRes-Dateien gefunden.")
    else:
        send_notification("Script für ProRes-Verschiebung", f"{files_moved} ProRes-Datei(en) wurde(n) verschoben.")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python3 move_prores_files.py /path/to/source_directory /path/to/target_directory")
        sys.exit(1)

    source_dir = sys.argv[1]
    target_dir = sys.argv[2]

    move_prores_files(source_dir, target_dir)