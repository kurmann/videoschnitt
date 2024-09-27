# src/apple_compressor_manager/commands/compress_file_command.py

import logging
import typer
import asyncio
from typing import List
from apple_compressor_manager.single_compression.single_file_compressor import compress_multiple_profiles
from apple_compressor_manager.profiles.profile_manager import validate_profile

# Konfiguriere das Logging mit Dateihandler und Streamhandler
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("apple_compressor_manager.log"),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

app = typer.Typer(help="Apple Compressor Manager")

@app.command()
def compress_file_command(
    input_file: str = typer.Argument(..., help="Pfad zur Datei, die komprimiert werden soll"),
    compressor_profiles: str = typer.Argument(..., help="Durch Kommas getrennte Liste von Compressor-Profilnamen"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll"),
    check_interval: int = typer.Option(30, "--check-interval", help="Intervall in Sekunden für die Überprüfung des Komprimierungsstatus")
):
    """
    Komprimiert eine oder mehrere Dateien unter Verwendung von Compressor-Profilen.

    ## Argumente:
    - **input_file** (*str*): Pfad zur Datei, die komprimiert werden soll.
    - **compressor_profiles** (*str*): Durch Kommas getrennte Liste von Compressor-Profilnamen.
    - **output** (*str, optional*): Verzeichnis für die Ausgabedatei. Standardmäßig das Verzeichnis der Eingabedatei mit angehängtem Profilnamen.
    - **check_interval** (*int*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus.

    ## Beispielaufruf:
    ```bash
    apple-compressor-manager compress-file /Pfad/zur/Datei.m4v "HEVC-A,HEVC-B" --output /Pfad/zum/Output-Verzeichnis --check-interval 60
    ```
    """
    # Splitte die Profilnamen nach Kommas und entferne Leerzeichen sowie Anführungszeichen
    profile_list: List[str] = [p.strip().strip('"').strip("'") for p in compressor_profiles.split(',') if p.strip()]

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

    async def async_main():
        # Führe die Komprimierung für alle Profile gleichzeitig aus
        await compress_multiple_profiles(input_file, profile_list, output, check_interval)
        typer.secho("Alle Komprimierungen abgeschlossen.", fg=typer.colors.GREEN)
        logger.info("Alle Komprimierungen abgeschlossen.")

    # Starte die asynchrone Hauptfunktion
    asyncio.run(async_main())

if __name__ == "__main__":
    app()