# original_media_integrator/app.py

import typer
from original_media_integrator.new_media_importer import import_media_only
from config_manager.config_loader import load_app_env
import logging

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
    source_dir: str = typer.Option(
        None,
        "--source-dir",
        "-s",
        help="Pfad zum Quellverzeichnis",
        envvar="original_media_source_dir"
    ),
    destination_dir: str = typer.Option(
        None,
        "--destination-dir",
        "-d",
        help="Pfad zum Zielverzeichnis",
        envvar="original_media_destination_dir"
    )
):
    """
    CLI-Kommando zum Importieren und Organisieren von Medien.

    ## Argumente:
    - **source_dir** (*str*): Pfad zum Quellverzeichnis.
    - **destination_dir** (*str*): Pfad zum Zielverzeichnis.

    ## Beispielaufrufe:
    ```bash
    original-media-integrator import-media --source-dir /Pfad/zum/Quellverzeichnis --destination-dir /Pfad/zum/Zielverzeichnis
    ```
    """
    logger.debug("Starte Import-Prozess")
    # Überprüfe, ob erforderliche Argumente vorhanden sind
    missing_args = []
    if not source_dir:
        missing_args.append("original_media_source_dir")
    if not destination_dir:
        missing_args.append("original_media_destination_dir")

    if missing_args:
        if env_path:
            typer.echo(f"Fehler: Die folgenden Konfigurationswerte fehlen: {', '.join(missing_args)}.")
            typer.echo(f"Bitte geben Sie die fehlenden Werte entweder über die CLI oder in der .env-Datei unter {env_path} an.")
        else:
            typer.echo(f"Fehler: Die folgenden Konfigurationswerte fehlen: {', '.join(missing_args)}.")
            typer.echo(f"Bitte geben Sie die fehlenden Werte entweder über die CLI oder erstellen Sie eine .env-Datei unter ~/Library/Application Support/Kurmann/Videoschnitt/.env.")
        logger.error(f"Fehlende Konfigurationswerte: {', '.join(missing_args)}")
        raise typer.Exit(code=1)

    # Führe die Importfunktion aus
    try:
        import_media_only(
            source_dir=source_dir,
            destination_dir=destination_dir
        )
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten während des Imports: {e}", fg=typer.colors.RED)
        logger.exception("Fehler während des Imports")
        raise typer.Exit(code=1)

    typer.secho("Import und Organisation abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Import und Organisation abgeschlossen.")

if __name__ == "__main__":
    app()