# src/apple_compressor_manager/app.py

import os
import asyncio
from src.config_manager.config_loader import load_app_env
import typer
from apple_compressor_manager.cleanup_prores import run_cleanup
from apple_compressor_manager.compress_filelist import compress_prores_files_async
from apple_compressor_manager.compress_file import compress_prores_file
from apple_compressor_manager.file_utils import add_compression_tag
from apple_compressor_manager.video_utils import get_video_codec
from apple_compressor_manager.profiles.profile_manager import list_profiles  # Importiere list_profiles
import logging

# Lade die .env Datei
env_path = load_app_env()

# Initialisiere das Logging
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

app = typer.Typer(help="Apple Compressor Manager")

@app.command("cleanup-prores")
def cleanup_prores_command(
    hevc_a_dir: str = typer.Argument(..., help="Pfad zum HEVC-A-Verzeichnis"),
    prores_dir: str = typer.Argument(None, help="Pfad zum ProRes-Verzeichnis", show_default=False),
    verbose: bool = typer.Option(False, "--verbose", help="Aktiviere detaillierte Ausgaben.")
):
    """Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant."""
    run_cleanup(hevc_a_dir, prores_dir, verbose)

@app.command("add-tag")
def add_tag_command(file_path: str = typer.Argument(..., help="Pfad zur Datei, die getaggt werden soll")):
    """Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu."""
    add_compression_tag(file_path)

@app.command("compress-prores-files")
def compress_prores_files_command(
    input_dir: str = typer.Argument(..., help="Pfad zum Quellverzeichnis der ProRes-Dateien"),
    compressor_profile_path: str = typer.Argument(..., help="Pfad zur Compressor-Settings-Datei"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedateien gespeichert werden sollen"),
    delete_prores: bool = typer.Option(False, "--delete-prores", help="Lösche ProRes-Dateien nach erfolgreicher Komprimierung")
):
    """Komprimiert ProRes-Dateien in einem Verzeichnis."""

    async def async_main():
        if output is None:
            output = input_dir  # Standardmäßig wird das input_dir als output_dir verwendet

        # Definiere einen Callback für den Abschluss der Komprimierung
        def on_completion(output_file):
            typer.echo(f"Komprimierung abgeschlossen für Datei: {output_file}")

        await compress_prores_files_async(
            file_list=[
                os.path.join(root, file)
                for root, _, files in os.walk(input_dir)
                for file in files
                if file.lower().endswith(".mov") and get_video_codec(os.path.join(root, file)) == "prores"
            ],
            base_source_dir=input_dir,
            output_directory=output,
            compressor_profile_path=compressor_profile_path,
            delete_prores=delete_prores,
            callback=on_completion,
            prores_dir=input_dir
        )

    # Starte die asynchrone Funktion
    asyncio.run(async_main())

@app.command("compress-prores-file")
def compress_prores_file_command(
    input_file: str = typer.Argument(..., help="Pfad zur ProRes-Datei"),
    compressor_profile_path: str = typer.Argument(..., help="Pfad zur Compressor-Settings-Datei"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll"),
    delete_prores: bool = typer.Option(False, "--delete-prores", help="Lösche die ProRes-Datei nach erfolgreicher Komprimierung")
):
    """Komprimiert eine einzelne ProRes-Datei."""

    # Überprüfe und initialisiere die `output`-Variable außerhalb der asynchronen Funktion
    if output is None:
        output = os.path.dirname(input_file)
    
    # Definiere die asynchrone Funktion
    async def async_main():
        # Definiere einen Callback für den Abschluss der Komprimierung
        def on_completion(output_file):
            typer.echo(f"Komprimierung abgeschlossen für Datei: {output_file}")

        # Führe die Komprimierung durch
        await compress_prores_file(
            input_file=input_file,
            output_file=os.path.join(output, os.path.basename(input_file)),  # Verwende das Verzeichnis von output
            compressor_profile_path=compressor_profile_path,
            callback=on_completion,
            delete_prores=delete_prores
        )
    
    # Führe die asynchrone Hauptfunktion aus
    asyncio.run(async_main())

@app.command("list-profiles")
def list_profiles_command():
    """Listet alle verfügbaren Compressor-Profile auf."""
    profiles = list_profiles()
    if not profiles:
        typer.echo("Keine Compressor-Profile gefunden.")
    else:
        typer.echo("Verfügbare Compressor-Profile:")
        for profile in profiles:
            typer.echo(f"- {profile}")

if __name__ == "__main__":
    app()
