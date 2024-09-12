import os
import osxmetadata
from osxmetadata import Tag  # Importiere das Tag namedtuple
import typer  # Typer für die CLI-Integration

# Modulvariable für das Compression-Tag
COMPRESSION_TAG_NAME = "An Apple Kompressor übergeben"
COMPRESSION_TAG_COLOR = 0  # Standardfarbe (z.B. 0 für keine Farbe)

app = typer.Typer(help="CLI-Tools zur Verwaltung von Dateitags für den Apple Compressor Manager")

def add_compression_tag(file_path):
    """
    Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu, sofern es noch nicht vorhanden ist.
    
    Argumente:
    - file_path: Der Pfad zur Datei, die getaggt werden soll.
    
    Hinweis:
    - Falls das Tag bereits existiert, wird es nicht erneut hinzugefügt.
    """
    metadata = osxmetadata.OSXMetaData(file_path)
    
    # Erstelle ein Tag namedtuple mit Name und Farbe
    compression_tag = Tag(name=COMPRESSION_TAG_NAME, color=COMPRESSION_TAG_COLOR)
    
    # Prüfe, ob das Tag bereits vorhanden ist
    existing_tags = metadata.tags
    if compression_tag in existing_tags:
        print(f"Tag '{COMPRESSION_TAG_NAME}' ist bereits vorhanden für {file_path}. Hinzufügen wird übersprungen.")
    else:
        # Füge das Tag hinzu
        metadata.tags = existing_tags + [compression_tag]
        print(f"Tag '{COMPRESSION_TAG_NAME}' zu {file_path} hinzugefügt.")

def is_file_in_use(filepath):
    """Überprüft, ob eine Datei in Verwendung ist."""
    try:
        os.rename(filepath, filepath)  # Versuch, die Datei umzubenennen, um sicherzustellen, dass sie nicht in Verwendung ist.
        return False
    except OSError:
        return True

# CLI-Methode zur Verwendung von Typer
@app.command("add-compressor-tag")
def add_compressor_tag_cli(file_path: str = typer.Argument(..., help="Der Pfad zur Datei, die getaggt werden soll")):
    """
    CLI-Kommando: Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu, sofern es noch nicht vorhanden ist.
    
    Argumente:
    - file_path: Der Pfad zur Datei, die getaggt werden soll.
    
    Hinweis:
    - Falls das Tag bereits existiert, wird es nicht erneut hinzugefügt.
    """
    add_compression_tag(file_path)

if __name__ == "__main__":
    app()