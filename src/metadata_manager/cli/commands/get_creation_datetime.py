# src/metadata_manager/cli/commands/get_creation_datetime.py

import typer
from pathlib import Path
from metadata_manager.exif import get_creation_datetime

def get_creation_datetime_command(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, deren Erstellungsdatum abgerufen werden soll")
):
    """
    Gibt das Erstellungsdatum einer Mediendatei zurück.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Mediendatei, deren Erstellungsdatum abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-creation-datetime /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Erstellungsdatum: 2024-09-23T19:16:33+02:00
    ```
    """
    try:
        creation_datetime = get_creation_datetime(str(file_path))
        if creation_datetime:
            typer.echo(f"Erstellungsdatum: {creation_datetime.isoformat()}")
        else:
            typer.secho("Erstellungsdatum konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command()(get_creation_datetime_command)