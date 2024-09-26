# src/apple_compressor_manager/commands/compress_file_command.py

import typer
import os
import asyncio
from apple_compressor_manager.single_compression.single_file_compressor import compress
from apple_compressor_manager.profiles.profile_manager import get_profile_path, validate_profile

def compress_file_command(
    input_file: str = typer.Argument(..., help="Pfad zur Datei, die komprimiert werden soll"),
    compressor_profile: str = typer.Argument(..., help="Name des Compressor-Profils"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll"),
    check_interval: int = typer.Option(30, "--check-interval", help="Intervall in Sekunden für die Überprüfung des Komprimierungsstatus")
):
    """
    Komprimiert eine einzelne Datei unter Verwendung eines Compressor-Profils.

    Wenn kein Output-Pfad angegeben ist, wird der Profilname an den Dateinamen angehängt.

    ## Argumente:
    - **input_file** (*str*): Pfad zur Datei, die komprimiert werden soll.
    - **compressor_profile** (*str*): Name des Compressor-Profils.
    - **output** (*str, optional*): Verzeichnis für die Ausgabedatei. Standardmäßig das Verzeichnis der Eingabedatei mit angehängtem Profilnamen.
    - **check_interval** (*int*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus.

    ## Beispielaufruf:
    ```bash
    apple-compressor-manager compress-file /Pfad/zur/Datei.m4v "HEVC-A" --output /Pfad/zum/Output-Verzeichnis --check-interval 60
    ```

    Wenn kein Output-Pfad angegeben ist:
    ```bash
    apple-compressor-manager compress-file /Pfad/zur/Datei.m4v "HEVC-A" --check-interval 60
    ```
    Dies wird die Ausgabedatei als `/Pfad/zur/Datei-HEVC-A.m4v` speichern.
    """
    # Validierung des Compressor-Profils
    if not validate_profile(compressor_profile):
        typer.secho(f"Compressor-Profil '{compressor_profile}' ist ungültig oder existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    compressor_profile_path = get_profile_path(compressor_profile)
    
    # Überprüfe und initialisiere die `output`-Variable
    if output is None:
        input_dir = os.path.dirname(input_file)
        input_basename = os.path.basename(input_file)
        name, ext = os.path.splitext(input_basename)
        output_filename = f"{name}-{compressor_profile}{ext}"
        output_file = os.path.join(input_dir, output_filename)
    else:
        # Falls output ein Verzeichnis ist, füge den Profilnamen hinzu
        if os.path.isdir(output):
            input_basename = os.path.basename(input_file)
            name, ext = os.path.splitext(input_basename)
            output_filename = f"{name}-{compressor_profile}{ext}"
            output_file = os.path.join(output, output_filename)
        else:
            # Falls output ein spezifischer Dateipfad ist, verwende ihn direkt
            output_file = output
    
    async def async_main():
        # Definiere einen Callback für den Abschluss der Komprimierung
        def on_completion(output_file_path):
            typer.echo(f"Komprimierung abgeschlossen für Datei: {output_file_path}")
            # Keine Löschung der Originaldatei mehr
    
        # Führe die Komprimierung durch
        await compress(
            input_file=input_file,
            output_file=output_file,
            compressor_profile_path=compressor_profile_path,
            callback=on_completion,
            add_tag_flag=True,  # Jetzt wird das Tag vor dem Start hinzugefügt
            check_interval=check_interval
        )
    
    # Führe die asynchrone Hauptfunktion aus
    asyncio.run(async_main())