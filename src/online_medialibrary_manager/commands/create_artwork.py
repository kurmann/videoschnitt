import os
import typer
from emby_integrator.image_manager import convert_image_to_adobe_rgb

def create_artwork_command(
    input_image: str = typer.Argument(..., help="Pfad zur Eingabedatei (PNG oder JPG/JPEG)"),
    output_image: str = typer.Option(None, help="Optionaler Pfad zur Ausgabedatei")
):
    """
    Erzeugt ein Titelbild aus einer Eingabedatei.

    Diese Methode konvertiert ein Bild in das JPG-Format und wendet das Adobe RGB-Farbprofil an.
    Wenn kein Zielpfad angegeben wird, wird das Titelbild im gleichen Verzeichnis erstellt und 
    erhält das Suffix '-Titelbild'.

    Args:
        input_image (str): Pfad zur Eingabedatei.
        output_image (str, optional): Optionaler Pfad zur Ausgabedatei.
    """
    # Überprüfe, ob die Eingabedatei existiert
    if not os.path.isfile(input_image):
        typer.secho(f"Die Datei {input_image} existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Überprüfe, ob die Eingabedatei das richtige Format hat
    if not input_image.lower().endswith((".png", ".jpg", ".jpeg")):
        typer.secho("Die Eingabedatei muss eine PNG- oder JPG/JPEG-Datei sein.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Wenn kein Ausgabepfad angegeben wurde, im gleichen Verzeichnis speichern
    if output_image is None:
        base_name, ext = os.path.splitext(input_image)
        output_image = f"{base_name}-Titelbild.jpg"

    try:
        # Konvertiere das Bild zu JPG und wende das Adobe RGB-Farbprofil an
        convert_image_to_adobe_rgb(input_image, output_image)

        # Überprüfen, ob die Ausgabedatei erfolgreich erstellt wurde
        if os.path.isfile(output_image):
            typer.secho(f"Das Titelbild wurde erfolgreich erstellt: {output_image}", fg=typer.colors.GREEN)
        else:
            typer.secho("Fehler beim Erstellen des Titelbilds.", fg=typer.colors.RED)
            raise typer.Exit(code=1)

    except Exception as e:
        typer.secho(f"Fehler beim Verarbeiten des Bildes: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

# Registriere das Command für die CLI
def register(app: typer.Typer):
    app.command(name="create-artwork")(create_artwork_command)