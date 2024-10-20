import os
import logging
import typer
from datetime import datetime
from mediaset_manager.commands.create_video_metadata_file import create_video_metadata_file

logger = logging.getLogger(__name__)
app = typer.Typer()

# Unterstützte Video-Extensions
VIDEO_EXTENSIONS = {'.mp4', '.mov', '.m4v', '.avi', '.mkv'}

@app.command("create-metadata-file")
def create_metadata_file(
    metadata_source: str = typer.Argument(..., help="Pfad zur Mediadatei (z.B. eine Videodatei)")
):
    """
    Erstellt eine Metadaten.yaml-Datei für ein gegebenes Medienset basierend auf der Mediadatei.
    Erkennt automatisch den Medientyp und verwendet den entsprechenden spezifischen Command.
    """
    if not os.path.isfile(metadata_source):
        typer.secho(f"Die Datei '{metadata_source}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    _, ext = os.path.splitext(metadata_source)
    ext = ext.lower()
    
    if ext in VIDEO_EXTENSIONS:
        # Extrahiere notwendige Metadaten aus dem Dateinamen, z.B. "2024-10-10_Wanderung_auf_den_Napf.m4v"
        filename = os.path.basename(metadata_source)
        parts = filename.split('_')
        if len(parts) < 2:
            typer.secho("Der Dateiname muss das Format 'Erstellung_Title.ext' haben.", fg=typer.colors.RED)
            raise typer.Exit(code=1)
        creation_part = parts[0]
        title_part = '_'.join(parts[1:]).rsplit('.', 1)[0].replace('_', ' ')
        
        # Validierung und Flexibilität des Erstellungsdatums
        try:
            if len(creation_part) == 4:
                # Nur Jahr
                creation = creation_part
            elif len(creation_part) == 10:
                # Vollständiges Datum
                creation_obj = datetime.strptime(creation_part, "%Y-%m-%d")
                creation = creation_obj.strftime("%Y-%m-%d")
            else:
                raise ValueError
        except ValueError:
            typer.secho(f"Das Erstellungsdatum '{creation_part}' im Dateinamen ist kein gültiges Datum (erwartet 'YYYY' oder 'YYYY-MM-DD').", fg=typer.colors.RED)
            raise typer.Exit(code=1)
        
        # Extrahiere das Jahr aus dem Erstellungsdatum für die Verzeichnisbenennung
        erstellungsjahr = creation.split('-')[0]
        
        # Delegiere die Erstellung an den spezifischen Video-Command
        create_video_metadata_file(
            title=title_part,
            erstellungsjahr=erstellungsjahr,
            subtype=None  # Der spezifische Command fragt nach dem Untertyp
        )
    else:
        typer.secho(f"Medientyp mit der Erweiterung '{ext}' wird derzeit nicht unterstützt.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

if __name__ == "__main__":
    app()