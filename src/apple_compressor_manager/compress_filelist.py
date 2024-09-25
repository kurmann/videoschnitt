# apple_compressor_manager/compress_filelist.py

import os
import asyncio
from metadata_manager import get_video_codec
from apple_compressor_manager.compress_file import compress_prores_file, get_output_suffix

MAX_CONCURRENT_JOBS = 3

async def compress_prores_files_async(file_list, base_source_dir, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None, prores_dir=None):
    """
    Asynchrone Funktion zur Komprimierung von ProRes-Dateien, wobei die Unterverzeichnisstruktur beibehalten wird.

    Argumente:
    - file_list: Eine Liste von Pfaden zu den ProRes-Eingabedateien.
    - base_source_dir: Das Wurzelverzeichnis der Quelle. Dient zur Berechnung des relativen Pfads.
    - output_directory: Das Verzeichnis, in das die komprimierten Dateien gespeichert werden sollen.
    - compressor_profile_path: Der Pfad zur Compressor-Settings-Datei.
    - delete_prores: Boolean, der angibt, ob die ursprünglichen ProRes-Dateien nach erfolgreicher Komprimierung gelöscht werden sollen.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung jeder Datei aufgerufen wird.
    - prores_dir: Das Verzeichnis, in dem die ursprünglichen ProRes-Dateien gespeichert sind.
    """
    if not compressor_profile_path:
        raise ValueError("Ein gültiger compressor_profile_path muss angegeben werden.")

    semaphore = asyncio.Semaphore(MAX_CONCURRENT_JOBS)
    tasks = []

    output_suffix = get_output_suffix(compressor_profile_path)

    for input_file in file_list:

        if get_video_codec(input_file) != "prores":
            print(f"Überspringe Datei (nicht ProRes): {input_file}")
            continue

        if output_directory is None:
            output_directory = os.path.dirname(input_file)

        # Berechne den relativen Pfad vom base_source_dir zur Eingabedatei
        relative_path = os.path.relpath(os.path.dirname(input_file), base_source_dir)

        # Erstelle das entsprechende Unterverzeichnis im output_directory
        output_subdir = os.path.join(output_directory, relative_path)
        os.makedirs(output_subdir, exist_ok=True)

        # Erstelle den Pfad für die Ausgabedatei im entsprechenden Unterverzeichnis
        output_file = os.path.join(
            output_subdir,
            f"{os.path.splitext(os.path.basename(input_file))[0]}{output_suffix}.mov"
        )

        if is_output_file_valid(output_file):
            print(f"Überspringe Datei, komprimierte Version existiert bereits und ist gültig: {output_file}")
            continue
        else:
            print(f"Komprimiere Datei: {input_file} -> {output_file}")

        tasks.append(
            compress_file_with_semaphore(
                input_file,
                output_file,
                compressor_profile_path,
                semaphore,
                callback,
                delete_prores,
                prores_dir
            )
        )

    await asyncio.gather(*tasks)

async def compress_file_with_semaphore(input_file, output_file, compressor_profile_path, semaphore, callback, delete_prores, prores_dir):
    async with semaphore:
        await compress_prores_file(input_file, output_file, compressor_profile_path, callback, delete_prores, prores_dir)

def run_compress_prores_async(file_list, base_source_dir, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None, prores_dir=None):
    """
    Wrapper-Funktion zum Starten der asynchronen Komprimierung.

    Hinweis:
    - Diese Funktion sollte innerhalb einer asynchronen Funktion mit 'await' aufgerufen werden.
    """
    return compress_prores_files_async(file_list, base_source_dir, output_directory, compressor_profile_path, delete_prores, callback, prores_dir)

def is_output_file_valid(output_file):
    """Prüft, ob die Ausgabedatei gültig ist."""
    if not os.path.exists(output_file):
        return False
    if os.path.getsize(output_file) < 100 * 1024:  # Beispielwert: 100 KB
        return False
    codec = get_video_codec(output_file)
    if codec != "hevc":
        return False
    return True

async def compress_file_with_semaphore(input_file, output_file, compressor_profile_path, semaphore, callback, delete_prores, prores_dir):
    async with semaphore:
        await compress_prores_file(input_file, output_file, compressor_profile_path, callback, delete_prores, prores_dir)
