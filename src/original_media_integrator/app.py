# src/original_media_integrator/app.py

import typer
from typing import List, Optional
from original_media_integrator.new_media_importer import import_media_only
from config_manager.config_loader import load_app_env
import logging
import subprocess

# Lade die .env Datei
env_path = load_app_env()

# Initialisiere das Logging
logging.basicConfig(
    level=logging.INFO,  # Setze auf INFO, kann bei Bedarf auf DEBUG geändert werden
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("original_media_integrator.log"),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

app = typer.Typer(help="Original Media Integrator")

@app.command()
def import_media(
    source_dirs: Optional[str] = typer.Option(
        None,
        "--source-dir",
        "-s",
        help="Pfad zum Quellverzeichnis. Kann als durch Kommas getrennte Liste angegeben werden.",
        envvar="original_media_source_dirs",
    ),
    destination_dir: Optional[str] = typer.Option(
        None,
        "--destination-dir",
        "-d",
        help="Pfad zum Zielverzeichnis",
        envvar="original_media_destination_dir"
    )
):
    """
    CLI-Kommando zum Importieren und Organisieren von Medien aus angegebenen Quellverzeichnissen.
    """
    # Wenn source_dirs über die CLI oder envvar gesetzt wurde, splitte sie in eine Liste
    if source_dirs:
        # Splitte die Quelle nach Kommas und entferne Leerzeichen
        source_dirs_list = [s.strip() for s in source_dirs.split(',') if s.strip()]
    else:
        source_dirs_list = []

    # Überprüfe auf fehlende Argumente
    missing_args = []
    if not source_dirs_list:
        missing_args.append("original_media_source_dirs")
    if not destination_dir:
        missing_args.append("original_media_destination_dir")

    if missing_args:
        typer.echo(f"Fehler: Die folgenden Konfigurationswerte fehlen: {', '.join(missing_args)}.")
        typer.echo(f"Bitte geben Sie die fehlenden Werte entweder über die CLI oder in der .env-Datei unter {env_path} an.")
        logger.error(f"Fehlende Konfigurationswerte: {', '.join(missing_args)}")
        raise typer.Exit(code=1)

    # Führe die Importfunktion für jedes Quellverzeichnis aus
    for idx, source_dir in enumerate(source_dirs_list, start=1):
        typer.echo(f"Importiere aus Quellverzeichnis {idx}: {source_dir}")
        logger.info(f"Importiere aus Quellverzeichnis {idx}: {source_dir}")
        try:
            import_media_only(
                source_dir=source_dir,
                destination_dir=destination_dir
            )
        except Exception as e:
            typer.secho(f"Ein Fehler ist aufgetreten beim Importieren aus {source_dir}: {e}", fg=typer.colors.RED)
            logger.exception(f"Fehler beim Importieren aus {source_dir}: {e}")

    typer.secho("Import und Organisation abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Import und Organisation abgeschlossen.")

if __name__ == "__main__":
    app()