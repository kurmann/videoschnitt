import os
import re
import subprocess
import json
import xml.etree.ElementTree as ET
from datetime import datetime

# Liste der benötigten Metadaten
METADATA_KEYS = [
    "FileName", "Directory", "FileSize", "FileModificationDateTime", "FileType", "MIMEType", 
    "CreateDate", "Duration", "AudioFormat", "ImageWidth", "ImageHeight", "CompressorID",
    "CompressorName", "BitDepth", "VideoFrameRate", "Title", "Album", "Description", "Copyright", 
    "Author", "Keywords", "AvgBitrate", "Producer", "Studio"
]

def get_metadata(file_path: str) -> dict:
    """
    Extrahiert die Metadaten aus einer Datei mithilfe des Exif-Tools und gibt ein strukturiertes Dictionary zurück.

    Argumente:
    - file_path: Der Pfad zur Datei, aus der die Metadaten extrahiert werden sollen.

    Rückgabewert:
    - Ein Dictionary, das die relevanten Metadaten enthält.
    """
    if not os.path.exists(file_path):
        raise FileNotFoundError(f"Die Datei '{file_path}' wurde nicht gefunden.")
    
    # Der Dateipfad muss in Anführungszeichen gesetzt werden, um Sonderzeichen zu behandeln
    command = f'exiftool -json "{file_path}"'
    try:
        result = subprocess.run(command, shell=True, capture_output=True, text=True, check=True)
        if not result.stdout:
            raise ValueError(f"Keine Ausgabe von ExifTool für '{file_path}'. Möglicherweise enthält die Datei keine Metadaten.")
        
        metadata_list = json.loads(result.stdout)
        metadata = metadata_list[0]  # Wir nehmen an, dass nur eine Datei übergeben wird

        # Filtern der gewünschten Metadaten
        filtered_metadata = {key: metadata.get(key, "N/A") for key in METADATA_KEYS}
        
        return filtered_metadata
    except subprocess.CalledProcessError as e:
        error_message = (
            f"Fehler beim Extrahieren der Metadaten für '{file_path}'.\n"
            f"Exit Code: {e.returncode}\n"
            f"Fehlerausgabe: {e.stderr.strip() if e.stderr else 'Keine Fehlermeldung verfügbar.'}\n"
            f"Vollständiger Befehl: {command}"
        )
        raise ValueError(error_message)

def parse_recording_date(file_path: str) -> datetime | None:
    """
    Extrahiert das Aufnahmedatum aus dem Dateinamen.

    Der Dateiname muss im Format 'YYYY-MM-DD <Rest des Dateinamens>' vorliegen.
    Wenn kein Datum im Dateinamen gefunden wird, wird None zurückgegeben.

    Args:
        file_path (str): Pfad zur Datei, deren Dateiname das Datum enthalten soll.

    Returns:
        datetime | None: Das extrahierte Datum als datetime-Objekt, oder None, wenn kein Datum gefunden wurde.
    """
    file_name = os.path.basename(file_path)
    match = re.search(r"\d{4}-\d{2}-\d{2}", file_name)
    if match:
        date_str = match.group()
        return datetime.strptime(date_str, "%Y-%m-%d")
    return None

class CustomProductionInfuseMetadata:
    def __init__(self, type, title, sorttitle, description, artist, copyright, published, releasedate,
                 studio, keywords, album, producers, directors):
        self.type = type
        self.title = title
        self.sorttitle = sorttitle
        self.description = description
        self.artist = artist
        self.copyright = copyright
        self.published = published  # datetime Objekt oder None
        self.releasedate = releasedate  # datetime Objekt oder None
        self.studio = studio
        self.keywords = keywords
        self.album = album
        self.producers = producers  # Liste von Namen
        self.directors = directors  # Liste von Namen

    @classmethod
    def create_from_metadata(cls, metadata, recording_date):
        if not metadata:
            raise ValueError("Die Metadaten sind leer.")

        # Initialisierung mit Standardwerten
        type = "Other"
        title = ''
        sorttitle = ''
        description = ''
        artist = ''
        copyright = ''
        releasedate = None
        studio = ''
        keywords = ''
        album = ''
        producers = ['']
        directors = []

        # Metadaten auslesen
        title_with_leading_date = metadata.get('Title', '')
        title = cls.get_title(title_with_leading_date, recording_date)
        sorttitle = metadata.get('Title', '')
        description = metadata.get('Description', '')
        artist = metadata.get('Author', '')
        copyright = metadata.get('Copyright', '')
        releasedate_str = metadata.get('CreateDate', None)
        if releasedate_str and releasedate_str != 'N/A':
            try:
                releasedate = datetime.strptime(releasedate_str, '%Y:%m:%d %H:%M:%S')
            except ValueError:
                releasedate = None
        studio = metadata.get('Studio', '')
        keywords = metadata.get('Keywords', '')
        album = metadata.get('Album', '')
        producers = [metadata.get('Producer', '')] if metadata.get('Producer', '') else ['']
        # Directors können ähnlich gehandhabt werden, falls vorhanden

        published = recording_date

        return cls(type, title, sorttitle, description, artist, copyright, published, releasedate,
                   studio, keywords, album, producers, directors)

    @staticmethod
    def get_title(title_with_leading_date, recording_date):
        date_str = recording_date.strftime('%Y-%m-%d')
        return title_with_leading_date.replace(date_str, '').strip()

    def to_xml(self):
        media = ET.Element('media', {'type': self.type})
        ET.SubElement(media, 'title').text = self.title
        ET.SubElement(media, 'sorttitle').text = self.sorttitle
        ET.SubElement(media, 'description').text = self.description
        ET.SubElement(media, 'artist').text = self.artist
        ET.SubElement(media, 'copyright').text = self.copyright
        ET.SubElement(media, 'published').text = self.published.strftime('%Y-%m-%d') if self.published else ''
        ET.SubElement(media, 'releasedate').text = self.releasedate.strftime('%Y-%m-%d') if self.releasedate else ''
        ET.SubElement(media, 'studio').text = self.studio
        ET.SubElement(media, 'keywords').text = self.keywords
        ET.SubElement(media, 'album').text = self.album

        producers_elem = ET.SubElement(media, 'producers')
        for producer in self.producers:
            ET.SubElement(producers_elem, 'name').text = producer

        directors_elem = ET.SubElement(media, 'directors')
        for director in self.directors:
            ET.SubElement(directors_elem, 'name').text = director

        return media

    def write_to_file(self, file_path):
        media_elem = self.to_xml()
        tree = ET.ElementTree(media_elem)
        tree.write(file_path, encoding='utf-8', xml_declaration=True)

    def __str__(self):
        return f"Title: {self.title}, Published: {self.published}, Album: {self.album}"