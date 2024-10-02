# src/metadata_manager/cli/commands/get_bitrate.py

import typer
from pathlib import Path
from metadata_manager.utils import get_bitrate

def get_bitrate_command(
    file_path: Path = typer.Argument(..., help="Pfad zur Videodatei, deren Bitrate abgerufen werden soll")
):
    """
    Gibt die Bitrate einer Videodatei zurück.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Videodatei, deren Bitrate abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-bitrate /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Bitrate: 1069.47 Mbps
    ```
    """
    try:
        bitrate = get_bitrate(str(file_path))
        if bitrate:
            # Umrechnung der Bitrate in Mbps für die Ausgabe
            bitrate_mbps = float(bitrate) / 1_000_000
            typer.echo(f"Bitrate: {bitrate_mbps:.2f} Mbps")
        else:
            typer.secho("Bitrate konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command()(get_bitrate_command)