# apple_compressor_manager/compress_file.py

import os
import asyncio
import subprocess

from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compression_monitor import monitor_compression
from apple_compressor_manager.file_utils import add_compression_tag 

MIN_PRORES_SIZE_MB = 25  # ProRes-Dateien unter 25 MB werden nicht komprimiert

async def compress_prores_file(input_file, output_file, compressor_profile_path, callback=None, delete_prores=False, prores_dir=None, add_tag=True):
    """
    Startet die Komprimierung einer einzelnen ProRes-Datei und überwacht den Prozess.

    Argumente:
    - input_file: Der Pfad zur ProRes-Eingabedatei.
    - output_file: Der Pfad zur komprimierten Ausgabedatei.
    - compressor_profile_path: Der Pfad zur Compressor-Settings-Datei.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
    - delete_prores: Boolean, der angibt, ob die ursprüngliche ProRes-Datei nach erfolgreicher Komprimierung gelöscht werden soll.
    - prores_dir: Das Verzeichnis, in dem die ursprüngliche ProRes-Datei gespeichert ist. Wird verwendet, wenn delete_prores=True gesetzt ist.
    - add_tag: Boolean, der angibt, ob das Tag 'An Apple Kompressor übergeben' hinzugefügt werden soll (Standard: True).

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
        if add_tag:
            add_compression_tag(input_file)  # Tag hinzufügen über file_utils
        await monitor_compression(output_file, compressor_profile_path, callback, delete_prores, prores_dir)
    else:
        print(f"Fehler bei der Komprimierung von {input_file}: {result.stderr}")

def run_compress_file(input_file, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None, add_tag=True):
    """
    Startet den Kompressionsprozess für eine einzelne ProRes-Datei.

    Argumente:
    - input_file: Der Pfad zur ProRes-Eingabedatei.
    - output_directory: Das Verzeichnis, in das die komprimierte Datei gespeichert werden soll.
    - compressor_profile_path: Der Pfad zur Compressor-Settings-Datei.
    - delete_prores: Boolean, der angibt, ob die ursprüngliche ProRes-Datei nach erfolgreicher Komprimierung gelöscht werden soll.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
    - add_tag: Boolean, der angibt, ob das Tag 'An Apple Kompressor übergeben' hinzugefügt werden soll (Standard: True).
    
    Hinweis:
    - Diese Methode startet die Kompression für eine einzelne Datei und verwendet dabei das gewählte Compressor-Setting.
    """
    if compressor_profile_path is None:
        raise ValueError("Ein gültiger compressor_profile_path muss angegeben werden.")

    if output_directory is None:
        output_directory = os.path.dirname(input_file)

    output_suffix = get_output_suffix(compressor_profile_path)
    output_file = os.path.join(output_directory, f"{os.path.splitext(os.path.basename(input_file))[0]}{output_suffix}.mov")

    asyncio.run(compress_prores_file(input_file, output_file, compressor_profile_path, callback, delete_prores, add_tag=add_tag))
    
def get_output_suffix(compressor_profile_path):
    """Ermittelt das Suffix für die Ausgabedatei basierend auf dem Compressor-Setting-Namen."""
    setting_name = os.path.splitext(os.path.basename(compressor_profile_path))[0]
    return f"-{setting_name}"