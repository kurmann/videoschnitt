import os
import sys
import subprocess
import atexit
import signal

from file_utils import is_file_in_use, get_directory_size, move_and_rename_to_target
from video_utils import get_video_codec
from media_processor import process_completed_hevca_and_delete_prores, process_file
from date_utils import get_creation_datetime

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/original_media_processor.lock")

def send_macos_notification(title, message):
    """Sendet eine macOS-Benachrichtigung."""
    subprocess.run([
        "osascript", "-e", f'display notification "{message}" with title "{title}"'
    ])

def remove_lock_file():
    """Entfernt die Lock-Datei."""
    if os.path.exists(LOCK_FILE):
        os.remove(LOCK_FILE)
        print("Lock-Datei entfernt.")

def signal_handler(sig, frame):
    """Signalhandler für saubere Beendigung."""
    remove_lock_file()
    sys.exit(0)

def process_media_files(source_dir, original_media_dir):
    """Verschiebt QuickTime- und Bilddateien direkt ins Zielverzeichnis."""
    for root, _, files in os.walk(source_dir):  # Das "_" ersetzt "dirs", da es nicht verwendet wird
        for filename in files:
            if filename.startswith('.') or not (filename.lower().endswith(('.mov', '.mp4', '.jpg', '.jpeg', '.png', '.heif', '.heic', '.dng'))):
                continue  # Versteckte Dateien oder nicht unterstützte Formate überspringen

            file_path = os.path.join(root, filename)

            if is_file_in_use(file_path):
                print(f"Datei {filename} wird noch verwendet. Überspringe.")
                continue

            # Ermittele das Erstellungsdatum der Datei
            creation_time = get_creation_datetime(file_path)
            date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
            codec = get_video_codec(file_path)
            new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-{codec.upper()}.mov') if codec else filename

            # Bestimme das relative Verzeichnis gegenüber dem Quellverzeichnis
            relative_dir = os.path.relpath(root, source_dir)
            destination_dir = os.path.join(original_media_dir, date_path, relative_dir)

            # Verschiebe und benenne die Datei um
            from file_utils import move_and_rename_to_target
            move_and_rename_to_target(file_path, destination_dir, new_filename)

def main():
    # Überprüfe, ob das Script bereits ausgeführt wird
    if os.path.exists(LOCK_FILE):
        print("Das Script wird bereits ausgeführt. Beende Ausführung.")
        sys.exit(0)

    # Erstelle die Lock-Datei
    with open(LOCK_FILE, 'w') as lock_file:
        lock_file.write(str(os.getpid()))  # Schreibe die PID des Prozesses in die Lock-Datei

    # Registriere die Entfernung der Lock-Datei für verschiedene Beendigungsarten
    atexit.register(remove_lock_file)
    signal.signal(signal.SIGINT, signal_handler)  # Für CTRL+C
    signal.signal(signal.SIGTERM, signal_handler)  # Für externe Beendigungssignale

    # Benachrichtigung, dass das Script gestartet wurde
    send_macos_notification("Original Media Processor", "Das Script wurde gestartet und die Verarbeitung beginnt.")

    if len(sys.argv) == 3:
        # Modus mit nur zwei Parametern: Quelle und Ziel
        source_dir = sys.argv[1]
        original_media_dir = sys.argv[2]

        if not os.path.isdir(source_dir):
            print(f"Error: {source_dir} is not a valid directory")
            sys.exit(1)

        if not os.path.isdir(original_media_dir):
            print(f"Error: {original_media_dir} is not a valid directory")
            sys.exit(1)

        # Prozessiere QuickTime- und Bilddateien direkt ins Zielverzeichnis
        process_media_files(source_dir, original_media_dir)

    elif len(sys.argv) == 6:
        # Modus mit allen Parametern: Quelle, Komprimierungsverzeichnis, Komprimierungsausgabeverzeichnis, Ziel und Größenlimit
        source_dir = sys.argv[1]
        comp_dir = sys.argv[2]
        comp_output_dir = sys.argv[3]
        original_media_dir = sys.argv[4]
        max_gb_limit = float(sys.argv[5])  # Die maximal erlaubte Größe in GB für das Komprimierungsverzeichnis

        if not os.path.isdir(source_dir):
            print(f"Error: {source_dir} is not a valid directory")
            sys.exit(1)

        if not os.path.isdir(comp_dir):
            print(f"Error: {comp_dir} is not a valid directory")
            sys.exit(1)

        if not os.path.isdir(comp_output_dir):
            print(f"Error: {comp_output_dir} is not a valid directory")
            sys.exit(1)

        if not os.path.isdir(original_media_dir):
            print(f"Error: {original_media_dir} is not a valid directory")
            sys.exit(1)

        compressor_started = False

        # Zuerst prüfen, ob fertige HEVC-A-Dateien im Komprimierungs-Ausgabeverzeichnis vorhanden sind und die passende ProRes-Datei löschen
        process_completed_hevca_and_delete_prores(comp_output_dir, comp_dir, original_media_dir)

        # Berechne die aktuelle Größe des Komprimierungsverzeichnisses
        initial_compression_size = get_directory_size(comp_dir) / (1024 * 1024 * 1024)  # Umrechnung in GB

        # Verarbeite die Dateien im Quellverzeichnis
        for filename in os.listdir(source_dir):
            if filename.startswith('.') or not filename.lower().endswith('.mov'):
                continue  # Versteckte Dateien oder Dateien, die nicht MOV sind, überspringen

            file_path = os.path.join(source_dir, filename)

            if is_file_in_use(file_path):
                print(f"Datei {filename} wird noch verwendet. Überspringe.")
                continue

            # Prüfe, ob die Datei eine ProRes-Datei ist und das Limit überschritten wird
            codec = get_video_codec(file_path)
            if codec == 'prores':
                file_size_gb = os.path.getsize(file_path) / (1024 * 1024 * 1024)  # Dateigröße in GB
                if initial_compression_size + file_size_gb > max_gb_limit:
                    print(f"Das Komprimierungsverzeichnis hat bereits das Limit von {max_gb_limit} GB erreicht.")
                    print(f"Überspringe Datei: {filename}, da das Limit für das Komprimierungsverzeichnis erreicht ist.")
                    continue  # Überspringe diese Datei, da das Limit überschritten wird

                initial_compression_size += file_size_gb

            # Prozessiere die Datei (verschiebe ins Komprimierungsverzeichnis oder direkt ins Ziel)
            process_file(file_path, comp_dir, original_media_dir, compressor_started)

    else:
        print("Usage: python3 main.py /path/to/source_directory /path/to/destination_directory")
        print("       or")
        print("Usage: python3 main.py /path/to/source_directory /path/to/compression_directory /path/to/compression_output_directory /path/to/original_media_directory max_gb_limit")
        sys.exit(1)

if __name__ == "__main__":
    main()