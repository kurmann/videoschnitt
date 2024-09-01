import os
import asyncio
import subprocess
from pathlib import Path
from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compressor_utils import are_sb_files_present
from apple_compressor_manager.cleanup_prores import delete_prores_if_hevc_a_exists

MIN_PRORES_SIZE_MB = 25  # ProRes-Dateien unter 25 MB werden nicht komprimiert
MIN_OUTPUT_SIZE_KB = 100  # Output-Dateien unter 100 KB werden als nicht abgeschlossen betrachtet
MAX_CHECKS = 10  # Maximale Anzahl von Überprüfungen, bevor die Komprimierung als nicht abgeschlossen betrachtet wird
CHECK_INTERVAL = 240  # Intervall in Sekunden zwischen den Überprüfungen

def get_output_suffix(compressor_profile_path):
    """Ermittelt das Suffix für die Ausgabedatei basierend auf dem Compressor-Setting-Namen."""
    setting_name = os.path.splitext(os.path.basename(compressor_profile_path))[0]
    return f"-{setting_name}"

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
        await monitor_compression(output_file, compressor_profile_path, callback, delete_prores, prores_dir)
    else:
        print(f"Fehler bei der Komprimierung von {input_file}: {result.stderr}")

async def monitor_compression(output_file, compressor_profile_path, callback=None, delete_prores=False, prores_dir=None):
    """
    Überwacht die Komprimierung und überprüft periodisch den Fortschritt.

    Argumente:
    - output_file: Der Pfad zur komprimierten Ausgabedatei.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
    - delete_prores: Boolean, der angibt, ob die ursprüngliche ProRes-Datei nach erfolgreicher Komprimierung gelöscht werden soll.
    - prores_dir: Das Verzeichnis, in dem die ursprüngliche ProRes-Datei gespeichert ist. Wird verwendet, wenn delete_prores=True gesetzt ist.

    Hinweis:
    - Die Methode überprüft, ob die Komprimierung noch läuft, abgeschlossen ist oder ob Fehler aufgetreten sind.
    - Wenn delete_prores=True gesetzt ist und die Komprimierung erfolgreich war, wird die Original-ProRes-Datei gelöscht.
    """
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
                prores_file = Path(prores_dir) / Path(output_file).name.replace(get_output_suffix(compressor_profile_path), "")
                delete_prores_if_hevc_a_exists(prores_file, Path(prores_dir))
            break
        else:
            print(f"Fehlerhafter Codec oder Datei zu klein für: {output_file}. Codec: '{codec}', Grösse: {os.path.getsize(output_file)} KB")

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