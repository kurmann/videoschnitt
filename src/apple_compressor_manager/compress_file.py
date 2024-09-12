import os
import asyncio
import subprocess
from pathlib import Path
import osxmetadata

from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compression_monitor import monitor_compression 

# Modulvariable für den Tag-Namen
COMPRESSION_TAG = "An Apple Kompressor übergeben"

MIN_PRORES_SIZE_MB = 25  # ProRes-Dateien unter 25 MB werden nicht komprimiert
MIN_OUTPUT_SIZE_KB = 100  # Output-Dateien unter 100 KB werden als nicht abgeschlossen betrachtet

def get_output_suffix(compressor_profile_path):
    """Ermittelt das Suffix für die Ausgabedatei basierend auf dem Compressor-Setting-Namen."""
    setting_name = os.path.splitext(os.path.basename(compressor_profile_path))[0]
    return f"-{setting_name}"

def add_tag_to_file(file_path):
    """Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu."""
    metadata = osxmetadata.OSXMetaData(file_path)
    metadata.tags = metadata.tags + [COMPRESSION_TAG]
    print(f"Tag '{COMPRESSION_TAG}' zu {file_path} hinzugefügt.")

async def compress_prores_file(input_file, output_file, compressor_profile_path, callback=None, delete_prores=False, prores_dir=None):
    """
    Startet die Komprimierung einer einzelnen ProRes-Datei und überwacht den Prozess.

    Argumente:
    - input_file: Der Pfad zur ProRes-Eingabedatei.
    - output_file: Der Pfad zur komprimierten Ausgabedatei.
    - compressor_profile_path: Der Pfad zur Compressor-Settings-Datei.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
    - delete_prores: Boolean, der angibt, ob die ursprüngliche ProRes-Datei nach erfolgreicher Komprimierung gelöscht werden soll.
    - prores_dir: Das Verzeichnis, in dem die ursprüngliche ProRes-Datei gespeichert ist. Wird verwendet, wenn delete_prores=True gesetzt ist.

    Hinweis:
    - Die Datei wird nur komprimiert, wenn sie das ProRes-Format hat und größer ist als die definierte Mindestgröße.
    - Tritt ein Fehler bei der Komprimierung auf, wird dies in der Konsole angezeigt.
    """
    if os.path.getsize(input_file) < MIN_PRORES_SIZE_MB * 1024 * 1024:
        print(f"Überspringe Datei (zu klein für Komprimierung): {input_file}")
        return

    if get_video_codec(input_file) != "prores":
        print(f"Überspringe Datei (nicht ProRes): {input_file}")
        return

    job_title = f"Kompression '{os.path.basename(input_file)}'"

    command = [
        "/Applications/Compressor.app/Contents/MacOS/Compressor",
        "-batchname", job_title,
        "-jobpath", input_file,
        "-locationpath", output_file,
        "-settingpath", compressor_profile_path
    ]

    result = subprocess.run(command, check=False)

    print(f"Kompressionsauftrag erstellt für: {input_file} (Job-Titel: {job_title})")

    if result.returncode == 0:
        add_tag_to_file(input_file)  # Tag hinzufügen nach erfolgreicher Auftragsvergabe
        await monitor_compression(output_file, compressor_profile_path, callback, delete_prores, prores_dir)
    else:
        print(f"Fehler bei der Komprimierung von {input_file}: {result.stderr}")

def run_compress_file(input_file, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None):
    """
    Startet den Kompressionsprozess für eine einzelne ProRes-Datei.

    Argumente:
    - input_file: Der Pfad zur ProRes-Eingabedatei.
    - output_directory: Das Verzeichnis, in das die komprimierte Datei gespeichert werden soll. Wenn None, wird die Datei im Quellverzeichnis gespeichert.
    - compressor_profile_path: Der Pfad zur Compressor-Settings-Datei.
    - delete_prores: Boolean, der angibt, ob die ursprüngliche ProRes-Datei nach erfolgreicher Komprimierung gelöscht werden soll.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.

    Hinweis:
    - Diese Methode startet die Kompression für eine einzelne Datei und verwendet dabei das gewählte Compressor-Setting.
    """
    if output_directory is None:
        output_directory = os.path.dirname(input_file)

    output_suffix = get_output_suffix(compressor_profile_path)
    output_file = os.path.join(output_directory, f"{os.path.splitext(os.path.basename(input_file))[0]}{output_suffix}.mov")

    asyncio.run(compress_prores_file(input_file, output_file, compressor_profile_path, callback, delete_prores))