# src/metadata_manager/commands/get_title.py

import json
import subprocess
from typing import Optional
import typer
import logging

logger = logging.getLogger(__name__)

def get_title_command(
    filepath: str = typer.Argument(..., help="Pfad zur Mediendatei, aus der der Titel ausgelesen werden soll")
):
    """
    Liest den "Title"-Tag aus den Metadaten einer Mediendatei und gibt ihn aus.
    """
    title = get_title(filepath)
    if title:
        typer.secho(f"Titel: {title}", fg=typer.colors.GREEN)
    else:
        typer.secho(f"Kein Titel-Tag in der Datei {filepath} gefunden.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

def get_title(filepath: str) -> Optional[str]:
    """
    Liest den "Title"-Tag aus den Metadaten einer Mediendatei aus.

    Args:
        filepath (str): Der Pfad zur Mediendatei.

    Returns:
        str | None: Der Titel der Datei, oder None, wenn der Tag nicht gefunden wurde.
    """
    try:
        cmd = [
            'exiftool',
            '-Title',
            '-json',
            filepath
        ]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

        if result.returncode != 0:
            logger.error(f"ExifTool Fehler: {result.stderr.strip()}")
            return None

        exif_metadata = result.stdout.strip()
        logger.debug(f"Rohdaten von exiftool: {exif_metadata}")

        exif_json = json.loads(exif_metadata)

        if exif_json and len(exif_json) > 0:
            title = exif_json[0].get("Title")
            logger.debug(f"Ausgelesener Titel: {title}")
            return title

    except Exception as e:
        logger.error(f"Fehler beim Abrufen des Titel-Tags mit exiftool: {e}")

    logger.warning(f"Titel-Tag konnte nicht aus {filepath} gelesen werden.")
    return None

if __name__ == "__main__":
    typer.run(get_title_command)