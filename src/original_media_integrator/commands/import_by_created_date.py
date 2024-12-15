import os
import re
import shutil
import logging
from datetime import datetime
from pathlib import Path
import typer
from original_media_integrator.media_manager import remove_empty_directories
from metadata_manager.exif import get_creation_datetime

app = typer.Typer(help="Importiert Mediendateien basierend auf dem File Created Date oder EXIF-Datum.")

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
    Importiert Mediendateien basierend auf dem File Created Date, Dateinamen oder EXIF-Datum ins Zielverzeichnis.

    Unterstützte Dateinamenformate (wird bevorzugt verarbeitet, ohne EXIF-Daten auszulesen):
    1. 'YYYY-MM-DD_hh-mm-ss.ext' (z.B. '2024-10-29_19-24-54.mov')
    2. 'YYYY-MM-DD.ext' (z.B. '2024-10-29.mov')

    Wenn die Datei eines dieser Formate hat, wird das Datum direkt aus dem Dateinamen extrahiert.
    Andernfalls versucht das Script, das Datum aus EXIF-Daten zu lesen. Fallback: File Created Date.

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
                # 1. Versuche, das Datum direkt aus dem Dateinamen zu extrahieren
                date_match = re.match(r'(\d{4}-\d{2}-\d{2})(?:_(\d{2}-\d{2}-\d{2}))?', filename)
                if date_match:
                    # ISO-Datum aus dem Dateinamen
                    date_part = date_match.group(1)
                    time_part = date_match.group(2) or "12-00-00"  # Falls keine Uhrzeit, setze 12:00 Uhr
                    created_date = f"{date_part}_{time_part}".split('_')[0]
                    logger.info(f"Extrahiere Datum {created_date} aus Dateinamen {filename}.")
                else:
                    # 2. Versuche, das Erstellungsdatum aus EXIF-Daten zu lesen
                    creation_datetime = get_creation_datetime(str(source_file))
                    if not creation_datetime:
                        # Fallback: Verwende File Created Date
                        logger.warning(f"EXIF-Datum nicht gefunden für {source_file}, verwende File Created Date.")
                        creation_datetime = datetime.fromtimestamp(source_file.stat().st_ctime)

                    # Setze Zeit auf 12:00 Uhr, um Zeitzonenprobleme zu vermeiden
                    creation_datetime = creation_datetime.replace(hour=12, minute=0, second=0, microsecond=0)
                    created_date = creation_datetime.strftime('%Y-%m-%d')

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