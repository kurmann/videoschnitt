# apple_compressor_manager/compression_monitor.py

import os
import asyncio
from pathlib import Path
from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compressor_utils import are_sb_files_present
from apple_compressor_manager.cleanup_prores import delete_prores_if_hevc_a_exists

CHECK_INTERVAL = 30  # Intervall in Sekunden zwischen den Überprüfungen
MIN_OUTPUT_SIZE_KB = 100  # Output-Dateien unter 100 KB werden als nicht abgeschlossen betrachtet

async def monitor_compression(input_file, output_file, callback=None, delete_prores=False):
    """
    Überwacht die Komprimierung und überprüft periodisch den Fortschritt, bis die Komprimierung abgeschlossen ist.
    
    Argumente:
    - input_file: Die Original-ProRes-Datei, die komprimiert wird.
    - output_file: Die komprimierte Ausgabedatei.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
    - delete_prores: Boolean, der angibt, ob die ursprüngliche ProRes-Datei nach erfolgreicher Komprimierung gelöscht werden soll.
    """
    while True:
        await asyncio.sleep(CHECK_INTERVAL)
        print(f"Überprüfung für {output_file}...")

        if are_sb_files_present(output_file):
            print(f"Komprimierung für: {output_file} läuft noch.")
            continue

        if not os.path.exists(output_file):
            print(f"Komprimierung für: {output_file} hat noch nicht begonnen.")
            continue

        if os.path.getsize(output_file) < MIN_OUTPUT_SIZE_KB * 1024:
            print(f"Ausgabedatei {output_file} ist zu klein. Warte weiter.")
            continue

        codec = get_video_codec(output_file)
        if codec == "hevc":
            print(f"Komprimierung abgeschlossen: {output_file}")
            if callback:
                callback(output_file)

            if delete_prores:
                try:
                    print(f"Lösche Original-ProRes-Datei: {input_file}")
                    os.remove(input_file)
                except Exception as e:
                    print(f"Fehler beim Löschen der ProRes-Datei: {e}")
            break
        else:
            print(f"Unerwarteter Codec für {output_file}: '{codec}'. Warte weiter.")
            
def get_output_suffix(compressor_profile_path):
    """Ermittelt das Suffix für die Ausgabedatei basierend auf dem Compressor-Setting-Namen."""
    setting_name = os.path.splitext(os.path.basename(compressor_profile_path))[0]
    return f"-{setting_name}"