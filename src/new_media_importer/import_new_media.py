from datetime import datetime
import os
import sys
import atexit
import signal
from apple_compressor_manager.compress_prores import compress_files
from original_media_integrator.integrate_new_media import organize_media_files

log_file = "/Users/patrickkurmann/Code/videoschnitt/log.txt"
with open(log_file, "a") as f:
    f.write(f"Skript gestartet um {datetime.now()}\n")

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/new_media_importer.lock")

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
    
    print("Das Skript wurde erfolgreich gestartet.", file=sys.stdout)
    sys.stdout.flush()
    
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
    send_macos_notification("New Media Importer", "Das Skript wurde gestartet und die Verarbeitung beginnt.")

    if len(sys.argv) != 4:
        print("Usage: import-new-media /path/to/source_directory /path/to/destination_directory [true/false]")
        sys.exit(1)

    source_directory = sys.argv[1]
    destination_directory = sys.argv[2]
    compress_flag = sys.argv[3].lower() == "true"

    if compress_flag:
        print("Starte Apple Compressor Manager...")
        compress_files(source_directory, destination_directory)

    print("Starte die Integration der neuen Medien...")
    organize_media_files(source_directory, destination_directory)

    send_macos_notification("New Media Importer", "Die Verarbeitung wurde abgeschlossen.")
    remove_lock_file()

if __name__ == "__main__":
    main()