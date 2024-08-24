import os
import sys
import atexit
import signal
from apple_compressor_manager.compressor_helpers import send_macos_notification, process_batch, final_cleanup
from apple_compressor_manager.video_utils import get_video_codec

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/compressor_prores_to_hevca.lock")

# Pfad zu deinem Compressor-Profil (HEVC-A)
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/HEVC-A.compressorsetting"

# Zeitintervall für die Überprüfung (in Sekunden)
CHECK_INTERVAL = 60

# Anzahl der Dateien, die pro Durchlauf gleichzeitig verarbeitet werden
BATCH_SIZE = 3

# Maximale Anzahl an Überprüfungen, um eine Endlosschleife zu verhindern
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
    # Überprüfen, ob das Eingabeverzeichnis existiert
    if not os.path.isdir(input_directory):
        print(f"Fehler: Das Eingabeverzeichnis {input_directory} existiert nicht. Bitte prüfen und erneut versuchen.")
        sys.exit(1)

    # Überprüfen, ob das Ausgabeverzeichnis existiert
    if not os.path.isdir(output_directory):
        print(f"Fehler: Das Ausgabeverzeichnis {output_directory} existiert nicht. Bitte prüfen und erneut versuchen.")
        sys.exit(1)

    files_to_process = []

    # Durchlaufen aller Dateien im Eingabeverzeichnis
    for root, _, files in os.walk(input_directory):
        for file in files:
            # Überspringe versteckte Dateien und prüfe, ob die Datei in Verwendung ist
            if file.startswith("._") or not file.lower().endswith(".mov"):
                continue

            input_file = os.path.join(root, file)

            # Überprüfen, ob die Datei im ProRes-Codec vorliegt
            codec = get_video_codec(input_file)
            if codec != "prores":
                print(f"Überspringe Datei (nicht ProRes): {input_file}")
                continue

            # Berechne den relativen Pfad vom Quellverzeichnis
            relative_path = os.path.relpath(root, input_directory)

            # Generiere das entsprechende Ausgabe-Unterverzeichnis
            output_subdirectory = os.path.join(output_directory, relative_path)
            os.makedirs(output_subdirectory, exist_ok=True)

            # Generiere den Ausgabedateinamen im Ausgabeordner mit dem Postfix "-HEVC-A"
            output_file = os.path.join(output_subdirectory, f"{os.path.splitext(file)[0]}-HEVC-A.mov")

            # Prüfen, ob die HEVC-A-Datei bereits existiert und der Codec korrekt ist
            if os.path.exists(output_file):
                existing_codec = get_video_codec(output_file)
                if existing_codec == "hevc":
                    print(f"Überspringe Datei, HEVC-A existiert bereits: {output_file}")
                    continue

            files_to_process.append((input_file, output_file))

            # Verarbeite Dateien in Batches
            if len(files_to_process) == BATCH_SIZE:
                process_batch(files_to_process, COMPRESSOR_PROFILE_PATH, CHECK_INTERVAL, MAX_CHECKS)
                files_to_process.clear()

    # Verarbeite die verbleibenden Dateien
    if files_to_process:
        process_batch(files_to_process, COMPRESSOR_PROFILE_PATH, CHECK_INTERVAL, MAX_CHECKS)

    # Zusätzlicher Durchlauf zur abschließenden Überprüfung und Löschung
    final_cleanup(input_directory, output_directory)

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
    send_macos_notification("Compressor Script", "Das Skript wurde gestartet und die Verarbeitung beginnt.")

    # Überprüfen der Argumente und Ausführen der entsprechenden Logik
    if len(sys.argv) == 2:
        input_directory = sys.argv[1]
        output_directory = input_directory  # Quelle und Ziel sind identisch
    elif len(sys.argv) == 3:
        input_directory = sys.argv[1]
        output_directory = sys.argv[2]
    else:
        print("Usage: python3 compressor_prores_to_hevca.py /path/to/source_directory [optional: /path/to/destination_directory]")
        sys.exit(1)

    if not os.path.isdir(input_directory):
        print(f"Error: {input_directory} ist kein gültiges Verzeichnis.")
        sys.exit(1)

    if not os.path.isdir(output_directory):
        print(f"Error: {output_directory} ist kein gültiges Verzeichnis.")
        sys.exit(1)

    # Starte die Kompression
    compress_files(input_directory, output_directory)

if __name__ == "__main__":
    main()