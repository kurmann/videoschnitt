# src/apple_compressor_manager/utils/file_utils.py

import os
import osxmetadata
from osxmetadata import Tag  # Importiere das Tag namedtuple
import typer  # Typer für die CLI-Integration
import logging

logger = logging.getLogger(__name__)

# Modulvariable für das Compression-Tag
COMPRESSION_TAG_NAME = "An Apple Kompressor übergeben"
COMPRESSION_TAG_COLOR = 0  # Standardfarbe (z.B. 0 für keine Farbe)

def add_compression_tag(file_path: str):
    """
    Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu, sofern es noch nicht vorhanden ist.

    ## Argumente:
    - **file_path** (*str*): Der Pfad zur Datei, die getaggt werden soll.

    ## Beispielaufruf:
    ```python
    add_compression_tag("/Pfad/zur/Datei.mov")
    ```
    """
    logger.debug(f"Versuche, Tag zu Datei hinzuzufügen: {file_path}")
    if not os.path.exists(file_path):
        logger.error(f"Datei existiert nicht: {file_path}")
        typer.secho(f"Fehler beim Hinzufügen des Tags zu '{file_path}': Datei existiert nicht.", fg=typer.colors.RED)
        return

    try:
        metadata = osxmetadata.OSXMetaData(file_path)
        
        # Erstelle ein Tag namedtuple mit Name und Farbe
        compression_tag = Tag(name=COMPRESSION_TAG_NAME, color=COMPRESSION_TAG_COLOR)
        
        # Erstelle eine Liste der aktuellen Tags
        existing_tags = metadata.tags or []
        
        # Prüfe, ob das Tag bereits vorhanden ist
        if compression_tag in existing_tags:
            typer.echo(f"Tag '{COMPRESSION_TAG_NAME}' ist bereits vorhanden für {file_path}. Hinzufügen wird übersprungen.")
            logger.info(f"Tag '{COMPRESSION_TAG_NAME}' ist bereits vorhanden für {file_path}. Hinzufügen wird übersprungen.")
        else:
            # Füge das Tag hinzu
            metadata.tags = existing_tags + [compression_tag]
            typer.echo(f"Tag '{COMPRESSION_TAG_NAME}' zu {file_path} hinzugefügt.")
            logger.info(f"Tag '{COMPRESSION_TAG_NAME}' zu {file_path} hinzugefügt.")
    except Exception as e:
        typer.secho(f"Fehler beim Hinzufügen des Tags zu '{file_path}': {e}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Hinzufügen des Tags zu '{file_path}': {e}")