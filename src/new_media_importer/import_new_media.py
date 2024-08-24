import os
import subprocess
import sys

def check_for_drive_and_count_files(target_directory):
    try:
        # Überprüfen, ob das Verzeichnis existiert
        if os.path.exists(target_directory):
            # Anzahl der Dateien im Verzeichnis zählen
            num_files = len(os.listdir(target_directory))

            # Systemmeldung ausgeben
            message = f"Festplatte erkannt! Es wurden {num_files} Dateien im Verzeichnis '{target_directory}' gefunden."
            subprocess.run(["osascript", "-e", f'display notification "{message}" with title "Medien-Importer"'])
        else:
            subprocess.run(["osascript", "-e", f'display notification \"Verzeichnis {target_directory} nicht gefunden.\" with title \"Medien-Importer\"'])
    except Exception as e:
        # Fehlerhafte Ausführung protokollieren
        error_message = f"Fehler: {str(e)}"
        subprocess.run(["osascript", "-e", f'display notification "{error_message}" with title "Medien-Importer"'])
        sys.exit(1)  # Rückgabewert 1 bei Fehler

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python import_new_media.py <directory_path>")
        sys.exit(1)
    else:
        target_directory = sys.argv[1]
        check_for_drive_and_count_files(target_directory)