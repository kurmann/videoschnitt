import os
import asyncio
from .video_utils import get_video_codec
from .compress_file import compress_prores_file, get_output_suffix

MAX_CONCURRENT_JOBS = 3

async def compress_prores_files(file_list, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None, prores_dir=None):
    """
    Komprimiert alle ProRes-Dateien in der übergebenen Liste unter Berücksichtigung der maximalen Anzahl gleichzeitiger Jobs.

    Argumente:
    - file_list: Eine Liste von Pfaden zu den ProRes-Eingabedateien.
    - output_directory: Das Verzeichnis, in das die komprimierten Dateien gespeichert werden sollen. Wenn None, werden die Dateien im Quellverzeichnis gespeichert.
    - compressor_profile_path: Der Pfad zur Compressor-Settings-Datei. Muss ein gültiger Pfad sein.
    - delete_prores: Boolean, der angibt, ob die ursprünglichen ProRes-Dateien nach erfolgreicher Komprimierung gelöscht werden sollen.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung jeder Datei aufgerufen wird.
    - prores_dir: Das Verzeichnis, in dem die ursprünglichen ProRes-Dateien gesucht werden sollen (normalerweise das `source_dir`).

    Hinweis:
    - Nur ProRes-Dateien werden komprimiert. Andere Dateiformate werden übersprungen.
    - Wenn output_directory=None ist, werden die komprimierten Dateien im gleichen Verzeichnis wie die Originaldateien gespeichert.
    - Das Suffix der Ausgabedatei wird auf Basis des Compressor-Settings-Namens erstellt.
    - Wenn eine komprimierte Datei bereits existiert, wird diese Datei übersprungen.
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

        output_file = os.path.join(output_directory, f"{os.path.splitext(os.path.basename(input_file))[0]}{output_suffix}.mov")

        if os.path.exists(output_file):
            existing_codec = get_video_codec(output_file)
            if existing_codec == "hevc":
                print(f"Überspringe Datei, komprimierte Version existiert bereits: {output_file}")
                continue

        tasks.append(compress_file_with_semaphore(input_file, output_file, compressor_profile_path, semaphore, callback, delete_prores, prores_dir))

    await asyncio.gather(*tasks)

async def compress_file_with_semaphore(input_file, output_file, compressor_profile_path, semaphore, callback, delete_prores, prores_dir):
    async with semaphore:
        await compress_prores_file(input_file, output_file, compressor_profile_path, callback, delete_prores, prores_dir)

def run_compress_prores(file_list, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None, prores_dir=None):
    """
    Startet den Kompressionsprozess für eine Liste von ProRes-Dateien.

    Argumente:
    - file_list: Eine Liste von Pfaden zu den ProRes-Eingabedateien.
    - output_directory: Das Verzeichnis, in das die komprimierten Dateien gespeichert werden sollen. Wenn None, werden die Dateien im Quellverzeichnis gespeichert.
    - compressor_profile_path: Der Pfad zur Compressor-Settings-Datei. Muss ein gültiger Pfad sein.
    - delete_prores: Boolean, der angibt, ob die ursprünglichen ProRes-Dateien nach erfolgreicher Komprimierung gelöscht werden sollen.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung jeder Datei aufgerufen wird.
    - prores_dir: Das Verzeichnis, in dem die ursprünglichen ProRes-Dateien gesucht werden sollen (normalerweise das `source_dir`).

    Hinweis:
    - Diese Methode kapselt den asynchronen Kompressionsprozess, damit er synchron aufgerufen werden kann.
    """
    if not compressor_profile_path:
        raise ValueError("Ein gültiger compressor_profile_path muss angegeben werden.")

    asyncio.run(compress_prores_files(file_list, output_directory, compressor_profile_path, delete_prores, callback, prores_dir))