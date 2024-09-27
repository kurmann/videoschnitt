# src/apple_compressor_runner/commands/run_job_command.py

import typer
import logging
import os
from typing import List
from metadata_manager.utils import get_video_codec
from apple_compressor_manager.commands.compress_file_command import compress_file_command

logger = logging.getLogger(__name__)

def run_job_command(
    input_dir: str = typer.Argument(..., help="Pfad zum Eingangsverzeichnis"),
    compressor_profiles: str = typer.Argument(..., help="Durch Kommas getrennte Liste von Compressor-Profilnamen")
):
    """
    Vergibt Komprimierungsjobs für alle ProRes-Dateien im angegebenen Verzeichnis und für jedes angegebene Profil.

    ## Argumente:
    - **input_dir** (*str*): Pfad zum Eingangsverzeichnis, das nach ProRes-Dateien durchsucht wird.
    - **compressor_profiles** (*str*): Durch Kommas getrennte Liste von Compressor-Profilnamen.
    """
    typer.echo(f"Durchsuche das Verzeichnis: {input_dir} nach ProRes-Dateien.")
    logger.info(f"Durchsuche das Verzeichnis: {input_dir} nach ProRes-Dateien.")

    # Liste der unterstützten ProRes-Codecs (Beispiel)
    prores_codecs = ["prores", "prores_ks", "prores_aw"]

    # Finden aller ProRes-Dateien im Eingangsverzeichnis
    prores_files = []
    for root, _, files in os.walk(input_dir):
        for file in files:
            filepath = os.path.join(root, file)
            codec = get_video_codec(filepath)
            if codec and codec.lower() in prores_codecs:
                prores_files.append(filepath)
                typer.echo(f"Gefundene ProRes-Datei: {filepath}")
                logger.info(f"Gefundene ProRes-Datei: {filepath}")

    if not prores_files:
        typer.secho("Keine ProRes-Dateien im angegebenen Verzeichnis gefunden.", fg=typer.colors.YELLOW)
        logger.warning("Keine ProRes-Dateien im angegebenen Verzeichnis gefunden.")
        raise typer.Exit()

    # Verarbeitung der Compressor-Profile
    profile_list: List[str] = [p.strip() for p in compressor_profiles.split(',') if p.strip()]

    if not profile_list:
        typer.secho("Fehler: Keine gültigen Compressor-Profile angegeben.", fg=typer.colors.RED)
        logger.error("Keine gültigen Compressor-Profile angegeben.")
        raise typer.Exit(code=1)

    # Überprüfe die Validität aller Profile vor dem Start
    from apple_compressor_manager.profiles.profile_manager import validate_profile
    invalid_profiles = [p for p in profile_list if not validate_profile(p)]
    if invalid_profiles:
        for p in invalid_profiles:
            typer.secho(f"Compressor-Profil '{p}' ist ungültig oder existiert nicht.", fg=typer.colors.RED)
            logger.error(f"Compressor-Profil '{p}' ist ungültig oder existiert nicht.")
        raise typer.Exit(code=1)

    # Vergabe der Komprimierungsjobs
    total_jobs = len(prores_files) * len(profile_list)
    typer.echo(f"Starte {total_jobs} Komprimierungsaufträge...")
    logger.info(f"Starte {total_jobs} Komprimierungsaufträge...")

    for filepath in prores_files:
        input_dir_path = os.path.dirname(filepath)
        input_basename = os.path.basename(filepath)
        name, ext = os.path.splitext(input_basename)

        for profile in profile_list:
            output_filename = f"{name}-{profile}{ext}"
            output_file = os.path.join(input_dir_path, output_filename)

            typer.echo(f"Starte Komprimierung für: {filepath} mit Profil: {profile}")
            logger.info(f"Starte Komprimierung für: {filepath} mit Profil: {profile}")

            # Aufruf der compress_file_command Funktion aus dem Manager
            compress_file_command(
                input_file=filepath,
                compressor_profiles=profile,
                output=output_file
            )

    typer.secho("Alle Komprimierungsaufträge wurden gestartet.", fg=typer.colors.GREEN)
    logger.info("Alle Komprimierungsaufträge wurden gestartet.")