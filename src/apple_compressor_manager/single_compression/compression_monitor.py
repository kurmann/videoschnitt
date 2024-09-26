# src/apple_compressor_manager/compression_monitor.py

import os
import asyncio
import logging
from typing import Callable, Optional
from metadata_manager import get_video_codec
from apple_compressor_manager.utils.compressor_utils import are_sb_files_present

CHECK_INTERVAL_DEFAULT = 30  # Standardintervall in Sekunden
MIN_OUTPUT_SIZE_KB = 100  # Output-Dateien unter 100 KB werden als nicht abgeschlossen betrachtet

logger = logging.getLogger(__name__)

async def monitor_compression(
    output_file: str,
    callback: Optional[Callable[[str], None]] = None,
    check_interval: int = CHECK_INTERVAL_DEFAULT
):
    """
    Überwacht die Komprimierung und überprüft periodisch den Fortschritt, bis die Komprimierung abgeschlossen ist.

    ## Argumente:
    - **output_file** (*str*): Der Pfad zur komprimierten Ausgabedatei.
    - **callback** (*Callable[[str], None], optional*): Eine Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
    - **check_interval** (*int*, optional*): Intervall in Sekunden zwischen den Überprüfungen des Komprimierungsstatus.

    ## Beispielaufruf:
    ```python
    await monitor_compression("/Pfad/zur/Ausgabedatei.mov", callback=meine_callback_funktion, check_interval=60)
    ```
    """
    logger.info(f"Beginne Überwachung der Komprimierung für: {output_file} mit Intervall: {check_interval} Sekunden")
    while True:
        await asyncio.sleep(check_interval)
        logger.info(f"Überprüfung für {output_file}...")

        if are_sb_files_present(output_file):
            logger.info(f"Komprimierung für: {output_file} läuft noch.")
            continue

        if not os.path.exists(output_file):
            logger.warning(f"Komprimierung für: {output_file} hat noch nicht begonnen.")
            continue

        if os.path.getsize(output_file) < MIN_OUTPUT_SIZE_KB * 1024:
            logger.warning(f"Ausgabedatei {output_file} ist zu klein ({os.path.getsize(output_file)} Bytes). Warte weiter.")
            continue

        codec = get_video_codec(output_file)
        if codec == "hevc":
            logger.info(f"Komprimierung abgeschlossen: {output_file}")
            if callback:
                callback(output_file)
            break  # Beende die Überwachung, da die Komprimierung abgeschlossen ist
        else:
            logger.warning(f"Unerwarteter Codec für {output_file}: '{codec}'. Warte weiter.")