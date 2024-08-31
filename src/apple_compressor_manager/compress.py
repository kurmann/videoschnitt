import os
import asyncio
import subprocess
from pathlib import Path
from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compressor_utils import are_sb_files_present
from apple_compressor_manager.cleanup_prores import delete_prores_if_hevc_a_exists

# Modulkonstanten
MIN_PRORES_SIZE_MB = 25  # ProRes-Dateien unter 25 MB werden nicht komprimiert
MIN_OUTPUT_SIZE_KB = 100  # Output-Dateien unter 100 KB werden als nicht abgeschlossen betrachtet

CHECK_INTERVAL = 60
MAX_CONCURRENT_JOBS = 3
MAX_CHECKS = 10

async def compress_prores_file(input_file, output_file, compressor_profile_path, semaphore, callback=None, delete_prores=False, prores_dir=None):
    """Startet die Komprimierung einer einzelnen ProRes-Datei und überwacht den Prozess."""
    async with semaphore:
        # Überspringe Dateien, die kleiner als die definierte Mindestgröße sind
        if os.path.getsize(input_file) < MIN_PRORES_SIZE_MB * 1024 * 1024:
            print(f"Überspringe Datei (zu klein für Komprimierung): {input_file}")
            return

        # Prüfe, ob die Datei wirklich ProRes ist
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

        # Überspringe Dateien, die kleiner als die definierte Mindestgröße sind
        if os.path.getsize(output_file) < MIN_OUTPUT_SIZE_KB * 1024:
            print(f"Ausgabedatei {output_file} ist zu klein und wird als nicht abgeschlossen betrachtet.")
            continue

        codec = get_video_codec(output_file)
        if os.path.getsize(output_file) > MIN_OUTPUT_SIZE_KB * 1024:
            print(f"Komprimierung abgeschlossen: {output_file}")
            if callback:
                callback(output_file)
            if delete_prores:
                if prores_dir is None:
                    prores_dir = os.path.dirname(output_file)
                delete_prores_if_hevc_a_exists(Path(output_file), Path(prores_dir))
            break
        else:
            print(f"Fehlerhafter Codec oder Datei zu klein für: {output_file}. Codec: '{codec}', Grösse: {os.path.getsize(output_file)} KB")

async def compress_prores_files(file_list, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None):
    """Komprimiert alle ProRes-Dateien in der übergebenen Liste unter Berücksichtigung der maximalen Anzahl gleichzeitiger Jobs."""
    semaphore = asyncio.Semaphore(MAX_CONCURRENT_JOBS)
    tasks = []

    for input_file in file_list:
        if not input_file.lower().endswith(".mov"):
            print(f"Überspringe Datei (nicht MOV-Format): {input_file}")
            continue

        if get_video_codec(input_file) != "prores":
            print(f"Überspringe Datei (nicht ProRes): {input_file}")
            continue

        if output_directory is None:
            output_directory = os.path.dirname(input_file)

        output_file = os.path.join(output_directory, f"{os.path.splitext(os.path.basename(input_file))[0]}-compressed.mov")

        if os.path.exists(output_file):
            existing_codec = get_video_codec(output_file)
            if existing_codec == "hevc":
                print(f"Überspringe Datei, komprimierte Version existiert bereits: {output_file}")
                continue

        tasks.append(compress_prores_file(input_file, output_file, compressor_profile_path, semaphore, callback, delete_prores, output_directory))

    await asyncio.gather(*tasks)

def run_compress_prores(file_list, output_directory=None, compressor_profile_path=None, delete_prores=False, callback=None):
    """Startet den Kompressionsprozess für die übergebene Liste von ProRes-Dateien."""
    asyncio.run(compress_prores_files(file_list, output_directory, compressor_profile_path, delete_prores, callback))