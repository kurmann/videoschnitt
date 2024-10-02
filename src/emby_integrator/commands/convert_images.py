# src/emby_integrator/commands/convert_images.py

import os
import typer
from emby_integrator.image_manager import convert_images_to_adobe_rgb

def convert_images_command(media_dir: str):
    """
    Konvertiere eine Liste von PNG-Bildern in Adobe RGB, falls eine passende Videodatei existiert.

    Diese Methode durchsucht das angegebene Verzeichnis nach PNG-Bildern und konvertiert sie in das 
    Adobe RGB-Farbprofil, falls eine passende Videodatei im gleichen Verzeichnis existiert.

    Args:
        media_dir (str): Der Pfad zu dem Verzeichnis, das nach PNG-Bildern und passenden Videodateien durchsucht wird.

    Returns:
        None

    Beispiel:
        $ emby-integrator convert-images-to-adobe-rgb /path/to/mediadirectory
    """
    image_files = [os.path.join(media_dir, f) for f in os.listdir(media_dir) if f.lower().endswith(".png")]
    convert_images_to_adobe_rgb(image_files, media_dir)

def register(app: typer.Typer):
    app.command(name="convert-images-to-adobe-rgb")(convert_images_command)