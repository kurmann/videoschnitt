import typer
from pathlib import Path
from original_media_integrator.media_manager import organize_media_files
import logging

app = typer.Typer(help="Importiert Mediendateien basierend auf EXIF Creation Date")

logger = logging.getLogger(__name__)
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[logging.StreamHandler()]
)

@app.command()
def import_by_exif_creation_date(
    source_dir: Path = typer.Argument(..., help="Pfad zum Quellverzeichnis"),
    destination_dir: Path = typer.Argument(..., help="Pfad zum Zielverzeichnis"),
    base_source_dir: Path = typer.Option(
        None, help="Wurzelverzeichnis zur Berechnung des relativen Pfads. Standard: source_dir"
    ),
):
    """
    Importiert Mediendateien basierend auf dem EXIF-Erstellungsdatum (CreationDate).

    - Beibehaltung der relativen Unterverzeichnisstruktur des Quellverzeichnisses.
    - Organisation der Dateien nach Jahr/Monat/Tag basierend auf EXIF-Daten.
    - Unterstützt Videodateien (z. B. MOV, MP4) und Bilddateien (z. B. JPG, PNG).

    Beispiel:
        python -m original_media_integrator import-by-exif-creation-date /source /destination
    """
    try:
        source_dir = source_dir.resolve()
        destination_dir = destination_dir.resolve()
        base_source_dir = base_source_dir.resolve() if base_source_dir else None

        logger.info(f"Importiere Medien von {source_dir} nach {destination_dir}")
        organize_media_files(str(source_dir), str(destination_dir), str(base_source_dir))
        typer.secho("Import abgeschlossen.", fg=typer.colors.GREEN)
    except Exception as e:
        logger.error(f"Fehler beim Importieren: {e}")
        typer.secho(f"Fehler beim Importieren: {e}", fg=typer.colors.RED)

if __name__ == "__main__":
    app()