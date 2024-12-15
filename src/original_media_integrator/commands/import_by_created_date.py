import os
import re
import shutil
import logging
from datetime import datetime
from pathlib import Path
import typer
from original_media_integrator.media_manager import remove_empty_directories
from metadata_manager.exif import get_creation_datetime

app = typer.Typer(help="Importiert Mediendateien basierend auf dem Dateinamen, EXIF-Datum oder File Created Date.")

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
    Importiert Mediendateien und organisiert sie in einer Verzeichnisstruktur: Jahr/Jahr-Monat/Jahr-Monat-Tag.

    - Unterstützte Dateiformate: .mov, .mp4, .jpg, .jpeg, .png, .heif, .heic, .dng.
    - Unterstützte Dateinamenformate: YYYY-MM-DD oder YYYY-MM-DD_hh-mm-ss.
    - Zielstruktur: /Zielverzeichnis/Jahr/Jahr-Monat/Jahr-Monat-Tag/Datei.ext.

    Verhindert, dass bereits bestehende Datumsverzeichnisse doppelt erstellt werden.
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
                # 1. Datum extrahieren (Präferenz: Dateiname > EXIF > File Created Date)
                date_match = re.match(r'(\d{4}-\d{2}-\d{2})(?:_(\d{2}-\d{2}-\d{2}))?', filename)
                if date_match:
                    date_str = date_match.group(1)
                    creation_datetime = datetime.strptime(date_str, '%Y-%m-%d')
                    logger.info(f"Extrahiere Datum {date_str} aus Dateinamen {filename}.")
                else:
                    # Fallback: Versuche EXIF-Daten zu lesen
                    creation_datetime = get_creation_datetime(str(source_file))
                    if not creation_datetime:
                        logger.warning(f"EXIF-Datum nicht gefunden für {source_file}, verwende File Created Date.")
                        creation_datetime = datetime.fromtimestamp(source_file.stat().st_ctime)
                
                # 2. Zielverzeichnisstruktur erstellen (Jahr/Jahr-Monat/Jahr-Monat-Tag)
                year = creation_datetime.strftime('%Y')
                year_month = creation_datetime.strftime('%Y-%m')
                year_month_day = creation_datetime.strftime('%Y-%m-%d')
                date_path = destination_dir / year / year_month / year_month_day

                # Verzeichnis erstellen (falls nicht vorhanden)
                date_path.mkdir(parents=True, exist_ok=True)

                # 3. Zielpfad prüfen und Datei verschieben
                destination_file = date_path / filename
                if destination_file.exists():
                    logger.warning(f"Datei {destination_file} existiert bereits, überspringe...")
                    continue

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