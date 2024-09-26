# src/apple_compressor_manager/commands/compress_prores_file_command.py

import typer
import os
import asyncio
from apple_compressor_manager.single_compression.single_file_compressor import compress
from apple_compressor_manager.profiles.profile_manager import get_profile_path, validate_profile

def compress_file_command(
    input_file: str = typer.Argument(..., help="Pfad zur ProRes-Datei"),
    compressor_profile: str = typer.Argument(..., help="Name des Compressor-Profils"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll"),
    check_interval: int = typer.Option(30, "--check-interval", help="Intervall in Sekunden für die Überprüfung des Komprimierungsstatus")
):
    """
    Komprimiert eine einzelne ProRes-Datei unter Verwendung eines Compressor-Profils.
    
    ## Argumente:
    - **input_file** (*str*): Pfad zur ProRes-Datei.
    - **compressor_profile** (*str*): Name des Compressor-Profils.
    - **output** (*str, optional*): Verzeichnis für die Ausgabedatei. Standardmäßig das Verzeichnis der Eingabedatei.
    - **check_interval** (*int*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus.
    
    ## Beispielaufruf:
    ```bash
    apple-compressor-manager compress-prores-file /Pfad/zur/Datei.mov "MeinCompressorProfil" --output /Pfad/zum/Output-Verzeichnis --check-interval 60
    ```
    """
    # Validierung des Compressor-Profils
    if not validate_profile(compressor_profile):
        typer.secho(f"Compressor-Profil '{compressor_profile}' ist ungültig oder existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    compressor_profile_path = get_profile_path(compressor_profile)
    
    # Überprüfe und initialisiere die `output`-Variable
    if output is None:
        output = os.path.dirname(input_file)
    
    async def async_main():
        # Definiere einen Callback für den Abschluss der Komprimierung
        def on_completion(output_file):
            typer.echo(f"Komprimierung abgeschlossen für Datei: {output_file}")
            # Keine Löschung der Originaldatei mehr

        # Führe die Komprimierung durch
        await compress(
            input_file=input_file,
            output_file=os.path.join(output, os.path.basename(input_file)),
            compressor_profile_path=compressor_profile_path,
            callback=on_completion,
            add_tag_flag=True,  # Jetzt wird das Tag vor dem Start hinzugefügt
            check_interval=check_interval
        )
    
    # Führe die asynchrone Hauptfunktion aus
    asyncio.run(async_main())