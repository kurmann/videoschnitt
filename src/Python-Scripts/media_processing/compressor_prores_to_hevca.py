import os
import sys
import subprocess
import atexit
import signal
import time
from media_processor import are_sb_files_present
from file_utils import is_file_in_use
from video_utils import get_video_codec

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/compressor_prores_to_hevca.lock")

# Pfad zu deinem Compressor-Profil (HEVC-A)
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/HEVC-A.compressorsetting"

# Zeitintervall für die Überprüfung (in Sekunden)
CHECK_INTERVAL = 60

# Anzahl der Dateien, die pro Durchlauf gleichzeitig verarbeitet werden
BATCH_SIZE = 10

# Maximale Anzahl an Überprüfungen, um eine Endlosschleife zu verhindern
MAX_CHECKS = 10

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
            if file.startswith("._") or not file.lower().endswith(".mov") or is_file_in_use(os.path.join(root, file)):
                continue

            input_file = os.path.join(root, file)
            
            # Überprüfen, ob die Datei im ProRes-Codec vorliegt
            codec = get_video_codec(input_file)
            if codec != "prores":
                print(f"Überspringe Datei (nicht ProRes): {input_file}")
                continue

            # Generiere den Ausgabedateinamen im Ausgabeordner mit dem Postfix "-HEVC-A"
            output_file = os.path.join(output_directory, f"{os.path.splitext(file)[0]}-HEVC-A.mov")
            files_to_process.append((input_file, output_file))

            # Verarbeite Dateien in Batches
            if len(files_to_process) == BATCH_SIZE:
                process_batch(files_to_process)
                files_to_process.clear()

    # Verarbeite die verbleibenden Dateien
    if files_to_process:
        process_batch(files_to_process)

def process_batch(files):
    """Verarbeitet einen Batch von Dateien und prüft periodisch, welche fertig komprimiert sind."""
    # Starte die Kompression für den gesamten Batch
    for input_file, output_file in files:
        # Dynamischer Job-Titel
        job_title = f"Kompression '{os.path.basename(input_file)}' zu HEVC-A"
        
        command = [
            "/Applications/Compressor.app/Contents/MacOS/Compressor",
            "-batchname", job_title,
            "-jobpath", input_file,
            "-locationpath", output_file,
            "-settingpath", COMPRESSOR_PROFILE_PATH
        ]
        try:
            subprocess.run(command, check=False)
            print(f"Kompressionsauftrag erstellt für: {input_file} (Job-Titel: {job_title})")
        except subprocess.CalledProcessError as e:
            print(f"Fehler bei der Komprimierung von {input_file}: {e}")

    # Warte und prüfe periodisch den Status der Kompression
    check_count = 0
    total_wait_time = CHECK_INTERVAL * MAX_CHECKS  # Gesamtwartezeit in Sekunden
    print(f"Das Skript wird insgesamt bis zu {total_wait_time // 60} Minuten (600 Sekunden) warten, um die Kompression zu überprüfen.")

    while files and check_count < MAX_CHECKS:
        time.sleep(CHECK_INTERVAL)
        check_count += 1
        print(f"Überprüfung {check_count}/{MAX_CHECKS} nach {check_count * CHECK_INTERVAL} Sekunden...")
        
        for input_file, output_file in files[:]:  # Verwende eine Kopie der Liste, um sicher zu iterieren
            print(f"Prüfe Status für: {output_file}")

            # Prüfen, ob die Datei vorhanden ist (hat die Kompression begonnen?)
            if not os.path.exists(output_file):
                print(f"Komprimierung für: {output_file} noch nicht gestartet")
                continue
            
            # Prüfen, ob temporäre .sb-Dateien vorhanden sind (läuft die Kompression noch?)
            if are_sb_files_present(output_file):
                print(f"Komprimierung für: {output_file} läuft noch")
                continue

            # Prüfen, ob die komprimierte Datei den Codec "hevc" hat
            codec = get_video_codec(output_file)
            if codec != "hevc":
                print(f"Fehlerhafter Codec für: {output_file}. Erwartet: 'hevc', erhalten: '{codec}'")
                continue

            print(f"Komprimierung abgeschlossen: {output_file}")
            try:
                os.remove(input_file)
                print(f"Originaldatei gelöscht: {input_file}")
                files.remove((input_file, output_file))  # Entferne die Datei aus der Liste
                check_count = 0  # Timer zurücksetzen, da eine Datei erfolgreich abgeschlossen wurde
            except Exception as e:
                print(f"Fehler beim Löschen der Datei: {e}")

    if files:
        print(f"Maximale Überprüfungsanzahl erreicht. {len(files)} Dateien wurden nicht erfolgreich verarbeitet.")
        print(f"Das Skript hat insgesamt {total_wait_time // 60} Minuten gewartet.")

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
    if len(sys.argv) != 3:
        print("Usage: python3 compressor_prores_to_hevca.py /path/to/source_directory /path/to/destination_directory")
        sys.exit(1)

    input_directory = sys.argv[1]
    output_directory = sys.argv[2]

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