# src/metadata_manager/cli/commands/get_album.py

import typer
from pathlib import Path
from metadata_manager.exif import get_album

def get_album_command(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, deren Album-Tag abgerufen werden soll")
):
    """
    Gibt den Album-Tag einer Mediendatei zurück.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Mediendatei, deren Album-Tag abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-album /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Album: MeinAlbum
    ```
    """
    try:
        album = get_album(str(file_path))
        if album:
            typer.echo(f"Album: {album}")
        else:
            typer.secho("Album-Tag konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command()(get_album_command)