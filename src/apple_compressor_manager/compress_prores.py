import os
import asyncio
import subprocess
from pathlib import Path
from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compressor_utils import are_sb_files_present
from apple_compressor_manager.cleanup_prores import delete_prores_if_hevc_a_exists

COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/HEVC-A.compressorsetting"
CHECK_INTERVAL = 60
MAX_CONCURRENT_JOBS = 3
MAX_CHECKS = 10

async def compress_file(input_file, output_file, semaphore, callback=None, delete_prores=False, prores_dir=None):
    """Startet die Komprimierung einer einzelnen Datei und überwacht den Prozess."""
    async with semaphore:
        job_title = f"Kompression '{os.path.basename(input_file)}' zu HEVC-A"

        command = [
            "/Applications/Compressor.app/Contents/MacOS/Compressor",
            "-batchname", job_title,
            "-jobpath", input_file,
            "-locationpath", output_file,
            "-settingpath", COMPRESSOR_PROFILE_PATH
        ]

        result = subprocess.run(command, check=False)

        print(f"Kompressionsauftrag erstellt für: {input_file} (Job-Titel: {job_title})")

        if result.returncode == 0:
            await monitor_compression(output_file, callback, delete_prores, prores_dir)
        else:
            print(f"Fehler bei der Komprimierung von {input_file}: {result.stderr}")

async def monitor_compression(output_file, callback=None, delete_prores=False, prores_dir=None):
    """Überwacht die Komprimierung und überprüft periodisch den Fortschritt."""
    check_count = 0

    while check_count < MAX_CHECKS:
        await asyncio.sleep(CHECK_INTERVAL)
        check_count += 1
        print(f"Überprüfung {check_count}/{MAX_CHECKS} für {output_file}...")

        if are_sb_files_present(output_file):
            print(f"Komprimierung für: {output_file} läuft noch.")
            continue

        if not os.path.exists(output_file):
            print(f"Komprimierung für: {output_file} noch nicht abgeschlossen.")
            continue

        codec = get_video_codec(output_file)
        if codec == "hevc":
            print(f"Komprimierung abgeschlossen: {output_file}")
            if callback:
                callback(output_file)
            if delete_prores:
                if prores_dir is None:
                    prores_dir = os.path.dirname(output_file)
                delete_prores_if_hevc_a_exists(Path(output_file), Path(prores_dir))
            break
        else:
            print(f"Fehlerhafter Codec für: {output_file}. Erwartet: 'hevc', erhalten: '{codec}'")

async def compress_files(input_directory, output_directory=None, delete_prores=False, callback=None):
    """Komprimiert alle Dateien im Eingangsverzeichnis unter Berücksichtigung der maximalen Anzahl gleichzeitiger Jobs."""
    semaphore = asyncio.Semaphore(MAX_CONCURRENT_JOBS)
    tasks = []

    if output_directory is None:
        output_directory = input_directory

    for root, _, files in os.walk(input_directory):
        for file in files:
            if file.startswith("._") or not file.lower().endswith(".mov"):
                continue

            input_file = os.path.join(root, file)
            codec = get_video_codec(input_file)
            if codec != "prores":
                print(f"Überspringe Datei (nicht ProRes): {input_file}")
                continue

            relative_path = os.path.relpath(root, input_directory)
            output_subdirectory = os.path.join(output_directory, relative_path)
            os.makedirs(output_subdirectory, exist_ok=True)

            output_file = os.path.join(output_subdirectory, f"{os.path.splitext(file)[0]}-HEVC-A.mov")

            if os.path.exists(output_file):
                existing_codec = get_video_codec(output_file)
                if existing_codec == "hevc":
                    print(f"Überspringe Datei, HEVC-A existiert bereits: {output_file}")
                    continue

            tasks.append(compress_file(input_file, output_file, semaphore, callback, delete_prores, input_directory))

    await asyncio.gather(*tasks)

def run_compress(input_directory, output_directory=None, delete_prores=False, callback=None):
    """Startet den Kompressionsprozess für ProRes zu HEVC-A."""
    asyncio.run(compress_files(input_directory, output_directory, delete_prores, callback))