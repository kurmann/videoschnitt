# src/apple_compressor_manager/commands/list_profiles_command.py

import typer
from apple_compressor_manager.profiles.profile_manager import list_profiles

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
        typer.echo("Keine Compressor-Profile gefunden.")
    else:
        typer.echo("Verfügbare Compressor-Profile:")
        for profile in profiles:
            typer.echo(f"- {profile}")