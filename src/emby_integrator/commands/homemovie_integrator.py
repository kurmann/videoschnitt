# src/emby_integrator/commands/homemovie_integrator.py

import typer
from pathlib import Path
from typing import Optional, List, Dict
import shutil
from datetime import datetime
import subprocess
import json
import xml.etree.ElementTree as ET
import logging
import re
import unicodedata

app = typer.Typer()

# Konfiguriere das Logging
logging.basicConfig(
    filename='homemovie_integrator.log',
    filemode='a',
    format='%(asctime)s - %(levelname)s - %(message)s',
    level=logging.DEBUG  # Setze auf DEBUG für detailliertes Logging
)
logger = logging.getLogger(__name__)

# Modulvariablen
NFO_SUFFIX = ".nfo"

# Konstante für Adobe RGB Profil
ADOBE_RGB_PROFILE = "/System/Library/ColorSync/Profiles/AdobeRGB1998.icc"
SUPPORTED_IMAGE_FORMATS = [".jpg", ".jpeg", ".png"]
SUPPORTED_AUDIO_FORMATS = [".m4a", ".mp3", ".aac"]  # Passe die Formate nach Bedarf an

# Manuelle Zuordnung der Wochentage zu deutschen Abkürzungen
WEEKDAY_MAP = {
    0: "Mo.",
    1: "Di.",
    2: "Mi.",
    3: "Do.",
    4: "Fr.",
    5: "Sa.",
    6: "So."
}

def validate_filename(filename: str) -> bool:
    """
    Überprüft, ob der Dateiname gültige Zeichen enthält.
    Gültige Zeichen sind in der Regel alle, die vom Betriebssystem unterstützt werden.
    Hier wird eine einfache Überprüfung auf häufig ungültige Zeichen durchgeführt.
    """
    # Definiere ungültige Zeichen (für Windows als Beispiel; anpassen für andere OS)
    invalid_chars = r'<>:"/\|?*'
    if any(char in invalid_chars for char in filename):
        logger.debug(f"Ungültige Zeichen gefunden im Dateinamen: '{filename}'")
        return False
    return True

def extract_metadata(file_path: Path) -> dict:
    """
    Extrahiert relevante Metadaten aus der Videodatei mithilfe von ExifTool.
    """
    command = ['exiftool', '-json', str(file_path)]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        typer.secho(f"Fehler beim Ausführen von ExifTool: {result.stderr}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Ausführen von ExifTool: {result.stderr}")
        raise typer.Exit(code=1)
    
    try:
        metadata_list = json.loads(result.stdout)
        if not metadata_list:
            raise ValueError("Keine Metadaten gefunden.")
        metadata = metadata_list[0]
    except json.JSONDecodeError:
        typer.secho("Ungültige JSON-Ausgabe von ExifTool.", fg=typer.colors.RED)
        logger.error("Ungültige JSON-Ausgabe von ExifTool.")
        raise typer.Exit(code=1)
    
    # Filtern der relevanten Metadaten
    relevant_keys = [
        "Title", "Description", "Author", "Keywords", "Producer",
        "Director", "Album", "CreateDate", "Artist", "DisplayName",
        "CreationDate", "VideoCodec"
    ]
    filtered_metadata = {key: metadata.get(key, '') for key in relevant_keys}
    
    # Debug: Ausgabe der extrahierten Metadaten
    logger.debug(f"Extrahierte Metadaten: {filtered_metadata}")
    
    return filtered_metadata

def determine_target_directory(mediathek_dir: Path, metadata: dict) -> Path:
    """
    Bestimmt das Zielverzeichnis basierend auf den Metadaten.
    Struktur: /Mediathek/[Album]/[Jahr]/[Titel (Jahr)]/
    """
    album = metadata.get("Album", "Unbekanntes Album")
    ziel_album_dir = mediathek_dir / album
    ziel_album_dir.mkdir(parents=True, exist_ok=True)
    
    # Bestimme das Jahresverzeichnis
    try:
        creation_date = datetime.strptime(metadata.get('CreationDate'), '%Y:%m:%d')
        jahr = str(creation_date.year)
    except (ValueError, TypeError):
        jahr = 'Unknown'
    
    ziel_jahr_dir = ziel_album_dir / jahr
    ziel_jahr_dir.mkdir(parents=True, exist_ok=True)
    
    # Erstelle ein Unterverzeichnis mit dem Namen der Videodatei
    title = metadata.get('Title', "Unbekannter Titel")
    if not validate_filename(title):
        typer.secho(f"Der Titel '{title}' enthält ungültige Zeichen für das Dateisystem.", fg=typer.colors.RED)
        logger.error(f"Der Titel '{title}' enthält ungültige Zeichen für das Dateisystem.")
        raise typer.Exit(code=1)
    
    base_filename = f"{title} ({jahr})"
    ziel_sub_dir = ziel_jahr_dir / base_filename
    ziel_sub_dir.mkdir(parents=True, exist_ok=True)
    
    logger.debug(f"Ziel-Unterverzeichnis erstellt: '{ziel_sub_dir}'")
    
    return ziel_sub_dir  # Rückgabe des Unterverzeichnisses

def generate_metadata_xml(metadata: dict, base_filename: str) -> ET.Element:
    """
    Erstellt das XML-Element für die NFO-Datei basierend auf den extrahierten Metadaten.
    Anpassung: Jedes Tag wird einzeln als <tag> Element hinzugefügt.
    Das <keywords> Element wird komplett weggelassen.
    """
    movie = ET.Element('movie')
    
    # Plot mit Datum und Beschreibung
    creation_date_str = metadata.get("CreationDate", "")
    if creation_date_str:
        try:
            creation_date = datetime.strptime(creation_date_str, '%Y:%m:%d')
            weekday_number = creation_date.weekday()  # 0=Monday, 6=Sunday
            weekday_german = WEEKDAY_MAP.get(weekday_number, "")
            plot_text = f"{weekday_german} {creation_date.strftime('%d.%m.%Y')}. {metadata.get('Description', '')}"
        except ValueError:
            plot_text = metadata.get('Description', '')
    else:
        plot_text = metadata.get('Description', '')
    
    movie_plot = ET.SubElement(movie, 'plot')
    movie_plot.text = plot_text  # CDATA entfernt
    
    # Weitere Felder gemäß Spezifikation
    ET.SubElement(movie, 'title').text = metadata.get('Title', 'Unbekannt')
    try:
        year = str(datetime.strptime(metadata.get('CreationDate', ''), '%Y:%m:%d').year)
    except ValueError:
        year = 'Unknown'
    ET.SubElement(movie, 'year').text = year
    ET.SubElement(movie, 'sorttitle').text = metadata.get('Title', 'Unbekannt')
    
    try:
        premiered = datetime.strptime(metadata.get('CreationDate', ''), '%Y:%m:%d').strftime('%Y-%m-%d')
    except ValueError:
        premiered = ''
    ET.SubElement(movie, 'premiered').text = premiered
    ET.SubElement(movie, 'releasedate').text = premiered
    ET.SubElement(movie, 'published').text = premiered

    
    # Produzenten als <actor> Tags mit <type>Producer</type>
    producers = [p.strip() for p in metadata.get('Producer', '').split(',') if p.strip()]
    if not producers:
        producers = [p.strip() for p in metadata.get('Author', '').split(',') if p.strip()]
    for producer in producers:
        actor_elem = ET.SubElement(movie, 'actor')
        ET.SubElement(actor_elem, 'name').text = producer
        ET.SubElement(actor_elem, 'type').text = "Producer"
    
    # Direktoren: Mehrere <director> Tags ohne parent <directors>
    directors = [d.strip() for d in metadata.get('Director', '').split(';') if d.strip()]
    for director in directors:
        ET.SubElement(movie, 'director').text = director
    
    # Akteure: Direkt unter <movie> ohne übergeordnetes <actors> Tag
    artists = [a.strip() for a in metadata.get('Artist', '').split(';') if a.strip()]
    for artist in artists:
        if '(' in artist and artist.endswith(')'):
            name, role = artist.split('(', 1)
            name = name.strip()
            role = role[:-1].strip()  # Entfernt das schließende ')'
            actor_elem = ET.SubElement(movie, 'actor')
            ET.SubElement(actor_elem, 'name').text = name
            ET.SubElement(actor_elem, 'role').text = role
            ET.SubElement(actor_elem, 'type').text = "Actor"
        else:
            actor_elem = ET.SubElement(movie, 'actor')
            ET.SubElement(actor_elem, 'name').text = artist
            ET.SubElement(actor_elem, 'type').text = "Actor"
    
    # **Hinzufügen der <tag> Elemente**
    keywords = metadata.get('Keywords', '')
    if keywords:
        # Splitte die Keywords nach Komma
        tags = [tag.strip() for tag in keywords.split(',') if tag.strip()]
        for tag in tags:
            ET.SubElement(movie, 'tag').text = tag
    
    return movie

def write_nfo(nfo_path: Path, xml_element: ET.Element):
    """
    Schreibt das XML-Element in eine NFO-Datei.
    """
    def indent_xml(elem, level=0):
        i = "\n" + level*"  "
        if len(elem):
            if not elem.text or not elem.text.strip():
                elem.text = i + "  "
            for child in elem:
                indent_xml(child, level+1)
                if not child.tail or not child.tail.strip():
                    child.tail = i
            if not child.tail or not child.tail.strip():
                elem.tail = i
        if level and (not elem.tail or not elem.tail.strip()):
            elem.tail = i
    
    indent_xml(xml_element)
    tree = ET.ElementTree(xml_element)
    tree.write(nfo_path, encoding='utf-8', xml_declaration=True)
    typer.secho(f"NFO-Datei wurde erfolgreich erstellt: {nfo_path}", fg=typer.colors.GREEN)
    logger.info(f"NFO-Datei erstellt: {nfo_path}")

def convert_image_to_adobe_rgb(input_file: Path, output_file: Path) -> None:
    """
    Konvertiere ein Bild in das Adobe RGB-Farbprofil und speichere es als JPEG.
    Bei PNG-Bildern zusätzlich eine JPG-Version erstellen.
    
    :param input_file: Pfad zur Eingabedatei (PNG/JPG/JPEG).
    :param output_file: Pfad zur Ausgabedatei (JPEG).
    """
    if input_file.suffix.lower() not in SUPPORTED_IMAGE_FORMATS:
        raise ValueError("Eingabedatei muss eine PNG- oder JPG/JPEG-Datei sein.")
    
    if output_file.suffix.lower() != ".jpg":
        raise ValueError("Ausgabedatei muss eine JPG-Datei sein.")
    
    # Verwende SIPS, um das Format zu ändern und das Farbprofil anzupassen
    command = [
        "sips", "-s", "format", "jpeg", "-m", ADOBE_RGB_PROFILE, str(input_file), "--out", str(output_file)
    ]
    
    try:
        subprocess.run(command, check=True)
        logger.info(f"Erfolgreich konvertiert: {input_file} -> {output_file}")
        typer.secho(f"Bild erfolgreich konvertiert: {input_file} -> {output_file}", fg=typer.colors.GREEN)
    except subprocess.CalledProcessError as e:
        logger.error(f"Fehler beim Konvertieren von {input_file}: {e}")
        typer.secho(f"Fehler beim Konvertieren von {input_file}: {e}", fg=typer.colors.RED)
        raise

def is_file_being_processed(video_file: Path) -> bool:
    """
    Prüft, ob zu der Videodatei entsprechende .sb-* Dateien existieren.
    """
    sb_files = list(video_file.parent.glob(f"{video_file.name}.sb-*"))
    return len(sb_files) > 0

def delete_associated_audio_files(video_file: Path, title: str):
    """
    Löscht alle Audiodateien, die zum Videotitel gehören.
    """
    # Erstelle ein Muster für den Dateinamen
    audio_patterns = [f"{title}*{ext}" for ext in SUPPORTED_AUDIO_FORMATS]
    for pattern in audio_patterns:
        audio_files = list(video_file.parent.glob(pattern))
        for audio_file in audio_files:
            try:
                audio_file.unlink()
                typer.secho(f"Audiodatei '{audio_file}' wurde gelöscht.", fg=typer.colors.GREEN)
                logger.info(f"Audiodatei '{audio_file}' wurde gelöscht.")
            except Exception as e:
                typer.secho(f"Fehler beim Löschen der Audiodatei '{audio_file}': {e}", fg=typer.colors.RED)
                logger.error(f"Fehler beim Löschen der Audiodatei '{audio_file}': {e}")

def delete_associated_files_based_on_title(directory: Path, title: str):
    """
    Löscht alle Dateien im Verzeichnis, die mit dem gegebenen Titel beginnen.
    Dies umfasst PNG, M4A und andere zugehörige Dateiformate.
    
    :param directory: Das Verzeichnis, in dem die Dateien gesucht werden sollen.
    :param title: Der Titel, der als Präfix der zu löschenden Dateien dient.
    """
    # Erstelle ein Muster, das den Titel als Präfix hat
    pattern = f"{title}.*"
    for associated_file in directory.glob(pattern):
        try:
            # Vermeide das Löschen der Videodatei selbst, falls sie noch existiert
            # Hier kannst du spezifische Bedingungen hinzufügen, falls nötig
            if associated_file.is_file():
                associated_file.unlink()
                typer.secho(f"Lösche zugehörige Datei: {associated_file}", fg=typer.colors.GREEN)
                logger.info(f"Lösche zugehörige Datei: {associated_file}")
        except Exception as e:
            typer.secho(f"Fehler beim Löschen der Datei {associated_file}: {e}", fg=typer.colors.RED)
            logger.error(f"Fehler beim Löschen der Datei {associated_file}: {e}")

def integrate_homemovie_to_emby(
    video_file: Path,
    title_image: Optional[Path],
    emby_dir: Path,
    overwrite_existing: bool,
    delete_source_files: bool
) -> None:
    """
    Führt die Integration eines einzelnen Familienfilms in die Emby Mediathek durch.

    :param video_file: Path zur Videodatei.
    :param title_image: Optional Path zum Titelbild.
    :param emby_dir: Path zum Emby Mediathek Verzeichnis.
    :param overwrite_existing: Ob bestehende Dateien überschrieben werden sollen.
    :param delete_source_files: Ob Quelldateien nach erfolgreicher Integration gelöscht werden sollen.
    """
    typer.secho(f"Integriere Familienfilm '{video_file}' in die Emby Mediathek...", fg=typer.colors.BLUE)
    
    # Schritt 1: Extrahiere Metadaten
    try:
        metadata = extract_metadata(video_file)
    except Exception as e:
        typer.secho(f"Fehler beim Extrahieren der Metadaten: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Debug: Ausgabe der Metadaten zur Überprüfung
    logger.debug(f"Überprüfte Metadaten: {metadata}")
    
    # Überprüfe notwendige Metadaten
    if not metadata.get('Title') or not metadata.get('CreationDate'):
        typer.secho("Essentielle Metadaten 'Title' oder 'CreationDate' fehlen.", fg=typer.colors.RED)
        logger.error("Essentielle Metadaten 'Title' oder 'CreationDate' fehlen.")
        raise typer.Exit(code=1)
    
    # Schritt 2: Bestimme das Zielverzeichnis (/Album/Jahr/[Titel (Jahr)]/)
    ziel_sub_dir = determine_target_directory(emby_dir, metadata)
    
    # Bestimme den neuen Dateinamen: Titel (Jahr).ext
    title = metadata.get('Title')
    try:
        creation_date = datetime.strptime(metadata.get('CreationDate'), '%Y:%m:%d')
        jahr = str(creation_date.year)
    except (ValueError, TypeError):
        jahr = 'Unknown'
    base_filename = f"{title} ({jahr})"
    
    # Schritt 3: Überprüfe, ob die Dateien bereits existieren
    existing_files = list(ziel_sub_dir.glob(f"{base_filename}*"))
    if existing_files and overwrite_existing:
        typer.secho(f"Dateien für '{base_filename}' existieren bereits in der Emby Mediathek.", fg=typer.colors.YELLOW)
        typer.secho("Überschreibe bestehende Dateien ohne Nachfrage...", fg=typer.colors.YELLOW)
    elif existing_files:
        typer.secho(f"Dateien für '{base_filename}' existieren bereits in der Emby Mediathek.", fg=typer.colors.YELLOW)
        proceed = typer.confirm(f"Möchtest du die bestehenden Dateien für '{base_filename}' überschreiben?")
        if not proceed:
            typer.secho("Abgebrochen.", fg=typer.colors.RED)
            logger.info(f"Integration abgebrochen durch den Benutzer. Existierende Dateien für: {base_filename}")
            raise typer.Exit(code=1)
        typer.secho(f"Überschreibe bestehende Dateien für '{base_filename}'.", fg=typer.colors.YELLOW)
    
    # Schritt 4: Zielverzeichnis ist bereits erstellt worden (determine_target_directory)
    
    # Schritt 5: Konvertiere das Titelbild, falls angegeben
    konvertierte_bilder = []
    if title_image:
        try:
            # Verwende 'poster.jpg' statt 'base_filename + POSTER_SUFFIX + .jpg'
            poster_filename = "poster.jpg"
            output_poster = ziel_sub_dir / poster_filename
            
            convert_image_to_adobe_rgb(title_image, output_poster)
            konvertierte_bilder.append(output_poster)
            
            # Erstelle 'fanart.jpg' als Kopie von 'poster.jpg'
            fanart_filename = "fanart.jpg"
            output_fanart = ziel_sub_dir / fanart_filename
            shutil.copy2(output_poster, output_fanart)
            konvertierte_bilder.append(output_fanart)
            
            typer.secho(f"Fanart-Bild wurde erstellt: '{output_fanart}'", fg=typer.colors.GREEN)
            logger.info(f"Fanart-Bild erstellt: '{output_fanart}'")
        except Exception as e:
            typer.secho(f"Fehler bei der Bildkonvertierung: {e}", fg=typer.colors.RED)
            logger.error(f"Fehler bei der Bildkonvertierung: {e}")
            raise typer.Exit(code=1)
    
    # Schritt 6: Erstelle die NFO-Datei gemäß Spezifikation
    try:
        xml_element = generate_metadata_xml(metadata, base_filename=base_filename)
        nfo_filename = f"{base_filename}{NFO_SUFFIX}"
        nfo_path = ziel_sub_dir / nfo_filename
        write_nfo(nfo_path, xml_element)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der NFO-Datei: {e}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Erstellen der NFO-Datei: {e}")
        raise typer.Exit(code=1)
    
    # Schritt 7: Kopiere die Videodatei ins Zielverzeichnis mit Benachrichtigung und Dateigröße
    try:
        video_ext = video_file.suffix.lower()
        target_video_path = ziel_sub_dir / f"{base_filename}{video_ext}"
        
        file_size_gb = video_file.stat().st_size / (1024 ** 3)
        typer.secho(f"Beginne mit dem Kopieren von '{video_file}' ({file_size_gb:.2f} GB)...", fg=typer.colors.BLUE)
        logger.info(f"Beginne mit dem Kopieren von '{video_file}' ({file_size_gb:.2f} GB)...")
        
        shutil.copy2(video_file, target_video_path)
        typer.secho(f"Videodatei wurde nach '{target_video_path}' kopiert.", fg=typer.colors.GREEN)
        logger.info(f"Videodatei '{video_file}' wurde nach '{target_video_path}' kopiert.")
    except Exception as e:
        typer.secho(f"Fehler beim Kopieren der Videodatei: {e}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Kopieren der Videodatei: {e}")
        raise typer.Exit(code=1)
    
    # Schritt 8: Optional, lösche die Quelldateien
    if delete_source_files:
        try:
            video_file.unlink()
            typer.secho(f"Videodatei '{video_file}' wurde gelöscht.", fg=typer.colors.GREEN)
            logger.info(f"Videodatei '{video_file}' wurde gelöscht.")
            
            # Lösche die zugehörigen Audiodateien
            delete_associated_audio_files(video_file, title)
            
            # Lösche alle anderen zugehörigen Dateien basierend auf dem Titel
            delete_associated_files_based_on_title(video_file.parent, title)
            
            if title_image:
                title_image.unlink()
                typer.secho(f"Titelbild '{title_image}' wurde gelöscht.", fg=typer.colors.GREEN)
                logger.info(f"Titelbild '{title_image}' wurde gelöscht.")
        except Exception as e:
            typer.secho(f"Fehler beim Löschen der Quelldateien: {e}", fg=typer.colors.RED)
            logger.error(f"Fehler beim Löschen der Quelldateien: {e}")
    else:
        if konvertierte_bilder or video_file.exists() or title_image:
            proceed = typer.confirm("Möchtest du die Quelldateien nach erfolgreicher Integration löschen?")
            if proceed:
                try:
                    video_file.unlink()
                    typer.secho(f"Videodatei '{video_file}' wurde gelöscht.", fg=typer.colors.GREEN)
                    logger.info(f"Videodatei '{video_file}' wurde gelöscht.")
                    
                    # Lösche die zugehörigen Audiodateien
                    delete_associated_audio_files(video_file, title)
                    
                    # Lösche alle anderen zugehörigen Dateien basierend auf dem Titel
                    delete_associated_files_based_on_title(video_file.parent, title)
                    
                except Exception as e:
                    typer.secho(f"Fehler beim Löschen der Videodatei: {e}", fg=typer.colors.RED)
                    logger.error(f"Fehler beim Löschen der Videodatei: {e}")
                
                if title_image:
                    try:
                        title_image.unlink()
                        typer.secho(f"Titelbild '{title_image}' wurde gelöscht.", fg=typer.colors.GREEN)
                        logger.info(f"Titelbild '{title_image}' wurde gelöscht.")
                    except Exception as e:
                        typer.secho(f"Fehler beim Löschen des Titelbildes: {e}", fg=typer.colors.RED)
                        logger.error(f"Fehler beim Löschen des Titelbildes: {e}")
            else:
                typer.secho("Quelldateien wurden nicht gelöscht.", fg=typer.colors.YELLOW)
    
    typer.secho("Familienfilm-Integration erfolgreich abgeschlossen.", fg=typer.colors.GREEN)
    logger.info(f"Familienfilm '{video_file}' erfolgreich integriert in '{ziel_sub_dir}'.")

def integrate_homemovies(
    search_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Das Verzeichnis, in dem nach Mediendateien gesucht werden soll."
    ),
    additional_dir: Optional[Path] = typer.Option(
        None,
        "--additional-dir",
        "-ad",
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Zusätzliches Verzeichnis zur Suche nach Mediendateien."
    ),
    mediathek_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        writable=True,
        readable=True,
        help="Das Hauptverzeichnis der Emby-Mediathek, in das die Familienfilme integriert werden sollen."
    ),
    overwrite_existing: bool = typer.Option(
        False,
        "--overwrite-existing",
        help="Überschreibt bestehende Dateien ohne Rückfrage, wenn diese existieren."
    ),
    delete_source_files: bool = typer.Option(
        False,
        "--delete-source-files",
        help="Löscht die Quelldateien nach erfolgreicher Integration."
    )
):
    """
    Integriert mehrere Familienfilme aus einem Verzeichnis (und optional einem weiteren) in die Emby-Mediathek.
    """
    typer.secho(f"Integriere mehrere Familienfilme aus '{search_dir}' in die Mediathek...", fg=typer.colors.BLUE)
    
    directories = [search_dir]
    if additional_dir:
        directories.append(additional_dir)
        typer.secho(f"Zusätzliches Verzeichnis hinzugefügt: '{additional_dir}'", fg=typer.colors.BLUE)
    
    # Schritt 1: Sammeln aller unterstützten Videodateien ohne ProRes
    media_files: List[Path] = []
    for dir_path in directories:
        typer.secho(f"Durchsuche Verzeichnis: '{dir_path}'", fg=typer.colors.BLUE)
        for file_path in dir_path.rglob('*'):
            if file_path.is_file() and file_path.suffix.lower() in ['.mov', '.mp4', '.m4v']:
                # Überprüfe, ob die Datei gerade verarbeitet wird
                if is_file_being_processed(file_path):
                    typer.secho(f"Überspringe Datei, die gerade verarbeitet wird: '{file_path}'", fg=typer.colors.YELLOW)
                    logger.info(f"Überspringe Datei, die gerade verarbeitet wird: '{file_path}'")
                    continue  # Fahre mit der nächsten Datei fort
                
                try:
                    metadata = extract_metadata(file_path)
                    # Überprüfe, ob der Codec nicht ProRes ist
                    video_codec = metadata.get('VideoCodec', '').strip()
                    if video_codec not in ['Apple ProRes 422', 'Apple ProRes 422 HQ', 'Apple ProRes 4444', 'Apple ProRes 4444 XQ']:
                        media_files.append(file_path)
                        logger.debug(f"Hinzufügen von '{file_path}' zur Liste der Mediendateien.")
                    else:
                        typer.secho(f"Überspringe ProRes-Datei: '{file_path}'", fg=typer.colors.YELLOW)
                        logger.info(f"Überspringe ProRes-Datei: '{file_path}'")
                except Exception as e:
                    typer.secho(f"Fehler beim Verarbeiten von '{file_path}': {e}", fg=typer.colors.RED)
                    logger.error(f"Fehler beim Verarbeiten von '{file_path}': {e}")
                    continue
    
    if not media_files:
        typer.secho("Keine unterstützten Mediendateien gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()
    
    # Schritt 2: Gruppierung der Mediendateien nach Titel
    groups: Dict[str, Dict[str, List[Path]]] = {}
    for file_path in media_files:
        try:
            metadata = extract_metadata(file_path)
            title = metadata.get('Title') or metadata.get('DisplayName') or file_path.stem
            # Hier keine Sanitization mehr, aber Validierung wird später durchgeführt
            if not title:
                typer.secho(f"Keine Titel-Metadaten in '{file_path}' gefunden. Datei wird übersprungen.", fg=typer.colors.YELLOW)
                logger.warning(f"Keine Titel-Metadaten in '{file_path}' gefunden. Datei wird übersprungen.")
                continue
            if title not in groups:
                groups[title] = {
                    'videos': [],
                    'images': []
                }
            groups[title]['videos'].append(file_path)
            logger.debug(f"Datei '{file_path}' zur Gruppe '{title}' hinzugefügt.")
        except Exception as e:
            typer.secho(f"Fehler beim Gruppieren von '{file_path}': {e}", fg=typer.colors.RED)
            logger.error(f"Fehler beim Gruppieren von '{file_path}': {e}")
            continue
    
    if not groups:
        typer.secho("Keine Gruppen von Mediendateien mit gemeinsamen Titeln gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()
    
    # Schritt 3: Suche nach zugehörigen Titelbildern und Auswahl der besten Videodatei
    for title, files in groups.items():
        # Bevorzugt das größte Video als die beste Qualität
        if len(files['videos']) > 1:
            sorted_videos = sorted(files['videos'], key=lambda f: f.stat().st_size, reverse=True)
            best_video = sorted_videos[0]
            groups[title]['videos'] = [best_video]
            typer.secho(f"Mehrere Videos gefunden für '{title}'. Wähle die größte Datei: '{best_video}'", fg=typer.colors.GREEN)
            logger.info(f"Mehrere Videos gefunden für '{title}'. Wähle die größte Datei: '{best_video}'")
        else:
            best_video = files['videos'][0]
        
        # Suche nach zugehörigem Titelbild, bevorzugt PNG über JPG
        image_found = False
        for ext in ['.png', '.jpg', '.jpeg']:
            image_file = Path(best_video.parent) / f"{title}{ext}"
            if image_file.exists() and image_file.suffix.lower() in SUPPORTED_IMAGE_FORMATS:
                groups[title]['images'].append(image_file)
                typer.secho(f"Gefundenes Titelbild für '{title}': '{image_file}'", fg=typer.colors.GREEN)
                logger.info(f"Gefundenes Titelbild für '{title}': '{image_file}'")
                image_found = True
                break  # Bevorzugt das erste gefundene Bild in der Reihenfolge PNG, JPG, JPEG
        if not image_found:
            typer.secho(f"Kein passendes Titelbild für '{title}' gefunden.", fg=typer.colors.YELLOW)
            logger.warning(f"Kein passendes Titelbild für '{title}' gefunden.")
    
    # Schritt 4: Integration der Mediensets
    typer.secho("\nBeginne mit der Integration der Mediensets...", fg=typer.colors.BLUE)
    for title, files in groups.items():
        video_file = files['videos'][0]
        title_image = files['images'][0] if files['images'] else None
        typer.secho(f"\nIntegriere Medienset '{title}'...", fg=typer.colors.CYAN)
        try:
            integrate_homemovie_to_emby(
                video_file=video_file,
                title_image=title_image,
                emby_dir=mediathek_dir,
                overwrite_existing=overwrite_existing,
                delete_source_files=delete_source_files  # Übergebe das Flag
            )
        except typer.Exit as e:
            if e.code != 0:
                typer.secho(f"Fehler beim Integrieren von '{title}': {e}", fg=typer.colors.RED)
                logger.error(f"Fehler beim Integrieren von '{title}': {e}")
            continue  # Fahre mit dem nächsten Medienset fort
        except Exception as e:
            typer.secho(f"Unbekannter Fehler beim Integrieren von '{title}': {e}", fg=typer.colors.RED)
            logger.error(f"Unbekannter Fehler beim Integrieren von '{title}': {e}")
            continue  # Fahre mit dem nächsten Medienset fort
    
    typer.secho("\nIntegration aller Mediensets abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Integration aller Mediensets abgeschlossen.")

if __name__ == "__main__":
    app()