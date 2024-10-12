# src/mediaset_manager/commands/create_metadata_file.py

import os
import yaml
import ulid
import logging
from typing import Optional, Dict, Any
import typer
from datetime import datetime

from metadata_manager.loader import aggregate_metadata  # Importiere die aggregierte Metadaten-Funktion
from metadata_manager.commands.get_recording_date import parse_recording_date_from_filename

logger = logging.getLogger(__name__)
app = typer.Typer()

def generate_ulid() -> str:
    return ulid.new().str

@app.command("create-metadata-file")
def create_metadata_file(
    metadata_source: str = typer.Argument(..., help="Pfad zur Metadaten-Quelle (z.B. eine Videodatei)")
):
    """
    Erstellt eine Metadaten.yaml-Datei für ein gegebenes Medienset basierend auf einer Metadaten-Quelle.
    """
    if not os.path.isfile(metadata_source):
        typer.secho(f"Die Datei '{metadata_source}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Bestimme den Namen und Pfad der Metadaten-Datei
    file_basename = os.path.basename(metadata_source)
    project_name = os.path.splitext(file_basename)[0]
    directory = os.path.dirname(metadata_source)
    metadata_filename = f"{file_basename}.yaml"
    metadata_path = os.path.join(directory, metadata_filename)
    
    # Überprüfe, ob die Metadaten-Datei bereits existiert
    if os.path.exists(metadata_path):
        overwrite = typer.confirm(f"Die Datei '{metadata_filename}' existiert bereits. Möchten Sie sie überschreiben?")
        if not overwrite:
            typer.secho("Operation abgebrochen. Die bestehende Metadaten-Datei wurde nicht überschrieben.", fg=typer.colors.YELLOW)
            raise typer.Exit()
    
    try:
        # Aggregiere die Metadaten
        metadata = aggregate_metadata(metadata_source)
        
        # Extrahiere das Aufnahmedatum
        recording_date = parse_recording_date_from_filename(project_name)
        if not recording_date:
            # Falls das Datum nicht aus dem Projektnamen extrahiert werden kann, verwende das Änderungsdatum der Datei
            recording_date = datetime.fromtimestamp(os.path.getmtime(metadata_source))
            logger.info(f"Verwende das Änderungsdatum der Datei für '{metadata_source}': {recording_date.strftime('%Y-%m-%d')}")
        
        # Generiere eine ULID für das Medienset
        medienset_id = generate_ulid()
        
        # Verarbeitung der Dauer (in Sekunden)
        duration_str = metadata.get("Duration", "0")
        try:
            if isinstance(duration_str, str) and duration_str.lower().endswith('s'):
                duration_seconds = round(float(duration_str.rstrip('s ').strip()))
            else:
                duration_seconds = round(float(duration_str))
        except ValueError:
            logger.warning(f"Ungültiges Format für Dauer: '{duration_str}'. Setze Dauer auf 0.")
            duration_seconds = 0
        
        # Erstelle die Metadatenstruktur
        metadaten = {
            "Id": medienset_id,
            "Titel": metadata.get("Title", ""),
            "Aufnahmedatum": recording_date.strftime("%Y-%m-%d"),
            "Beschreibung": metadata.get("Description", ""),
            "Copyright": metadata.get("Copyright", ""),
            "Veröffentlichungsdatum": metadata.get("releasedate", ""),
            "Studio": metadata.get("Studio", ""),
            "Schlüsselwörter": [kw.strip() for kw in metadata.get("Keywords", "").split(',') if kw.strip()],
            "Album": metadata.get("Album", ""),
            "Videoschnitt": [metadata.get("Producer", "")] if metadata.get("Producer", "") else [],
            "Aufnahmen": [metadata.get("Author", "")] if metadata.get("Author", "") else [],
            "Dauer_in_Sekunden": duration_seconds  # in Sekunden, gerundet
        }
        
        # Bereinige leere Felder und Listen
        metadaten = {k: v for k, v in metadaten.items() if v}
        
        # Speichere die Metadaten in der YAML-Datei
        with open(metadata_path, 'w', encoding='utf-8') as yaml_file:
            yaml.dump(metadaten, yaml_file, allow_unicode=True, sort_keys=False, default_flow_style=False)
        
        typer.secho(f"Metadaten erfolgreich in '{metadata_path}' gespeichert.", fg=typer.colors.GREEN)
    
    except Exception as e:
        logger.error(f"Fehler beim Generieren der Metadaten: {e}")
        typer.secho(f"Fehler beim Generieren der Metadaten: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

if __name__ == "__main__":
    app()