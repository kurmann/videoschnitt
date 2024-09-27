# src/apple_compressor_manager/single_compression/single_file_compressor.py

import os
import asyncio
import logging
from typing import Callable, List, Optional
import logging

import typer

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

from apple_compressor_manager.single_compression.compression_monitor import monitor_compression
from apple_compressor_manager.utils.file_utils import add_compression_tag
from apple_compressor_manager.profiles.profile_manager import get_profile_path

MIN_FILE_SIZE_MB = 25  # Dateien unter 25 MB werden nicht komprimiert
DEFAULT_CHECK_INTERVAL = 30  # Standard-Intervall für die Überprüfung

logger = logging.getLogger(__name__)

async def compress_multiple_profiles(
    input_file: str,
    profiles: List[str],
    output: Optional[str],
    check_interval: int = DEFAULT_CHECK_INTERVAL
):
    """
    Komprimiert eine Datei mit mehreren Profilen gleichzeitig.

    ## Argumente:
    - **input_file** (*str*): Der Pfad zur Eingabedatei.
    - **profiles** (*List[str]*): Liste der Compressor-Profile.
    - **output** (*str*): Optionaler Pfad zum Verzeichnis für die Ausgabe.
    - **check_interval** (*int*): Intervall für die Überprüfung in Sekunden.
    """
    tasks = []
    output_files = []

    for profile in profiles:
        compressor_profile_path = get_profile_path(profile)

        # Bestimme den Output-Pfad
        if output is None:
            input_dir = os.path.dirname(input_file)
            input_basename = os.path.basename(input_file)
            name, ext = os.path.splitext(input_basename)
            output_filename = f"{name}-{profile}{ext}"
            output_file = os.path.join(input_dir, output_filename)
        else:
            if os.path.isdir(output):
                input_basename = os.path.basename(input_file)
                name, ext = os.path.splitext(input_basename)
                output_filename = f"{name}-{profile}{ext}"
                output_file = os.path.join(output, output_filename)
            else:
                output_file = output

        # Definiere den Callback für die Komprimierung
        def on_completion(output_path: str):
            typer.echo(f"Komprimierung abgeschlossen für Profil '{profile}': {output_path}")
            logger.info(f"Komprimierung abgeschlossen für Profil '{profile}': {output_path}")
            output_files.append(output_path)

        # Starte die Komprimierung asynchron
        tasks.append(compress(
            input_file=input_file,
            output_file=output_file,
            compressor_profile_path=compressor_profile_path,
            callback=on_completion,
            add_tag_flag=True,
            check_interval=check_interval
        ))

    # Starte alle Komprimierungen gleichzeitig
    await asyncio.gather(*tasks)

    if output_files:
        typer.echo("Erstellte Dateien:")
        logger.info("Erstellte Dateien:")
        for file in output_files:
            typer.echo(f"- {file}")
            logger.info(f"- {file}")

async def compress(
    input_file: str,
    output_file: str,
    compressor_profile_path: str,
    callback: Optional[Callable[[str], None]] = None,
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
    """
    try:
        # Überprüfe die Dateigröße
        file_size_mb = os.path.getsize(input_file) / (1024 * 1024)
        if file_size_mb < MIN_FILE_SIZE_MB:
            logger.info(f"Überspringe Datei (zu klein für Komprimierung): {input_file}")
            typer.secho(f"Überspringe Datei (zu klein für Komprimierung): {input_file}", fg=typer.colors.YELLOW)
            return

        # Füge das Tag hinzu, bevor der Compressor-Prozess gestartet wird
        if add_tag_flag:
            logger.debug(f"Versuche, Tag zu Datei hinzuzufügen: {input_file}")
            if os.path.exists(input_file):
                logger.debug(f"Datei existiert: {input_file}")
            else:
                logger.error(f"Datei existiert nicht: {input_file}")
                typer.secho(f"Datei existiert nicht: {input_file}", fg=typer.colors.RED)
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
        typer.secho(f"Starte Kompression für: {input_file} mit Profil: {compressor_profile_path}", fg=typer.colors.BLUE)

        # Starte den Compressor-Prozess asynchron
        process = await asyncio.create_subprocess_exec(
            *command,
            stdout=asyncio.subprocess.PIPE,
            stderr=asyncio.subprocess.PIPE
        )

        logger.info(f"Kompressionsprozess gestartet: {' '.join(command)}")
        typer.echo(f"Kompressionsprozess gestartet für Profil '{compressor_profile_path}'. Nächste Überprüfung in {check_interval} Sekunden.")
        logger.info(f"Kompressionsprozess gestartet für Profil '{compressor_profile_path}'. Nächste Überprüfung in {check_interval} Sekunden.")

        stdout, stderr = await process.communicate()

        if process.returncode != 0:
            logger.error(f"Komprimierung fehlgeschlagen für {input_file}: {stderr.decode().strip()}")
            typer.secho(f"Komprimierung fehlgeschlagen für {input_file}: {stderr.decode().strip()}", fg=typer.colors.RED)
            return

        logger.info(f"Kompression abgeschlossen für {input_file} mit Profil {compressor_profile_path}")
        typer.secho(f"Kompression abgeschlossen für Profil '{compressor_profile_path}': {output_file}", fg=typer.colors.GREEN)

        # Warte auf Abschluss der Komprimierung
        await monitor_compression(output_file, callback, check_interval)

    except FileNotFoundError as e:
        logger.error(e)
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        logger.error(f"Unerwarteter Fehler bei der Komprimierung von {input_file}: {e}")
        typer.secho(f"Unerwarteter Fehler: {e}", fg=typer.colors.RED)