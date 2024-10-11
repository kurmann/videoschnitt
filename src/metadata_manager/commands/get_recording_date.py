import logging
import re
import typer
from typing import Optional
from datetime import datetime
from metadata_manager.commands.get_title import get_title

logger = logging.getLogger(__name__)

def get_recording_date_command(
    file_name: str = typer.Argument(..., help="Dateiname, aus dem das Aufnahmedatum extrahiert werden soll"),
    file_path: Optional[str] = typer.Option(None, "--file-path", "-f", help="Pfad zur Datei, um das Datum aus dem Titel zu extrahieren"),
    filename_only: bool = typer.Option(False, "--filename-only", "-n", help="Extrahiere das Aufnahmedatum nur aus dem Dateinamen")
):
    """
    Extrahiert das Aufnahmedatum aus Dateiname oder Titel.

    Gibt einen Fehler aus, wenn kein Datum gefunden wird. Standardmäßig wird zuerst aus dem Dateinamen extrahiert (Rang 1)
    und bei Misserfolg aus dem Titel (Rang 2), wenn ein Dateipfad angegeben ist.
    """
    recording_date = None

    # Option: Extrahiere nur aus dem Dateinamen
    if filename_only:
        recording_date = parse_recording_date_from_filename(file_name)
    else:
        # Versuche zuerst das Datum aus dem Dateinamen zu extrahieren (Rang 1)
        recording_date = parse_recording_date_from_filename(file_name)

        # Wenn im Dateinamen kein Datum gefunden wird, versuche es mit dem Titel (Rang 2)
        if not recording_date and file_path:
            title = get_title(file_path)
            if title:
                recording_date = parse_recording_date_from_filename(title)
    
    if recording_date:
        typer.secho(f"Aufnahmedatum: {recording_date.strftime('%A, %d. %B %Y')}", fg=typer.colors.GREEN)
    else:
        typer.secho("Kein Aufnahmedatum gefunden.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

def parse_date_from_string(date_string: str) -> Optional[datetime]:
    """
    Hilfsfunktion, um ein Datum aus einem String zu extrahieren.
    Berücksichtigt verschiedene Trennzeichen wie Leerschläge und Underscores.
    
    Args:
        date_string (str): Der String, der das Datum enthält.
        
    Returns:
        datetime | None: Das extrahierte Datum als datetime-Objekt, oder None, wenn kein gültiges Datum gefunden wurde.
    """
    # Regulärer Ausdruck für Datum im Format YYYY-MM-DD, wobei Leerschläge und Underscores als Trennzeichen erlaubt sind
    date_pattern = re.compile(r"(\d{4})[-_](\d{2})[-_](\d{2})")

    match = date_pattern.search(date_string)
    if match:
        year, month, day = match.groups()
        try:
            return datetime(int(year), int(month), int(day))
        except ValueError:
            return None
    return None

def parse_recording_date_from_filename(file_name: str) -> Optional[datetime]:
    """
    Extrahiert das Aufnahmedatum aus dem Dateinamen.

    Args:
        file_name (str): Der Dateiname, aus dem das Aufnahmedatum extrahiert werden soll.

    Returns:
        datetime | None: Das extrahierte Aufnahmedatum als datetime-Objekt, oder None, wenn kein Datum gefunden wurde.
    """
    recording_date = parse_date_from_string(file_name)
    if recording_date:
        logger.info(f"Aufnahmedatum aus Dateinamen extrahiert: {recording_date}")
        return recording_date

    logger.warning(f"Kein Aufnahmedatum im Dateinamen gefunden: {file_name}")
    return None

if __name__ == "__main__":
    typer.run(get_recording_date_command)