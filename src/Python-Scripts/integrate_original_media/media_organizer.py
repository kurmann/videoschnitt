import os
import subprocess
import sys
import atexit
import signal
from media_manager import organize_media_files

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/media_organizer.lock")

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

        # Organisiere alle Medien in die Zielverzeichnisstruktur
        print("Starte die Organisation und das Verschieben der Dateien...")
        organize_media_files(source_dir, destination_dir)

    else:
        print("Usage: python3 main_processor.py /path/to/source_directory /path/to/destination_directory")
        sys.exit(1)

if __name__ == "__main__":
    main()