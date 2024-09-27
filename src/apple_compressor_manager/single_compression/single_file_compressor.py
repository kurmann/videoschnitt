import os
import subprocess
from apple_compressor_manager.utils.file_utils import add_compression_tag

MIN_FILE_SIZE_MB = 25  # Dateien unter 25 MB werden nicht komprimiert

def compress(
    input_file: str,
    output_file: str,
    compressor_profile_path: str,
    add_tag_flag: bool = True
):
    """
    Startet die Komprimierung einer einzelnen Datei ohne Überwachung.

    ## Argumente:
    - **input_file** (*str*): Der Pfad zur Eingabedatei.
    - **output_file** (*str*): Der Pfad zur komprimierten Ausgabedatei.
    - **compressor_profile_path** (*str*): Der Pfad zur Compressor-Settings-Datei.
    - **add_tag_flag** (*bool*, optional*): Fügt das Tag 'An Apple Kompressor übergeben' hinzu, wenn gesetzt (Standard: True).
    """
    try:
        # Überprüfe die Dateigröße
        file_size_mb = os.path.getsize(input_file) / (1024 * 1024)
        if file_size_mb < MIN_FILE_SIZE_MB:
            print(f"Überspringe Datei (zu klein für Komprimierung): {input_file}")
            return

        # Füge das Tag hinzu, bevor der Compressor-Prozess gestartet wird
        if add_tag_flag:
            add_compression_tag(input_file)  # Tag hinzufügen

        # Definiere den Job-Titel
        job_title = f"Kompression '{os.path.basename(input_file)}'"

        # Erstelle den Befehl für den Compressor
        command = [
            "/Applications/Compressor.app/Contents/MacOS/Compressor",
            "-batchname", job_title,
            "-jobpath", input_file,
            "-locationpath", output_file,
            "-settingpath", compressor_profile_path
        ]

        # Starte den Komprimierungsprozess asynchron
        print(f"Starte Kompression für: {input_file} mit Profil: {compressor_profile_path}")
        subprocess.Popen(command)
        print(f"Kompressionsauftrag gestartet für: {input_file} (Job-Titel: {job_title})")

    except subprocess.CalledProcessError as e:
        print(f"Fehler beim Starten der Komprimierung für {input_file}: {e.stderr}")
    except FileNotFoundError as e:
        print(f"Datei oder Compressor nicht gefunden: {e}")
    except Exception as e:
        print(f"Unerwarteter Fehler bei der Komprimierung von {input_file}: {e}")