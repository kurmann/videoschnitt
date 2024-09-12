import os
import osxmetadata
from osxmetadata import Tag  # Importiere das Tag namedtuple

# Modulvariable für das Compression-Tag
COMPRESSION_TAG_NAME = "An Apple Kompressor übergeben"
COMPRESSION_TAG_COLOR = 0  # Standardfarbe (z.B. 0 für keine Farbe)

def add_compression_tag(file_path):
    """
    Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu.
    
    Argumente:
    - file_path: Der Pfad zur Datei, die getaggt werden soll.
    """
    metadata = osxmetadata.OSXMetaData(file_path)
    
    # Erstelle ein Tag namedtuple mit Name und Farbe
    compression_tag = Tag(name=COMPRESSION_TAG_NAME, color=COMPRESSION_TAG_COLOR)
    
    # Füge das Tag hinzu
    metadata.tags = metadata.tags + [compression_tag]
    
    print(f"Tag '{COMPRESSION_TAG_NAME}' zu {file_path} hinzugefügt.")

def is_file_in_use(filepath):
    """Überprüft, ob eine Datei in Verwendung ist."""
    try:
        os.rename(filepath, filepath)  # Versuch, die Datei umzubenennen, um sicherzustellen, dass sie nicht in Verwendung ist.
        return False
    except OSError:
        return True