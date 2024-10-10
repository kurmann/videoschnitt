import os
import subprocess
import typer

def create_og_image_command(
    artwork_image: str,
    output_image: str = None,  # Standardmäßig None, damit der Pfad dynamisch generiert wird
) -> str:
    """
    Erzeugt ein OpenGraph-Bild aus dem gegebenen Vorschaubild.
    Nur JPG/JPEG-Dateien werden akzeptiert.
    Wenn kein Ausgabepfad angegeben wird, wird das gleiche Verzeichnis verwendet,
    und das Bild erhält das Suffix '-OG'.

    Args:
        artwork_image (str): Pfad zum Eingabebild.
        output_image (str, optional): Pfad zur Ausgabedatei. Wenn nicht angegeben,
                                      wird der Dateiname des Eingabebilds verwendet, 
                                      mit dem Suffix '-OG'.

    Returns:
        str: Der Pfad zum erstellten OpenGraph-Bild.
    """
    if not os.path.exists(artwork_image):
        typer.secho(f"Die Datei {artwork_image} existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Überprüfen, ob das Eingabebild ein JPG/JPEG ist
    input_ext = os.path.splitext(artwork_image)[1].lower()
    if input_ext not in [".jpg", ".jpeg"]:
        typer.secho("Nur JPG/JPEG-Dateien werden akzeptiert.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Zielauflösung
    target_width = 1536
    target_height = 804
    target_aspect_ratio = target_width / target_height

    # Wenn kein output_image angegeben wurde, erstelle Standardnamen mit dem Suffix '-OG'
    if output_image is None:
        input_dir, input_filename = os.path.split(artwork_image)
        input_name, _ = os.path.splitext(input_filename)
        output_image = os.path.join(input_dir, f"{input_name}-OG.jpg")

    # Ermitteln der Original-Bildgröße
    command_get_size = [
        'sips', '-g', 'pixelWidth', '-g', 'pixelHeight', artwork_image
    ]
    result = subprocess.run(command_get_size, capture_output=True, text=True)
    original_width = int(result.stdout.split('pixelWidth: ')[1].split()[0])
    original_height = int(result.stdout.split('pixelHeight: ')[1].split()[0])
    original_aspect_ratio = original_width / original_height

    # Berechnung des richtigen Beschneidens, um das Seitenverhältnis zu erreichen
    if original_aspect_ratio > target_aspect_ratio:
        # Bild ist zu breit, es wird in der Breite beschnitten
        new_width = int(original_height * target_aspect_ratio)
        crop_command = [
            'sips', '-c', str(original_height), str(new_width),
            artwork_image, '--out', artwork_image
        ]
    else:
        # Bild ist zu hoch, es wird in der Höhe beschnitten
        new_height = int(original_width / target_aspect_ratio)
        crop_command = [
            'sips', '-c', str(new_height), str(original_width),
            artwork_image, '--out', artwork_image
        ]

    # Zuschneiden des Bildes
    try:
        subprocess.run(crop_command, check=True)
    except subprocess.CalledProcessError as e:
        typer.secho(f"Fehler beim Zuschneiden des Bildes: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Skalieren des Bildes auf die Zielauflösung
    scale_command = [
        'sips', '-z', str(target_height), str(target_width),
        artwork_image, '--out', output_image
    ]

    try:
        subprocess.run(scale_command, check=True)
    except subprocess.CalledProcessError as e:
        typer.secho(f"Fehler beim Skalieren des Bildes: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Überprüfung, ob das Bild erstellt wurde
    if os.path.isfile(output_image):
        typer.secho(f"OpenGraph-Bild wurde erfolgreich erstellt: {output_image}", fg=typer.colors.GREEN)
        return output_image
    else:
        typer.secho(f"Das OpenGraph-Bild wurde nicht erstellt.", fg=typer.colors.RED)
        raise typer.Exit(code=1)