import os
import sys
import atexit
import signal
import click
from apple_compressor_manager.compressor_helpers import send_macos_notification, process_batch, final_cleanup
from apple_compressor_manager.video_utils import get_video_codec

LOCK_FILE = os.path.expanduser("~/Library/Caches/compressor_prores_to_hevca.lock")
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/HEVC-A.compressorsetting"
CHECK_INTERVAL = 60
BATCH_SIZE = 3
MAX_CHECKS = 10

def remove_lock_file():
    """Entfernt die Lock-Datei."""
    if os.path.exists(LOCK_FILE):
        os.remove(LOCK_FILE)
        print("Lock-Datei entfernt.")

def signal_handler(sig, frame):
    """Signalhandler für saubere Beendigung."""
    remove_lock_file()
    sys.exit(0)

def compress_files(input_directory, output_directory):
    """Logik für das Komprimieren der Dateien."""
    files_to_process = []

    for root, _, files in os.walk(input_directory):
        for file in files:
            if file.startswith("._") or not file.lower().endswith(".mov"):
                continue

            input_file = os.path.join(root, file)
            codec = get_video_codec(input_file)
            if codec != "prores":
                print(f"Überspringe Datei (nicht ProRes): {input_file}")
                continue

            relative_path = os.path.relpath(root, input_directory)
            output_subdirectory = os.path.join(output_directory, relative_path)
            os.makedirs(output_subdirectory, exist_ok=True)

            output_file = os.path.join(output_subdirectory, f"{os.path.splitext(file)[0]}-HEVC-A.mov")

            if os.path.exists(output_file):
                existing_codec = get_video_codec(output_file)
                if existing_codec == "hevc":
                    print(f"Überspringe Datei, HEVC-A existiert bereits: {output_file}")
                    continue

            files_to_process.append((input_file, output_file))

            if len(files_to_process) == BATCH_SIZE:
                process_batch(files_to_process, COMPRESSOR_PROFILE_PATH, CHECK_INTERVAL, MAX_CHECKS)
                files_to_process.clear()

    if files_to_process:
        process_batch(files_to_process, COMPRESSOR_PROFILE_PATH, CHECK_INTERVAL, MAX_CHECKS)

    final_cleanup(input_directory, output_directory)

def compress_files_using_lockfile(input_directory, output_directory=None):
    """Startet den Kompressionsprozess von ProRes zu HEVC-A mit Lockfile-Handling."""
    
    # Lock-File-Logik bleibt unverändert
    if os.path.exists(LOCK_FILE):
        print("Das Skript wird bereits ausgeführt. Beende Ausführung.")
        sys.exit(0)

    with open(LOCK_FILE, 'w') as lock_file:
        lock_file.write(str(os.getpid()))

    atexit.register(remove_lock_file)
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

    send_macos_notification("Compressor Script", "Das Skript wurde gestartet und die Verarbeitung beginnt.")

    if output_directory is None:
        output_directory = input_directory

    compress_files(input_directory, output_directory)