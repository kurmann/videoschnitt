# src/emby_integrator/commands/convert_single_image.py

import os
import typer
from emby_integrator.image_manager import convert_image_to_adobe_rgb, delete_image
from typing import Optional

def convert_single_image_command(
    image_path: str,
    no_confirm: bool = typer.Option(
        False,
        "--no-confirm",
        "-n",
        help="Lösche das Originalbild ohne Rückfrage."
    )
):
    """
    Konvertiere ein einzelnes Bild in das Adobe RGB-Farbprofil.

    Diese Methode konvertiert das angegebene Bild in das Adobe RGB-Farbprofil und speichert es als JPG.
    Standardmäßig wird das Originalbild nach erfolgreicher Konvertierung gelöscht. Der Benutzer kann dies
    durch Bestätigung steuern oder das Löschen ohne Rückfrage erzwingen.

    Args:
        image_path (str): Pfad zur Bilddatei, die konvertiert werden soll.
        no_confirm (bool): Optional. Wenn gesetzt, wird das Originalbild ohne Rückfrage gelöscht. Standard ist `False`.

    Returns:
        None

    Beispiel:
        Konvertiere ein Bild mit Bestätigung zum Löschen des Originals:
            $ emby-integrator convert-single-image /path/to/image.png

        Konvertiere ein Bild und lösche das Original ohne Rückfrage:
            $ emby-integrator convert-single-image /path/to/image.png --no-confirm
    """
    # Überprüfe, ob die Eingabedatei existiert
    if not os.path.isfile(image_path):
        typer.secho(f"Die Datei {image_path} existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Überprüfe, ob die Eingabedatei ein unterstütztes Format ist
    if not image_path.lower().endswith((".png", ".jpg", ".jpeg")):
        typer.secho("Die Eingabedatei muss eine PNG- oder JPG/JPEG-Datei sein.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Bestimme den Pfad der Ausgabedatei (JPEG)
    output_file = os.path.splitext(image_path)[0] + ".jpg"
    
    try:
        # Konvertiere das Bild
        convert_image_to_adobe_rgb(image_path, output_file)
        
        # Überprüfe, ob die Konvertierung erfolgreich war
        if os.path.isfile(output_file):
            typer.secho(f"Bild erfolgreich konvertiert: {output_file}", fg=typer.colors.GREEN)
            
            # Entscheide, ob das Original gelöscht werden soll
            if no_confirm:
                delete_original = True
            else:
                delete_original = typer.confirm(f"Möchtest du das Originalbild {image_path} löschen?")
            
            if delete_original:
                delete_image(image_path)
                typer.secho(f"Originalbild gelöscht: {image_path}", fg=typer.colors.YELLOW)
            else:
                typer.secho("Originalbild wurde nicht gelöscht.", fg=typer.colors.BLUE)
        else:
            typer.secho(f"Konvertierung fehlgeschlagen: {image_path}", fg=typer.colors.RED)
    
    except Exception as e:
        typer.secho(f"Fehler beim Konvertieren des Bildes: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

def register(app: typer.Typer):
    app.command(name="convert-single-image")(convert_single_image_command)