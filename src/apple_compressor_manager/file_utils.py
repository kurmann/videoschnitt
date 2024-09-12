import os
import osxmetadata

# Modulvariable für das Compression-Tag
COMPRESSION_TAG = "An Apple Kompressor übergeben"

def add_compression_tag(file_path):
    """
    Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu.
    
    Argumente:
    - file_path: Der Pfad zur Datei, die getaggt werden soll.
    """
    metadata = osxmetadata.OSXMetaData(file_path)
    metadata.tags = metadata.tags + [COMPRESSION_TAG]
    print(f"Tag '{COMPRESSION_TAG}' zu {file_path} hinzugefügt.")

def is_file_in_use(filepath):
    """Überprüft, ob eine Datei in Verwendung ist."""
    try:
        os.rename(filepath, filepath)  # Versuch, die Datei umzubenennen, um sicherzustellen, dass sie nicht in Verwendung ist.
        return False
    except OSError:
        return True