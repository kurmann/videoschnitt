# src/metadata_manager/cli/commands/is_hevc_a.py

import typer
from pathlib import Path
from metadata_manager.utils import is_hevc_a

def is_hevc_a_command(
    file_path: Path = typer.Argument(..., help="Pfad zur Videodatei, die überprüft werden soll")
):
    """
    Überprüft, ob eine Videodatei HEVC-A ist (Bitrate > 80 Mbit/s).

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Videodatei, die überprüft werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager is-hevc-a /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).
    ```
    """
    try:
        hevc_a = is_hevc_a(str(file_path))
        if hevc_a:
            typer.secho("Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).", fg=typer.colors.GREEN)
        else:
            typer.secho("Die Datei ist nicht HEVC-A (Bitrate <= 80 Mbit/s).", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command()(is_hevc_a_command)