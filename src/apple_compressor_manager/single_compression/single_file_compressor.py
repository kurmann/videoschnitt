# src/apple_compressor_manager/single_compression/single_file_compressor.py

import os
import subprocess
import logging
import typer

from apple_compressor_manager.single_compression.compression_monitor import monitor_compression
from apple_compressor_manager.utils.file_utils import add_compression_tag

MIN_FILE_SIZE_MB = 25  # Dateien unter 25 MB werden nicht komprimiert
DEFAULT_CHECK_INTERVAL = 30  # Standard-Intervall für die Überprüfung

logger = logging.getLogger(__name__)

async def compress(
    input_file: str,
    output_file: str,
    compressor_profile_path: str,
    callback=None,
    add_tag_flag: bool = True,
    check_interval: int = DEFAULT_CHECK_INTERVAL
):
    """
    Startet die Komprimierung einer einzelnen Datei und überwacht den Prozess.

    ## Argumente:
    - **input_file** (*str*): Der Pfad zur Eingabedatei.
    - **output_file** (*str*): Der Pfad zur komprimierten Ausgabedatei.
    - **compressor_profile_path** (*str*): Der Pfad zur Compressor-Settings-Datei.
    - **callback** (*Callable[[str], None], optional*): Eine Rückruffunktion, die nach erfolgreicher Komprimierung aufgerufen wird.
    - **add_tag_flag** (*bool*, optional*): Fügt das Tag 'An Apple Kompressor übergeben' hinzu, wenn gesetzt (Standard: True).
    - **check_interval** (*int*, optional*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus (Standard: 30).

    ## Beispielaufruf:
    ```python
    await compress(
        input_file="/Pfad/zur/Datei.mov",
        output_file="/Pfad/zur/Ausgabedatei.mov",
        compressor_profile_path="/Pfad/zum/Profil.compressorsetting",
        callback=meine_callback_funktion,
        add_tag_flag=True,
        check_interval=60
    )
    ```
    """
    try:
        # Überprüfe die Dateigröße
        file_size_mb = os.path.getsize(input_file) / (1024 * 1024)
        if file_size_mb < MIN_FILE_SIZE_MB:
            logger.info(f"Überspringe Datei (zu klein für Komprimierung): {input_file}")
            return

        # Füge das Tag hinzu, bevor der Compressor-Prozess gestartet wird
        if add_tag_flag:
            logger.debug(f"Versuche, Tag zu Datei hinzuzufügen: {input_file}")
            if os.path.exists(input_file):
                logger.debug(f"Datei existiert: {input_file}")
            else:
                logger.error(f"Datei existiert nicht: {input_file}")
                raise FileNotFoundError(f"Datei existiert nicht: {input_file}")
            add_compression_tag(input_file)  # Tag hinzufügen

        # Definiere den Job-Titel
        job_title = f"Kompression '{os.path.basename(input_file)}'"

        # Erstelle den Befehl für den Compressor
        command = [
            "/Applications/Compressor.app/Contents/MacOS/Compressor",
            "-batchname", job_title,
            "-jobpath", input_file,
            "-locationpath", output_file,
            "-settingpath", compressor_profile_path
        ]

        logger.info(f"Starte Kompression für: {input_file} mit Profil: {compressor_profile_path}")
        result = subprocess.run(command, check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        logger.info(f"Kompressionsauftrag erstellt für: {input_file} (Job-Titel: {job_title})")

        # Warte auf Abschluss der Komprimierung
        await monitor_compression(output_file, callback, check_interval)

    except subprocess.CalledProcessError as e:
        logger.error(f"Fehler beim Starten der Komprimierung für {input_file}: Rückgabecode {e.returncode}, Fehlerausgabe: {e.stderr}")
    except FileNotFoundError as e:
        logger.error(e)
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        logger.error(f"Unerwarteter Fehler bei der Komprimierung von {input_file}: {e}")
        typer.secho(f"Unerwarteter Fehler: {e}", fg=typer.colors.RED)