# nfo_generator.py

"""
Das 'nfo_generator' Modul ist verantwortlich für die Erstellung von NFO-Dateien für den Emby-Medienserver.
Es enthält die Klasse 'CustomProductionInfuseMetadata', die die notwendigen Metadaten verwaltet und Methoden
zur Generierung des XML und zum Schreiben der NFO-Datei bereitstellt.
"""

import os
import xml.etree.ElementTree as ET
from datetime import datetime
from emby_integrator.xml_utils import indent

class CustomProductionInfuseMetadata:
    """
    Repräsentiert die Metadaten für eine benutzerdefinierte Produktion (Custom Production) und ermöglicht
    die Generierung des entsprechenden NFO-XML für den Emby-Medienserver.
    """

    def __init__(self, type, title, sorttitle, plot, artist, copyright, published, releasedate,
                 studio, keywords, album, producers, directors):
        self.type = type
        self.title = title
        self.sorttitle = sorttitle
        self.plot = plot  # 'description' geändert zu 'plot'
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
        """
        Erstellt eine Instanz von 'CustomProductionInfuseMetadata' basierend auf den gegebenen Metadaten
        und dem Aufnahmedatum.
        """
        if not metadata:
            raise ValueError("Die Metadaten sind leer.")

        # Initialisierung mit Standardwerten
        type = "Other"
        title = ''
        sorttitle = ''
        plot = ''  # 'description' geändert zu 'plot'
        artist = ''
        copyright = ''
        releasedate = None
        studio = ''
        keywords = ''
        album = ''
        producers = ['']
        directors = []

        # Metadaten auslesen und 'N/A' durch leere Strings ersetzen
        def clean_value(value):
            return value if value != 'N/A' else ''

        title_with_leading_date = clean_value(metadata.get('Title', ''))
        title = cls.get_title(title_with_leading_date, recording_date)
        sorttitle = title  # Sorttitle ist gleich dem bereinigten Titel
        plot = clean_value(metadata.get('Description', ''))
        artist = clean_value(metadata.get('Author', ''))
        copyright = clean_value(metadata.get('Copyright', ''))
        releasedate_str = clean_value(metadata.get('CreateDate', None))
        if releasedate_str:
            try:
                releasedate = datetime.strptime(releasedate_str, '%Y:%m:%d %H:%M:%S')
            except ValueError:
                releasedate = None
        studio = clean_value(metadata.get('Studio', ''))
        keywords = clean_value(metadata.get('Keywords', ''))
        album = clean_value(metadata.get('Album', ''))
        producer = clean_value(metadata.get('Producer', ''))
        producers = [producer] if producer else ['']
        # Directors können ähnlich gehandhabt werden, falls vorhanden

        published = recording_date

        return cls(type, title, sorttitle, plot, artist, copyright, published, releasedate,
                   studio, keywords, album, producers, directors)

    @staticmethod
    def get_title(title_with_leading_date, recording_date):
        """
        Entfernt das Aufnahmedatum aus dem Titel, um den eigentlichen Titel zu erhalten.
        """
        date_str = recording_date.strftime('%Y-%m-%d')
        return title_with_leading_date.replace(date_str, '').strip()

    def to_xml(self):
        """
        Generiert ein XML-Element basierend auf den Metadaten.
        """
        media = ET.Element('media', {'type': self.type})
        ET.SubElement(media, 'title').text = self.title
        ET.SubElement(media, 'sorttitle').text = self.sorttitle
        ET.SubElement(media, 'plot').text = self.plot  # 'description' geändert zu 'plot'
        ET.SubElement(media, 'artist').text = self.artist
        ET.SubElement(media, 'copyright').text = self.copyright
        ET.SubElement(media, 'published').text = self.published.strftime('%Y-%m-%d') if self.published else ''
        ET.SubElement(media, 'releasedate').text = self.releasedate.strftime('%Y-%m-%d') if self.releasedate else ''
        ET.SubElement(media, 'studio').text = self.studio
        ET.SubElement(media, 'keywords').text = self.keywords
        ET.SubElement(media, 'album').text = self.album

        if any(self.producers):
            producers_elem = ET.SubElement(media, 'producers')
            for producer in self.producers:
                if producer:
                    ET.SubElement(producers_elem, 'name').text = producer

        if any(self.directors):
            directors_elem = ET.SubElement(media, 'directors')
            for director in self.directors:
                if director:
                    ET.SubElement(directors_elem, 'name').text = director
        else:
            # Leeres 'directors' Element hinzufügen, wenn keine Daten vorhanden sind
            ET.SubElement(media, 'directors')

        return media

    def write_to_file(self, file_path):
        """
        Schreibt die Metadaten in eine NFO-Datei.
        """
        media_elem = self.to_xml()

        # XML-Elemente einrücken
        indent(media_elem, space="  ")

        tree = ET.ElementTree(media_elem)
        tree.write(file_path, encoding='utf-8', xml_declaration=True)

    def __str__(self):
        return f"Title: {self.title}, Published: {self.published}, Album: {self.album}"