# src/apple_compressor_manager/single_compression/compression_monitor.py

import os
import asyncio
import logging
from typing import Callable, Optional
import typer

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
    - **check_interval** (*int*, optional*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus (Standard: 30).

    ## Beispielaufruf:
    ```python
    await monitor_compression("/Pfad/zur/Ausgabedatei.mov", callback=meine_callback_funktion, check_interval=60)
    ```
    """
    logger.info(f"Beginne Überwachung der Komprimierung für: {output_file} mit Intervall: {check_interval} Sekunden")
    typer.secho(f"Beginne Überwachung der Komprimierung für: {output_file} mit Intervall: {check_interval} Sekunden", fg=typer.colors.CYAN)
    while True:
        await asyncio.sleep(check_interval)
        logger.info(f"Überprüfung für {output_file}...")
        typer.echo(f"Überprüfung für {output_file} in {check_interval} Sekunden...")

        if not os.path.exists(output_file):
            logger.warning(f"Komprimierung für: {output_file} hat noch nicht begonnen.")
            typer.secho(f"Komprimierung für: {output_file} hat noch nicht begonnen.", fg=typer.colors.YELLOW)
            continue

        file_size_kb = os.path.getsize(output_file) / 1024
        if file_size_kb < MIN_OUTPUT_SIZE_KB:
            logger.warning(f"Ausgabedatei {output_file} ist zu klein ({file_size_kb:.2f} KB). Warte weiter.")
            typer.secho(f"Ausgabedatei {output_file} ist zu klein ({file_size_kb:.2f} KB). Warte weiter.", fg=typer.colors.YELLOW)
            continue

        # Hier kannst du weitere Kriterien hinzufügen, falls notwendig

        logger.info(f"Komprimierung abgeschlossen: {output_file}")
        typer.secho(f"Komprimierung abgeschlossen: {output_file}", fg=typer.colors.GREEN)
        if callback:
            callback(output_file)
        break  # Beende die Überwachung, da die Komprimierung abgeschlossen ist