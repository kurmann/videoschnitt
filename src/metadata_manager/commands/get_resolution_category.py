import subprocess
import json
from typing import Optional
import typer
import logging

logger = logging.getLogger(__name__)

def get_resolution_command(
    file_path: str = typer.Argument(..., help="Pfad zur Videodatei, um die Auflösungskategorie zu bestimmen")
):
    """
    Gibt die Auflösungskategorie einer Videodatei zurück (SD, 720p, 1080p, 2K, 4K)
    sowie die exakte Auflösung in Pixeln (Höhe × Breite).
    """
    resolution, resolution_category = get_resolution_category(file_path)
    if resolution and resolution_category:
        typer.secho(f"Auflösung: {resolution[1]} x {resolution[0]} ({resolution_category})", fg=typer.colors.GREEN)
    else:
        typer.secho("Die Auflösung konnte nicht ermittelt werden.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

def get_resolution_category(file_path: str) -> Optional[tuple]:
    """
    Ermittelt die Auflösungskategorie einer Videodatei anhand ihrer Höhe und Breite.

    Args:
        file_path (str): Der Pfad zur Videodatei.

    Returns:
        tuple | None: Die exakte Auflösung (Höhe, Breite) und die Auflösungskategorie 
        (SD, 720p, 1080p, 2K, 4K), oder None, wenn keine Kategorie ermittelt werden konnte.
    """
    try:
        # ExifTool-Befehl, um die Videoauflösung zu ermitteln
        cmd = [
            'exiftool',
            '-ImageWidth',
            '-ImageHeight',
            '-json',
            file_path
        ]
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

        if result.returncode != 0:
            logger.error(f"ExifTool Fehler: {result.stderr.strip()}")
            return None

        exif_metadata = result.stdout.strip()
        logger.debug(f"Rohdaten von exiftool: {exif_metadata}")

        exif_json = json.loads(exif_metadata)

        if exif_json and len(exif_json) > 0:
            image_width = exif_json[0].get("ImageWidth")
            image_height = exif_json[0].get("ImageHeight")
            if image_width and image_height:
                logger.debug(f"Ermittelte Auflösung: {image_width} x {image_height}")
                resolution_category = classify_resolution(image_height)
                return (image_height, image_width), resolution_category

    except Exception as e:
        logger.error(f"Fehler beim Abrufen der Auflösung mit exiftool: {e}")

    return None

def classify_resolution(image_height: int) -> str:
    """
    Klassifiziert die Auflösung basierend auf der Bildhöhe.

    Args:
        image_height (int): Die Bildhöhe in Pixeln.

    Returns:
        str: Die Auflösungskategorie (SD, 720p, 1080p, 2K, 4K).
    """
    if image_height < 720:
        return "SD"
    elif 720 <= image_height < 1080:
        return "720p"
    elif 1080 <= image_height < 1440:
        return "1080p"
    elif 1440 <= image_height < 2160:
        return "2K"
    elif image_height >= 2160:
        return "4K"
    else:
        return "Unbekannt"

if __name__ == "__main__":
    typer.run(get_resolution_command)