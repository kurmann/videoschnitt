# apple_compressor_manager/compression_monitor.py

import os
import asyncio
from pathlib import Path
from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.compressor_utils import are_sb_files_present
from apple_compressor_manager.cleanup_prores import delete_prores_if_hevc_a_exists

CHECK_INTERVAL = 30  # Intervall in Sekunden zwischen den Überprüfungen
MIN_OUTPUT_SIZE_KB = 100  # Output-Dateien unter 100 KB werden als nicht abgeschlossen betrachtet

async def monitor_compression(output_file, callback=None):
    """
    Überwacht die Komprimierung und überprüft periodisch den Fortschritt, bis die Komprimierung abgeschlossen ist.

    Argumente:
    - output_file: Der Pfad zur komprimierten Ausgabedatei.
    - callback: Eine optionale Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
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
            break  # Beende die Überwachung, da die Komprimierung abgeschlossen ist
        else:
            print(f"Unerwarteter Codec für {output_file}: '{codec}'. Warte weiter.")