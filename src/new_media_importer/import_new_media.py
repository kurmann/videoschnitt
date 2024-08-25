from datetime import datetime
import os
import sys
import atexit
import signal
import yaml
from apple_compressor_manager.compress_prores import compress_files
from original_media_integrator.integrate_new_media import organize_media_files

# Log-Datei
log_file = "/Users/patrickkurmann/Library/Logs/simple_test.log"
def log_message(message):
    """Schreibt eine Nachricht in die Log-Datei und gibt sie auf der Konsole aus."""
    with open(log_file, "a") as f:
        f.write(f"{datetime.now()} - {message}\n")
    print(message)

# Pfad zur Config-Datei
config_file = os.path.expanduser("~/Library/Application Support/Kurmann/Videoschnitt/new_media_importer/config.yaml")

def load_config():
    """Lädt die Konfigurationswerte aus der YAML-Datei."""
    if os.path.exists(config_file):
        with open(config_file, "r") as file:
            config = yaml.safe_load(file)
        print(f"Konfigurationswerte aus {config_file} übernommen.")
        return config
    else:
        print(f"Keine Konfigurationsdatei gefunden unter {config_file}.")
        return {}

# Lock-Datei im Library/Caches Verzeichnis des Benutzers
LOCK_FILE = os.path.expanduser("~/Library/Caches/new_media_importer.lock")

def send_macos_notification(title, message):
    """Sendet eine macOS-Benachrichtigung."""
    os.system(f"osascript -e 'display notification \"{message}\" with title \"{title}\"'")

def remove_lock_file():
    """Entfernt die Lock-Datei."""
    if os.path.exists(LOCK_FILE):
        os.remove(LOCK_FILE)
        log_message("Lock-Datei entfernt.")

def signal_handler(sig, frame):
    """Signalhandler für saubere Beendigung."""
    log_message(f"Signal empfangen: {sig}. Beende das Skript...")
    remove_lock_file()
    sys.exit(0)

def main():
    # Konfigurationswerte laden
    config = load_config()

    # Kommandozeilenparameter priorisieren
    source_directory = sys.argv[1] if len(sys.argv) > 1 else config.get("source_directory")
    destination_directory = sys.argv[2] if len(sys.argv) > 2 else config.get("destination_directory")
    compress_flag = sys.argv[3].lower() == "true" if len(sys.argv) > 3 else config.get("compress", False)

    if not source_directory or not destination_directory:
        print("Fehlende Parameter: Das Skript erfordert mindestens ein Quell- und ein Zielverzeichnis.")
        sys.exit(1)

    print(f"Verwende Quellverzeichnis: {source_directory}")
    print(f"Verwende Zielverzeichnis: {destination_directory}")
    print(f"Kompression aktiviert: {compress_flag}")

    # Überprüfe, ob das Skript bereits ausgeführt wird
    if os.path.exists(LOCK_FILE):
        log_message("Das Skript wird bereits ausgeführt. Beende Ausführung.")
        sys.exit(0)

    # Erstelle die Lock-Datei
    with open(LOCK_FILE, 'w') as lock_file:
        lock_file.write(str(os.getpid()))  # Schreibe die PID des Prozesses in die Lock-Datei
    log_message(f"Lock-Datei erstellt: {LOCK_FILE}")

    # Registriere die Entfernung der Lock-Datei für verschiedene Beendigungsarten
    atexit.register(remove_lock_file)
    signal.signal(signal.SIGINT, signal_handler)  # Für CTRL+C
    signal.signal(signal.SIGTERM, signal_handler)  # Für externe Beendigungssignale

    # Benachrichtigung, dass das Skript gestartet wurde
    send_macos_notification("New Media Importer", "Das Skript wurde gestartet und die Verarbeitung beginnt.")

    if len(sys.argv) != 4:
        log_message("Ungültige Anzahl von Parametern. Usage: import-new-media /path/to/source_directory /path/to/destination_directory [true/false]")
        sys.exit(1)

    source_directory = sys.argv[1]
    destination_directory = sys.argv[2]
    compress_flag = sys.argv[3].lower() == "true"

    log_message(f"Quellverzeichnis: {source_directory}")
    log_message(f"Zielverzeichnis: {destination_directory}")
    log_message(f"Kompression aktiviert: {compress_flag}")

    if compress_flag:
        log_message("Starte Apple Compressor Manager...")
        compress_files(source_directory, destination_directory)

    log_message("Starte die Integration der neuen Medien...")
    organize_media_files(source_directory, destination_directory)

    log_message("Die Verarbeitung wurde abgeschlossen.")
    send_macos_notification("New Media Importer", "Die Verarbeitung wurde abgeschlossen.")
    remove_lock_file()

if __name__ == "__main__":
    main()