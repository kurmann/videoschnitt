# app.py

"""
Das 'app' Modul enthält die CLI-Befehle für den Emby Integrator.
Es ermöglicht die Interaktion mit der Anwendung über die Kommandozeile und nutzt die Funktionen
und Klassen der anderen Module.
"""

import json
import os
import typer
import xml.etree.ElementTree as ET
from emby_integrator.mediaset_manager import get_mediaserver_files
from emby_integrator.metadata_manager import (
    get_metadata, 
    parse_recording_date
)
from emby_integrator.nfo_generator import CustomProductionInfuseMetadata
from emby_integrator.video_manager import compress_masterfile
from emby_integrator.image_manager import convert_images_to_adobe_rgb
from emby_integrator.xml_utils import indent

app = typer.Typer(help="Emby Integrator")

@app.command()
def list_mediaserver_files(
    source_dir: str, 
    json_output: bool = typer.Option(False, help="Gebe die Ausgabe im JSON-Format aus")
):
    """
    Liste die Mediaserver-Dateien aus einem Verzeichnis auf und gruppiere sie nach Mediensets.

    Diese Methode durchsucht das angegebene Verzeichnis nach Videodateien und zugehörigen Titelbildern 
    und gruppiert diese nach Mediensets. Falls das Flag `--json-output` gesetzt wird, wird die Ausgabe 
    im JSON-Format zurückgegeben, andernfalls wird eine menschenlesbare Ausgabe erstellt, die die 
    Informationen bündig darstellt.

    Args:
        source_dir (str): Der Pfad zu dem Verzeichnis, das nach Mediendateien und Bildern durchsucht wird.
        json_output (bool): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

    Returns:
        None: Gibt die Mediensets in einer menschenlesbaren Form oder als JSON zurück, je nach dem Wert von `json_output`.
    
    Beispiel:
        $ emby-integrator list-mediaserver-files /path/to/mediadirectory

        Ausgabe:
        Medienset: 2024-08-27 Ann-Sophie Spielsachen Bett
        Videos:    2024-08-27 Ann-Sophie Spielsachen Bett.mov
        Titelbild: Kein Titelbild gefunden.
        ----------------------------------------
        Medienset: Ann-Sophie rennt (Testvideo)
        Videos:    Ann-Sophie rennt (Testvideo)-4K60-Medienserver.mov
        Titelbild: Ann-Sophie rennt (Testvideo).jpg
        ----------------------------------------
    
    Raises:
        FileNotFoundError: Wenn das angegebene Verzeichnis nicht existiert.
    """
    media_sets = get_mediaserver_files(source_dir)
    
    if json_output:
        # JSON-Ausgabe
        print(json.dumps(media_sets, indent=4))
    else:
        # Bündige Ausgabe der Informationen
        max_label_length = 10  # Feste Länge für die Labels "Videos" und "Titelbild"
        
        for set_name, data in media_sets.items():
            print(f"Medienset: {set_name}")
            print(f"{'Videos:':<{max_label_length}} {', '.join(data['videos']) if data['videos'] else 'Keine Videodateien gefunden.'}")
            print(f"{'Titelbild:':<{max_label_length}} {data['image'] if data['image'] else 'Kein Titelbild gefunden.'}")
            print("-" * 40)


@app.command(name="compress-masterfile")
def compress_masterfile_command(
    input_file: str, 
    delete_master_file: bool = typer.Option(False, help="Lösche die Master-Datei nach der Komprimierung.")
):
    """
    Komprimiere eine Master-Datei.

    Diese Methode startet die Kompression der angegebenen Datei und bietet die Möglichkeit, 
    nach Abschluss der Komprimierung die Originaldatei zu löschen.

    Args:
        input_file (str): Der Pfad zur Master-Datei, die komprimiert werden soll.
        delete_master_file (bool): Optional. Gibt an, ob die Master-Datei nach der Komprimierung gelöscht werden soll. Standard ist `False`.

    Returns:
        None
    
    Beispiel:
        $ emby-integrator compress-masterfile /path/to/masterfile.mov --delete-master-file
    """
    
    # Definiere einen Callback, der eine Benachrichtigung sendet, wenn die Komprimierung abgeschlossen ist
    def notify_completion(input_file, output_file):
        print(f"Komprimierung abgeschlossen für: {input_file}")
    
    compress_masterfile(input_file, delete_master_file, callback=notify_completion)


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
        metadata = get_metadata(file_path)
        
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
        metadata = get_metadata(file_path)
        recording_date = parse_recording_date(file_path)
        if recording_date is None:
            raise ValueError(f"Konnte kein Aufnahmedatum aus dem Dateinamen '{file_path}' extrahieren.")

        custom_metadata = CustomProductionInfuseMetadata.create_from_metadata(metadata, recording_date)
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
        metadata = get_metadata(file_path)
        recording_date = parse_recording_date(file_path)
        if recording_date is None:
            raise ValueError(f"Konnte kein Aufnahmedatum aus dem Dateinamen '{file_path}' extrahieren.")

        custom_metadata = CustomProductionInfuseMetadata.create_from_metadata(metadata, recording_date)
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