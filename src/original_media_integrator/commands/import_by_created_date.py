import os
import shutil
import logging
from datetime import datetime
from pathlib import Path
import typer
from original_media_integrator.media_manager import remove_empty_directories

app = typer.Typer(help="Importiert Mediendateien basierend auf dem File Created Date")

logger = logging.getLogger(__name__)
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[logging.StreamHandler()]
)

@app.command()
def import_by_created_date(
    source_dir: Path = typer.Argument(..., help="Pfad zum Quellverzeichnis"),
    destination_dir: Path = typer.Argument(..., help="Pfad zum Zielverzeichnis")
):
    """
    Importiert Mediendateien basierend auf dem File Created Date ins Zielverzeichnis.

    - Erstellt ein Unterverzeichnis im Zielverzeichnis basierend auf dem ISO-Datum des Erstellungsdatums.
    - Beibehaltung der relativen Unterverzeichnisstruktur des Quellverzeichnisses innerhalb des Datumsverzeichnisses.
    - Entfernt leere Verzeichnisse im Eingangsverzeichnis nach dem Verschieben der Dateien.
    """
    source_dir = source_dir.resolve()
    destination_dir = destination_dir.resolve()

    logger.info(f"Starte Import von {source_dir} nach {destination_dir}")

    for root, _, files in os.walk(source_dir):
        for filename in files:
            if filename.startswith('.') or not filename.lower().endswith(
                ('.mov', '.mp4', '.jpg', '.jpeg', '.png', '.heif', '.heic', '.dng')
            ):
                logger.debug(f"Überspringe Datei {filename} aufgrund des Dateityps oder versteckter Datei.")
                continue

            source_file = Path(root) / filename

            try:
                # Abrufen des Erstellungsdatums
                created_date = datetime.fromtimestamp(source_file.stat().st_ctime).strftime('%Y-%m-%d')

                # Zielverzeichnis basierend auf Erstellungsdatum und relativer Struktur
                relative_path = Path(root).relative_to(source_dir)
                date_path = destination_dir / created_date / relative_path

                # Zielverzeichnis erstellen
                date_path.mkdir(parents=True, exist_ok=True)

                # Zielpfad für die Datei
                destination_file = date_path / filename

                logger.info(f"Verschiebe Datei {source_file} nach {destination_file}")
                shutil.move(str(source_file), str(destination_file))
            except Exception as e:
                logger.error(f"Fehler beim Verschieben von {source_file}: {e}")
                typer.secho(f"Fehler beim Verschieben von {source_file}: {e}", fg=typer.colors.RED)

    # Entferne leere Verzeichnisse im Eingangsverzeichnis
    logger.info(f"Überprüfe auf leere Verzeichnisse in: {source_dir}")
    remove_empty_directories(source_dir)

    typer.secho("Import abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Import abgeschlossen.")

if __name__ == "__main__":
    app()