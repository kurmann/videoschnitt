import os
import sys
import subprocess
import atexit
import signal
import time
from media_processor import are_sb_files_present
from file_utils import is_file_in_use  # Der richtige Name der Methode
from video_utils import get_video_codec

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/compressor_prores_to_hevca.lock")

# Pfad zu deinem Compressor-Profil (HEVC-A)
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/HEVC-A.compressorsetting"

# Zeitintervall für die Überprüfung (in Sekunden)
CHECK_INTERVAL = 60

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
    # Überprüfen, ob das Eingabe- und Ausgangsverzeichnis existieren
    if not os.path.isdir(input_directory):
        print(f"Eingabeverzeichnis {input_directory} existiert nicht.")
        return

    if not os.path.isdir(output_directory):
        os.makedirs(output_directory)
        print(f"Ausgabeverzeichnis {output_directory} wurde erstellt.")

    # Durchlaufen aller Dateien im Eingabeverzeichnis
    for root, dirs, files in os.walk(input_directory):
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

            # Generiere den Ausgabedateinamen im Ausgabeordner
            output_file = os.path.join(output_directory, f"{os.path.splitext(file)[0]}_HEVC-A.mov")

            print(f"Starte Kompression für: {input_file}")
            
            # Compressor-Befehl mit dem korrekten Pfad zur Compressor-App und Profil
            command = [
                "/Applications/Compressor.app/Contents/MacOS/Compressor",
                "-batchname", "ProRes-Archivierung nach HEVC-A",
                "-jobpath", input_file,
                "-locationpath", output_file,
                "-settingpath", COMPRESSOR_PROFILE_PATH
            ]
            
            try:
                # Führe den Compressor-Befehl aus
                subprocess.run(command, check=True)
                print(f"Kompression gestartet für: {input_file}")

                # Warte bis die Komprimierung abgeschlossen ist
                while True:
                    time.sleep(CHECK_INTERVAL)
                    if not are_sb_files_present(output_file):
                        print(f"Komprimierung abgeschlossen: {output_file}")
                        break

                # Lösche die Originaldatei nach erfolgreicher Komprimierung
                os.remove(input_file)
                print(f"Originaldatei gelöscht: {input_file}")

            except subprocess.CalledProcessError as e:
                print(f"Fehler bei der Komprimierung von {input_file}: {e}")

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