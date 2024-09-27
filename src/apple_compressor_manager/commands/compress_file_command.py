import logging
import typer
import os
from typing import List
from apple_compressor_manager.single_compression.single_file_compressor import compress
from apple_compressor_manager.profiles.profile_manager import get_profile_path, validate_profile

logger = logging.getLogger(__name__)

def compress_file_command(
    input_file: str = typer.Argument(..., help="Pfad zur Datei, die komprimiert werden soll"),
    compressor_profiles: str = typer.Argument(..., help="Durch Kommas getrennte Liste von Compressor-Profilnamen"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll")
):
    """
    Komprimiert eine oder mehrere Dateien unter Verwendung von Compressor-Profilen.

    ## Argumente:
    - **input_file** (*str*): Pfad zur Datei, die komprimiert werden soll.
    - **compressor_profiles** (*str*): Durch Kommas getrennte Liste von Compressor-Profilnamen.
    - **output** (*str, optional*): Verzeichnis für die Ausgabedatei. Standardmäßig das Verzeichnis der Eingabedatei mit angehängtem Profilnamen.
    """
    profile_list: List[str] = [p.strip() for p in compressor_profiles.split(',') if p.strip()]

    if not profile_list:
        typer.secho("Fehler: Keine gültigen Compressor-Profile angegeben.", fg=typer.colors.RED)
        logger.error("Keine gültigen Compressor-Profile angegeben.")
        raise typer.Exit(code=1)

    # Überprüfe die Validität aller Profile vor dem Start
    invalid_profiles = [p for p in profile_list if not validate_profile(p)]
    if invalid_profiles:
        for p in invalid_profiles:
            typer.secho(f"Compressor-Profil '{p}' ist ungültig oder existiert nicht.", fg=typer.colors.RED)
            logger.error(f"Compressor-Profil '{p}' ist ungültig oder existiert nicht.")
        raise typer.Exit(code=1)

    output_files = []

    for idx, profile in enumerate(profile_list, start=1):
        typer.echo(f"Starte Komprimierung für Profil {idx}: {profile}")
        logger.info(f"Starte Komprimierung für Profil {idx}: {profile}")

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

        typer.echo(f"Starte Kompression für: {input_file} mit Profil: {compressor_profile_path}")

        compress(
            input_file=input_file,
            output_file=output_file,
            compressor_profile_path=compressor_profile_path,
            add_tag_flag=True
        )

        # Da keine Überwachung mehr erfolgt, füge die Datei sofort hinzu
        output_files.append(output_file)

    if output_files:
        typer.secho("Alle Komprimierungsaufträge wurden gestartet.", fg=typer.colors.GREEN)
        typer.echo("Erstellte Dateien:")
        logger.info("Erstellte Dateien:")
        for file in output_files:
            typer.echo(f"- {file}")
            logger.info(f"- {file}")
    else:
        typer.secho("Keine Dateien wurden komprimiert.", fg=typer.colors.YELLOW)