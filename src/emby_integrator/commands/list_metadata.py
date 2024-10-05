# src/emby_integrator/commands/list_metadata.py

import json
import typer
from metadata_manager import aggregate_metadata

def list_metadata_command(file_path: str, json_output: bool = False):
    """
    Extrahiere die Metadaten aus einer Datei und gebe sie aus.

    Diese Methode extrahiert relevante Metadaten wie Dateiname, Größe, Erstellungsdatum, Dauer, Videoformat 
    und andere Informationen aus der Datei mithilfe von ExifTool. Falls das Flag `--json-output` gesetzt wird, 
    wird die Ausgabe im JSON-Format zurückgegeben.

    Args:
        file_path (str): Der Pfad zur Datei, aus der die Metadaten extrahiert werden sollen.
        json_output (bool): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

    Returns:
        None: Gibt die extrahierten Metadaten in einer menschenlesbaren Form oder als JSON zurück, je nach dem Wert von `json_output`.
    
    Beispiel:
        $ emby-integrator get-metadata /path/to/video.mov

        Ausgabe:
        FileName: video.mov
        Directory: /path/to
        FileSize: 123456 bytes
        FileModificationDateTime: 2024-08-10 10:30:00
        ...
    
    Raises:
        FileNotFoundError: Wenn die angegebene Datei nicht existiert.
        ValueError: Wenn keine Metadaten extrahiert werden konnten.
    """
    try:
        metadata = aggregate_metadata(file_path)
        
        if json_output:
            # JSON-Ausgabe
            print(json.dumps(metadata, indent=4))
        else:
            # Menschenlesbare Ausgabe
            for key, value in metadata.items():
                print(f"{key}: {value}")
    except (FileNotFoundError, ValueError) as e:
        typer.secho(str(e), fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command(name="list-metadata")(list_metadata_command)