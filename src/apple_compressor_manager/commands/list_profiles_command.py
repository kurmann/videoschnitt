# src/apple_compressor_manager/commands/list_profiles_command.py

import logging
import typer
from apple_compressor_manager.profiles.profile_manager import list_profiles

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def list_profiles_command():
    """
    Listet alle verfügbaren Compressor-Profile auf.

    ## Beispielaufruf:
    ```bash
    apple-compressor-manager list-profiles
    ```

    ## Ausgabe:
    ```plaintext
    Verfügbare Compressor-Profile:
    - HEVC-A
    - 1080p-Internet
    - 4K60-Medienserver
    - 4K30-Medienserver
    - 4K-Internet
    ```
    """
    profiles = list_profiles()
    if not profiles:
        typer.echo("Keine Profile gefunden.")
        logger.info("Keine Profile gefunden.")
        raise typer.Exit()

    typer.echo("Verfügbare Profile:")
    logger.info("Verfügbare Profile aufgelistet.")

    for profile in profiles:
        if ',' in profile:
            typer.secho(f"Warnung: Das Profil '{profile}' enthält ein Komma, was problematisch sein könnte.", fg=typer.colors.YELLOW)
            logger.warning(f"Das Profil '{profile}' enthält ein Komma.")
        typer.echo(f"- {profile}")