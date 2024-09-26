# src/apple_compressor_manager/commands/add_tag_command.py

import typer
from apple_compressor_manager.utils.file_utils import add_compression_tag

def add_tag_command(
    file_path: str = typer.Argument(..., help="Pfad zur Datei, die getaggt werden soll")
):
    """
    Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu.

    ## Argumente:
    - **file_path** (*str*): Pfad zur Datei, die getaggt werden soll.

    ## Beispielaufruf:
    ```bash
    apple-compressor-manager add-tag /Pfad/zur/Datei.mov
    ```
    """
    add_compression_tag(file_path)