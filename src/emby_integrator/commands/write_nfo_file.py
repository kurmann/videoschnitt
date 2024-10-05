# src/emby_integrator/commands/write_nfo_file.py

import typer
from metadata_manager import get_metadata_with_exiftool
from emby_integrator.nfo_generator import CustomProductionInfuseMetadata
from emby_integrator.xml_utils import indent
import xml.etree.ElementTree as ET
import os

def write_nfo_file_command(file_path: str):
    """
    Generiert die NFO-Metadatendatei und schreibt sie in eine Datei.

    Diese Methode extrahiert die relevanten Metadaten aus der angegebenen Videodatei, erstellt eine
    benutzerdefinierte NFO-Metadatendatei und schreibt das resultierende XML in eine `.nfo` Datei neben der Videodatei.

    Args:
        file_path (str): Pfad zur Videodatei.

    Returns:
        None: Schreibt die generierte NFO-Datei in das gleiche Verzeichnis wie die Videodatei.

    Beispiel:
        $ emby-integrator write-nfo-file /path/to/video.mov

        Ausgabe:
        NFO-Datei wurde erfolgreich erstellt: /path/to/video.nfo
    """
    try:
        # Extrahiere relevante Metadaten aus der Videodatei
        metadata = get_metadata_with_exiftool(file_path)
        
        # Erstelle ein benutzerdefiniertes NFO-Metadatenset
        custom_metadata = CustomProductionInfuseMetadata.create_from_metadata(metadata, file_path)
        
        # Konvertiere die Metadaten in ein XML-Element
        xml_element = custom_metadata.to_xml()

        # XML-Elemente einrücken für bessere Lesbarkeit
        indent(xml_element, space="  ")

        # Bestimme den Pfad für die NFO-Datei
        nfo_file_path = os.path.splitext(file_path)[0] + '.nfo'
        
        # Erstelle einen ElementTree und schreibe das XML in die NFO-Datei
        tree = ET.ElementTree(xml_element)
        tree.write(nfo_file_path, encoding='utf-8', xml_declaration=True)

        # Erfolgsmeldung ausgeben
        typer.secho(f"NFO-Datei wurde erfolgreich erstellt: {nfo_file_path}", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Schreiben der NFO-Datei: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command(name="write-nfo-file")(write_nfo_file_command)