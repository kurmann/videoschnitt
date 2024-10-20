# src/emby_integrator/commands/generate_nfo_xml.py

import typer
from metadata_manager import aggregate_metadata
from emby_integrator.nfo_generator import CustomProductionInfuseMetadata
from emby_integrator.xml_utils import indent
import xml.etree.ElementTree as ET

def generate_nfo_xml_command(file_path: str):
    """
    Generiert die NFO-Metadatendatei und gibt das XML aus.

    Diese Methode extrahiert die relevanten Metadaten aus der angegebenen Videodatei, erstellt eine
    benutzerdefinierte NFO-Metadatendatei und gibt das resultierende XML in der Konsole aus.

    Args:
        file_path (str): Pfad zur Videodatei.

    Returns:
        None: Gibt das generierte XML in der Konsole aus.

    Beispiel:
        $ emby-integrator generate-nfo-xml /path/to/video.mov

        Ausgabe:
        <?xml version="1.0" encoding="utf-8"?>
        <nfo>
            ...
        </nfo>
    """
    try:
        # Extrahiere relevante Metadaten aus der Videodatei
        metadata = aggregate_metadata(file_path)
        
        # Erstelle ein benutzerdefiniertes NFO-Metadatenset
        custom_metadata = CustomProductionInfuseMetadata.create_from_metadata(metadata, file_path)
        
        # Konvertiere die Metadaten in ein XML-Element
        xml_element = custom_metadata.to_xml()

        # XML-Elemente einrücken für bessere Lesbarkeit
        indent(xml_element, space="  ")

        # XML als String umwandeln
        xml_str = ET.tostring(xml_element, encoding='utf-8', method='xml').decode('utf-8')

        # XML-Deklaration hinzufügen
        xml_declaration = '<?xml version="1.0" encoding="utf-8"?>\n'
        full_xml = xml_declaration + xml_str

        # Ausgabe des XML
        print(full_xml)
    except Exception as e:
        typer.secho(f"Fehler beim Generieren des NFO-XML: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command(name="generate-nfo-xml")(generate_nfo_xml_command)