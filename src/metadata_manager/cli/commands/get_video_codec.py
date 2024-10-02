# src/metadata_manager/cli/commands/get_video_codec.py

import typer
from pathlib import Path
from metadata_manager.utils import get_video_codec

def get_video_codec_command(
    file_path: Path = typer.Argument(..., help="Pfad zur Videodatei, deren Codec abgerufen werden soll")
):
    """
    Gibt den Videocodec einer Datei zurück.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Videodatei, deren Videocodec abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-video-codec /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Videocodec: prores
    ```
    """
    try:
        codec = get_video_codec(str(file_path))
        if codec:
            typer.echo(f"Videocodec: {codec}")
        else:
            typer.secho("Videocodec konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command()(get_video_codec_command)