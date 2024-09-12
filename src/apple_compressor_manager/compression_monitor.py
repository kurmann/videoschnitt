# apple_compressor_manager/compression_monitor.py

import os
import asyncio
from pathlib import Path
from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compressor_utils import are_sb_files_present
from apple_compressor_manager.cleanup_prores import delete_prores_if_hevc_a_exists

MAX_CHECKS = 100  # Maximale Anzahl von Überprüfungen, bevor die Komprimierung als nicht abgeschlossen betrachtet wird
CHECK_INTERVAL = 30  # Intervall in Sekunden zwischen den Überprüfungen
MIN_OUTPUT_SIZE_KB = 100  # Output-Dateien unter 100 KB werden als nicht abgeschlossen betrachtet

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
            
def get_output_suffix(compressor_profile_path):
    """Ermittelt das Suffix für die Ausgabedatei basierend auf dem Compressor-Setting-Namen."""
    setting_name = os.path.splitext(os.path.basename(compressor_profile_path))[0]
    return f"-{setting_name}"