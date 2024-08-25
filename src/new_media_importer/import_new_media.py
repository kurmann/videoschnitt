from datetime import datetime
import os
import sys
import atexit
import signal
import yaml
from apple_compressor_manager.compress_prores import compress_files
from original_media_integrator.integrate_new_media import organize_media_files

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
        print("Lock-Datei entfernt.")

def signal_handler(sig, frame):
    """Signalhandler für saubere Beendigung."""
    remove_lock_file()
    sys.exit(0)

def main(argv=None):
    
    # Gib die Uhrtzeit aus und informiere, dass das Skript gestartet wurde
    print(f"{datetime.now()}: Skript zum Neumedien-Import wurde gestartet")
    
    # Lasse den ersten Parameter leer, da es sich um den Skriptnamen handelt
    if argv is None:
        argv = sys.argv[1:]

    # Lade Config und setze Default-Werte
    config = load_config()
    source_directory = config.get("source_directory", "")
    destination_directory = config.get("destination_directory", "")
    compress_prores = config.get("compress_prores", False)
    compression_directory = config.get("compression_directory", "")

    # Übergeordnete Parameter haben Vorrang vor der Config
    if len(argv) >= 3:
        source_directory = argv[0]
        destination_directory = argv[1]
        compress_prores = argv[2].lower() == "true"
        # Das vierte Argument ist optional
        compression_directory = argv[3] if len(argv) >= 4 else ""

    print(f"Verwende Quellverzeichnis: {source_directory}")
    print(f"Verwende Zielverzeichnis: {destination_directory}")
    print(f"Kompression aktiviert: {compress_prores}")
    if compress_prores:
        print(f"Verwende Kompressionsverzeichnis: {compression_directory}")

    # Standard-Startup-Prozedur
    print("Das Skript wurde erfolgreich gestartet.", file=sys.stdout)
    sys.stdout.flush()

    if os.path.exists(LOCK_FILE):
        print("Das Skript wird bereits ausgeführt. Beende Ausführung.")
        sys.exit(0)

    with open(LOCK_FILE, 'w') as lock_file:
        lock_file.write(str(os.getpid()))  # Schreibe die PID des Prozesses in die Lock-Datei

    atexit.register(remove_lock_file)
    signal.signal(signal.SIGINT, signal_handler)  # Für CTRL+C
    signal.signal(signal.SIGTERM, signal_handler)  # Für externe Beendigungssignale

    send_macos_notification("New Media Importer", "Das Skript wurde gestartet und die Verarbeitung beginnt.")

    # Starte den Kompressionsvorgang, falls aktiviert
    if compress_prores:
        print("Starte Apple Compressor Manager...")
        # Das output_directory ist das gleiche wie das source_directory ausser es ist ein Kompressionsverzeichnis angegeben
        if compression_directory:
            output_directory = compression_directory
        else:
            output_directory = source_directory
        
        compress_files(source_directory, output_directory)

    print("Starte die Integration der neuen Medien...")
    # Wenn ein Kompressionsverzeichnis angegeben wurde, dann wird die Integration zwei Mal durchgeführt (einmal für das Quellverzeichnis und einmal für das Kompressionsverzeichnis)
    organize_media_files(source_directory, destination_directory)
    print("Integration der neuen Medien abgeschlossen.")
    organize_media_files(compression_directory, destination_directory)

    send_macos_notification("New Media Importer", "Die Verarbeitung wurde abgeschlossen.")
    remove_lock_file()

if __name__ == "__main__":
    main()