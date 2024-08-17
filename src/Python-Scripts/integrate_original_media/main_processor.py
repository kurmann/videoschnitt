import os
import subprocess
import sys
import atexit
import signal
import time
from media_manager import organize_media_files
from media_processor import are_sb_files_present, process_file
from file_utils import is_file_in_use
from video_utils import get_video_codec

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/compressor_prores_to_hevca.lock")

# Konfigurationsvariablen
CHECK_INTERVAL = 60  # Zeitintervall für die Überprüfung (in Sekunden)
BATCH_SIZE = 10  # Anzahl der Dateien, die pro Batch verarbeitet werden
MAX_CHECKS = 10  # Maximale Anzahl von Überprüfungen pro Datei

def send_macos_notification(title, message):
    """Sendet eine macOS-Benachrichtigung."""
    print(f"{title}: {message}")

def remove_lock_file():
    """Entfernt die Lock-Datei."""
    if os.path.exists(LOCK_FILE):
        os.remove(LOCK_FILE)
        print("Lock-Datei entfernt.")

def signal_handler(sig, frame):
    """Signalhandler für saubere Beendigung."""
    remove_lock_file()
    sys.exit(0)

def compress_prores_files(source_dir, comp_dir, comp_output_dir, destination_dir):
    """Verarbeitet und komprimiert alle ProRes-Dateien in Batches."""
    files_to_process = []
    
    for filename in os.listdir(source_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov'):
            continue  # Versteckte Dateien oder Dateien, die nicht MOV sind, überspringen

        file_path = os.path.join(source_dir, filename)

        if is_file_in_use(file_path):
            print(f"Datei {filename} wird noch verwendet. Überspringe.")
            continue

        # Prüfe, ob die Datei eine ProRes-Datei ist
        codec = get_video_codec(file_path)
        if codec == 'prores':
            files_to_process.append(file_path)

            # Verarbeite die Dateien in Batches
            if len(files_to_process) == BATCH_SIZE:
                process_batch(files_to_process, comp_dir, comp_output_dir, destination_dir)
                files_to_process.clear()

    # Verarbeite die verbleibenden Dateien
    if files_to_process:
        process_batch(files_to_process, comp_dir, comp_output_dir, destination_dir)

def process_batch(files, comp_dir, comp_output_dir, destination_dir):
    """Verarbeitet einen Batch von Dateien und prüft periodisch, welche fertig komprimiert sind."""
    for file in files:
        # Verschiebe ProRes-Dateien ins Komprimierungsverzeichnis
        process_file(file, comp_dir, destination_dir)

    check_count = 0
    while files and check_count < MAX_CHECKS:
        time.sleep(CHECK_INTERVAL)
        check_count += 1

        for file in files[:]:  # Verwende eine Kopie der Liste, um sicher zu iterieren
            output_file = os.path.join(comp_output_dir, os.path.basename(file).replace('.mov', '-HEVC-A.mov'))
            print(f"Prüfe Status für: {output_file}")

            # Prüfen, ob die Datei vorhanden ist und keine .sb-Dateien mehr vorhanden sind
            if os.path.exists(output_file) and not are_sb_files_present(output_file):
                # Prüfen, ob die komprimierte Datei den Codec "hevc" hat
                codec = get_video_codec(output_file)
                if codec == "hevc":
                    print(f"Komprimierung abgeschlossen: {output_file}")
                    try:
                        os.remove(file)
                        print(f"Originaldatei gelöscht: {file}")
                        files.remove(file)  # Entferne die Datei aus der Liste
                    except Exception as e:
                        print(f"Fehler beim Löschen der Datei: {e}")
                else:
                    print(f"Fehlerhafter Codec für: {output_file}. Erwartet: 'hevc', erhalten: '{codec}'")
            else:
                print(f"Komprimierung für: {output_file} läuft noch oder wurde noch nicht gestartet.")

    if files:
        print(f"Maximale Überprüfungsanzahl erreicht. {len(files)} Dateien wurden nicht erfolgreich verarbeitet.")

def main():
    # Überprüfe, ob das Skript bereits ausgeführt wird
    if os.path.exists(LOCK_FILE):
        print("Das Skript wird bereits ausgeführt. Beende Ausführung.")
        sys.exit(0)

    # Erstelle die Lock-Datei
    with open(LOCK_FILE, 'w') as lock_file:
        lock_file.write(str(os.getpid()))  # Schreibe die PID des Prozesses in die Lock-Datei

    # Registriere die Entfernung der Lock-Datei für verschiedene Beendigungsarten
    atexit.register(remove_lock_file)
    signal.signal(signal.SIGINT, signal_handler)  # Für CTRL+C
    signal.signal(signal.SIGTERM, signal_handler)  # Für externe Beendigungssignale

    # Benachrichtigung, dass das Skript gestartet wurde
    send_macos_notification("Media Processor", "Das Skript wurde gestartet und die Verarbeitung beginnt.")

    if len(sys.argv) == 3:
        # Modus mit zwei Parametern: Quelle und Ziel
        source_dir = sys.argv[1]
        destination_dir = sys.argv[2]

        if not os.path.isdir(source_dir):
            print(f"Fehler: {source_dir} ist kein gültiges Verzeichnis.")
            sys.exit(1)

        if not os.path.isdir(destination_dir):
            print(f"Fehler: {destination_dir} ist kein gültiges Verzeichnis.")
            sys.exit(1)

        # 1. Organisiere alle Medien in die Zielverzeichnisstruktur
        print("Starte die Organisation und das Verschieben der Dateien...")
        organize_media_files(source_dir, destination_dir)

    elif len(sys.argv) == 5:
        # Modus mit allen Parametern: Quelle, Komprimierungsverzeichnis, Komprimierungsausgabeverzeichnis und Ziel
        source_dir = sys.argv[1]
        comp_dir = sys.argv[2]
        comp_output_dir = sys.argv[3]
        destination_dir = sys.argv[4]

        if not os.path.isdir(source_dir):
            print(f"Fehler: {source_dir} ist kein gültiges Verzeichnis.")
            sys.exit(1)

        if not os.path.isdir(comp_dir):
            print(f"Fehler: {comp_dir} ist kein gültiges Verzeichnis.")
            sys.exit(1)

        if not os.path.isdir(comp_output_dir):
            print(f"Fehler: {comp_output_dir} ist kein gültiges Verzeichnis.")
            sys.exit(1)

        if not os.path.isdir(destination_dir):
            print(f"Fehler: {destination_dir} ist kein gültiges Verzeichnis.")
            sys.exit(1)

        # Schritt 1: Zunächst die Dateien organisieren und verschieben
        print("Organisiere und verschiebe alle unterstützten Medien...")
        organize_media_files(source_dir, destination_dir)

        # Schritt 2: ProRes-Kompression und Verarbeitung
        print("Starte die Verarbeitung der ProRes-Dateien und Kompression...")
        compress_prores_files(source_dir, comp_dir, comp_output_dir, destination_dir)

    else:
        print("Usage: python3 main_processor.py /path/to/source_directory /path/to/destination_directory")
        print("       or")
        print("Usage: python3 main_processor.py /path/to/source_directory /path/to/compression_directory /path/to/compression_output_directory /path/to/destination_directory")
        sys.exit(1)

if __name__ == "__main__":
    main()