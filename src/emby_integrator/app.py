# emby_integrator/app.py

"""
Das 'app' Modul enthält die CLI-Befehle für den Emby Integrator.
Es ermöglicht die Interaktion mit der Anwendung über die Kommandozeile und nutzt die Funktionen
und Klassen der anderen Module.
"""

import json
import os
import typer
import xml.etree.ElementTree as ET
from metadata_manager import get_relevant_metadata, parse_recording_date
from emby_integrator.nfo_generator import CustomProductionInfuseMetadata
from emby_integrator.image_manager import convert_images_to_adobe_rgb
from emby_integrator.xml_utils import indent

app = typer.Typer(help="Emby Integrator")

@app.command(name="convert-images-to-adobe-rgb")
def convert_images_to_adobe_rgb_command(media_dir: str):
    """
    Konvertiere eine Liste von PNG-Bildern in Adobe RGB, falls eine passende Videodatei existiert.

    Diese Methode durchsucht das angegebene Verzeichnis nach PNG-Bildern und konvertiert sie in das 
    Adobe RGB-Farbprofil, falls eine passende Videodatei im gleichen Verzeichnis existiert.

    Args:
        media_dir (str): Der Pfad zu dem Verzeichnis, das nach PNG-Bildern und passenden Videodateien durchsucht wird.

    Returns:
        None
    
    Beispiel:
        $ emby-integrator convert-images-to-adobe-rgb /path/to/mediadirectory
    """
    image_files = [os.path.join(media_dir, f) for f in os.listdir(media_dir) if f.lower().endswith(".png")]
    convert_images_to_adobe_rgb(image_files, media_dir)
    
@app.command()
def list_metadata(
    file_path: str, 
    json_output: bool = typer.Option(False, help="Gebe die Ausgabe im JSON-Format aus")
):
    """
    Extrahiere die Metadaten aus einer Datei und gebe sie aus.

    Diese Methode extrahiert relevante Metadaten wie Dateiname, Größe, Erstellungsdatum, Dauer, Videoformat 
    und andere Informationen aus der Datei mithilfe von ExifTool. Falls das Flag `--json-output` gesetzt wird, 
    wird die Ausgabe im JSON-Format zurückgegeben.

    Args:
        file_path (str): Der Pfad zur Datei, aus der die Metadaten extrahiert werden sollen.
        json_output (bool): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

    Returns:
        None: Gibt die extrahierten Metadaten in einer menschenlesbaren Form oder als JSON zurück, je nach dem Wert von `json_output`.
    
    Beispiel:
        $ emby-integrator get-metadata /path/to/video.mov

        Ausgabe:
        FileName: video.mov
        Directory: /path/to
        FileSize: 123456 bytes
        FileModificationDateTime: 2024-08-10 10:30:00
        ...
    
    Raises:
        FileNotFoundError: Wenn die angegebene Datei nicht existiert.
        ValueError: Wenn keine Metadaten extrahiert werden konnten.
    """
    try:
        metadata = get_relevant_metadata(file_path)
        
        if json_output:
            # JSON-Ausgabe
            print(json.dumps(metadata, indent=4))
        else:
            # Menschenlesbare Ausgabe
            for key, value in metadata.items():
                print(f"{key}: {value}")
    except (FileNotFoundError, ValueError) as e:
        typer.secho(str(e), fg=typer.colors.RED)

@app.command(name="get-recording-date")
def get_recording_date_command(file_path: str):
    """
    Gibt das Aufnahmedatum aus dem Dateinamen in einem deutschen Datumsformat mit Wochentag aus.

    Args:
        file_path (str): Pfad zur Datei, deren Aufnahmedatum im Dateinamen enthalten sein soll.
    """
    recording_date = parse_recording_date(file_path)
    
    if recording_date:
        # Datumsformat auf Deutsch: "Montag, 27. August 2024"
        formatted_date = recording_date.strftime("%A, %d. %B %Y")
        typer.secho(f"Aufnahmedatum: {formatted_date}", fg=typer.colors.GREEN)
    else:
        typer.secho(f"Kein gültiges Aufnahmedatum im Dateinamen gefunden: {file_path}", fg=typer.colors.RED)

@app.command()
def generate_nfo_xml(file_path: str):
    """
    Generiert die NFO-Metadatendatei und gibt das XML aus.

    Args:
        file_path (str): Pfad zur Videodatei.
    """
    try:
        metadata = get_relevant_metadata(file_path)
        custom_metadata = CustomProductionInfuseMetadata.create_from_metadata(metadata, file_path)
        xml_element = custom_metadata.to_xml()

        # XML-Elemente einrücken
        indent(xml_element, space="  ")

        # XML als String ausgeben
        xml_str = ET.tostring(xml_element, encoding='utf-8', method='xml').decode('utf-8')

        # XML-Deklaration hinzufügen
        xml_declaration = '<?xml version="1.0" encoding="utf-8"?>\n'
        full_xml = xml_declaration + xml_str

        print(full_xml)
    except Exception as e:
        typer.secho(f"Fehler beim Generieren des NFO-XML: {e}", fg=typer.colors.RED)

@app.command()
def write_nfo_file(file_path: str):
    """
    Generiert die NFO-Metadatendatei und schreibt sie in eine Datei.

    Args:
        file_path (str): Pfad zur Videodatei.
    """
    try:
        metadata = get_relevant_metadata(file_path)
        custom_metadata = CustomProductionInfuseMetadata.create_from_metadata(metadata, file_path)
        nfo_file_path = os.path.splitext(file_path)[0] + '.nfo'

        # XML-Elemente einrücken
        xml_element = custom_metadata.to_xml()
        indent(xml_element, space="  ")
        tree = ET.ElementTree(xml_element)
        tree.write(nfo_file_path, encoding='utf-8', xml_declaration=True)

        typer.secho(f"NFO-Datei wurde erfolgreich erstellt: {nfo_file_path}", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Schreiben der NFO-Datei: {e}", fg=typer.colors.RED)

if __name__ == '__main__':
    app()