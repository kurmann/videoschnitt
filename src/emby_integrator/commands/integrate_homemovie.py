# src/emby_integrator/commands/integrate_homemovie.py

import typer
from pathlib import Path
from typing import Optional
import shutil
from datetime import datetime
import subprocess
import json
import xml.etree.ElementTree as ET
import logging

app = typer.Typer()

# Konfiguriere das Logging
logging.basicConfig(
    filename='integrate_homemovie.log',
    filemode='a',
    format='%(asctime)s - %(levelname)s - %(message)s',
    level=logging.DEBUG  # Setze auf DEBUG für detailliertes Logging
)
logger = logging.getLogger(__name__)

# Modulvariablen
POSTER_SUFFIX = "-poster"
NFO_SUFFIX = ".nfo"

# Konstante für Adobe RGB Profil
ADOBE_RGB_PROFILE = "/System/Library/ColorSync/Profiles/AdobeRGB1998.icc"
SUPPORTED_IMAGE_FORMATS = [".jpg", ".jpeg", ".png"]

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

def sanitize_filename(filename: str) -> str:
    """
    Entfernt ungültige Zeichen aus dem Dateinamen.
    """
    return "".join(c for c in filename if c.isalnum() or c in " .-_()").rstrip()

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
        "CreationDate"
    ]
    filtered_metadata = {key: metadata.get(key, '') for key in relevant_keys}
    
    # Debug: Ausgabe der extrahierten Metadaten
    logger.debug(f"Extrahierte Metadaten: {filtered_metadata}")
    
    return filtered_metadata

def determine_target_directory(mediathek_dir: Path, metadata: dict) -> Path:
    """
    Bestimmt das Zielverzeichnis basierend auf den Metadaten.
    Struktur: /Mediathek/[Album]/[Jahr]/
    """
    album = metadata.get("Album", "Unbekanntes Album")
    sanitized_album = sanitize_filename(album)
    ziel_album_dir = mediathek_dir / sanitized_album
    ziel_album_dir.mkdir(parents=True, exist_ok=True)
    
    # Bestimme das Jahresverzeichnis
    try:
        creation_date = datetime.strptime(metadata.get('CreationDate'), '%Y:%m:%d')
        jahr = str(creation_date.year)
    except (ValueError, TypeError):
        jahr = 'Unknown'
    
    ziel_jahr_dir = ziel_album_dir / jahr
    ziel_jahr_dir.mkdir(parents=True, exist_ok=True)
    return ziel_jahr_dir

def generate_metadata_xml(metadata: dict, base_filename: str) -> ET.Element:
    """
    Erstellt das XML-Element für die NFO-Datei basierend auf den extrahierten Metadaten.
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
    
    # Keywords
    keywords = metadata.get('Keywords', '')
    ET.SubElement(movie, 'keywords').text = keywords
    
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
    
    # Tags
    for tag in keywords.split(';'):
        tag = tag.strip()
        if tag:
            ET.SubElement(movie, 'tag').text = tag
    
    # Entferne die Sets-Logik vollständig
    # Alle bisherigen Referenzen zu 'Category' und 'set' wurden entfernt
    
    # Entferne das <dateadded> Tag
    # ET.SubElement(movie, 'dateadded').text = dateadded  # Entfernt
    
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
        "sips", "-s", "format", "jpeg", "-m", ADOBE_RGB_PROFILE, str(input_file), "--out", str(output_file)  # Korrigiert: output_image zu output_file
    ]
    
    try:
        subprocess.run(command, check=True)
        logger.info(f"Erfolgreich konvertiert: {input_file} -> {output_file}")
        typer.secho(f"Bild erfolgreich konvertiert: {input_file} -> {output_file}", fg=typer.colors.GREEN)
    except subprocess.CalledProcessError as e:
        logger.error(f"Fehler beim Konvertieren von {input_file}: {e}")
        typer.secho(f"Fehler beim Konvertieren von {input_file}: {e}", fg=typer.colors.RED)
        raise

@app.command()
def integrate_homemovie(
    video_file: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=True,
        dir_okay=False,
        readable=True,
        help="Der Pfad zur Videodatei, die in die Mediathek integriert werden soll."
    ),
    title_image: Optional[Path] = typer.Option(
        None,
        "--title-image",
        "-i",
        exists=True,
        file_okay=True,
        dir_okay=False,
        readable=True,
        help="Der Pfad zum optionalen Titelbild."
    ),
    mediathek_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        writable=True,
        readable=True,
        help="Das Hauptverzeichnis der Emby-Mediathek, in das der Familienfilm integriert werden soll."
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
    Integriert einen Familienfilm in die Emby-Mediathek. Kopiert die Videodatei und optional das Titelbild in das passende Verzeichnis und erstellt die erforderlichen Metadaten.
    """
    typer.secho(f"Integriere Familienfilm '{video_file}' in die Mediathek...", fg=typer.colors.BLUE)
    
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
    
    # Schritt 2: Bestimme das Zielverzeichnis (/Album/Jahr/)
    ziel_jahr_dir = determine_target_directory(mediathek_dir, metadata)
    
    # Bestimme den neuen Dateinamen: Titel (Jahr).ext
    sanitized_title = sanitize_filename(metadata.get('Title'))
    try:
        creation_date = datetime.strptime(metadata.get('CreationDate'), '%Y:%m:%d')
        jahr = str(creation_date.year)
    except (ValueError, TypeError):
        jahr = 'Unknown'
    
    base_filename = f"{sanitized_title} ({jahr})"
    
    # Schritt 3: Überprüfe, ob die Dateien bereits existieren
    existing_files = list(ziel_jahr_dir.glob(f"{base_filename}*"))
    if existing_files and overwrite_existing:
        typer.secho(f"Dateien für '{base_filename}' existieren bereits in der Mediathek.", fg=typer.colors.YELLOW)
        typer.secho("Überschreibe bestehende Dateien ohne Nachfrage...", fg=typer.colors.YELLOW)
    elif existing_files:
        typer.secho(f"Dateien für '{base_filename}' existieren bereits in der Mediathek.", fg=typer.colors.YELLOW)
        proceed = typer.confirm(f"Möchtest du die bestehenden Dateien für '{base_filename}' überschreiben?")
        if not proceed:
            typer.secho("Abgebrochen.", fg=typer.colors.RED)
            logger.info(f"Integration abgebrochen durch den Benutzer. Existierende Dateien für: {base_filename}")
            raise typer.Exit(code=1)
        typer.secho(f"Überschreibe bestehende Dateien für '{base_filename}'.", fg=typer.colors.YELLOW)
    
    # Schritt 4: Erstelle das Zielverzeichnis, falls es noch nicht existiert
    try:
        ziel_jahr_dir.mkdir(parents=True, exist_ok=True)
        typer.secho(f"Zielverzeichnis '{ziel_jahr_dir}' wurde sichergestellt.", fg=typer.colors.GREEN)
        logger.info(f"Zielverzeichnis '{ziel_jahr_dir}' wurde sichergestellt.")
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen des Zielverzeichnisses: {e}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Erstellen des Zielverzeichnisses: {e}")
        raise typer.Exit(code=1)
    
    # Schritt 5: Konvertiere das Titelbild, falls angegeben
    konvertierte_bilder = []
    if title_image:
        try:
            # Bestimme den neuen Dateinamen: Titel (Jahr)-poster.jpg
            poster_filename = f"{base_filename}{POSTER_SUFFIX}.jpg"
            output_image = ziel_jahr_dir / poster_filename
            
            convert_image_to_adobe_rgb(title_image, output_image)
            konvertierte_bilder.append(output_image)
            
            # Falls das Eingabebild ein PNG ist, bereits eine JPG-Version erstellt
            if title_image.suffix.lower() == ".png":
                # Keine zusätzliche Aktion erforderlich, da die Funktion bereits eine JPG erstellt
                pass
        except Exception as e:
            typer.secho(f"Fehler bei der Bildkonvertierung: {e}", fg=typer.colors.RED)
            logger.error(f"Fehler bei der Bildkonvertierung: {e}")
            raise typer.Exit(code=1)
    
    # Schritt 6: Erstelle die NFO-Datei gemäß Spezifikation
    try:
        xml_element = generate_metadata_xml(metadata, base_filename=base_filename)
        nfo_filename = f"{base_filename}{NFO_SUFFIX}"
        nfo_path = ziel_jahr_dir / nfo_filename
        write_nfo(nfo_path, xml_element)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der NFO-Datei: {e}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Erstellen der NFO-Datei: {e}")
        raise typer.Exit(code=1)
    
    # Schritt 7: Kopiere die Videodatei ins Zielverzeichnis
    try:
        video_ext = video_file.suffix.lower()
        target_video_path = ziel_jahr_dir / f"{base_filename}{video_ext}"
        
        shutil.copy2(video_file, target_video_path)
        typer.secho(f"Videodatei wurde nach '{target_video_path}' kopiert.", fg=typer.colors.GREEN)
        logger.info(f"Videodatei '{video_file}' wurde nach '{target_video_path}' kopiert.")
    except Exception as e:
        typer.secho(f"Fehler beim Kopieren der Videodatei: {e}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Kopieren der Videodatei: {e}")
        raise typer.Exit(code=1)
    
    # Schritt 8: Kopiere die konvertierten Titelbilder ins Zielverzeichnis
    for converted_image in konvertierte_bilder:
        try:
            # Da die Bilder bereits im Zielverzeichnis erstellt und benannt wurden, ist keine weitere Aktion erforderlich
            pass
        except Exception as e:
            typer.secho(f"Fehler beim Kopieren des Titelbildes: {e}", fg=typer.colors.RED)
            logger.error(f"Fehler beim Kopieren des Titelbildes: {e}")
            raise typer.Exit(code=1)
    
    # Schritt 9: Optional, lösche die Quelldateien
    if delete_source_files:
        try:
            video_file.unlink()
            typer.secho(f"Videodatei '{video_file}' wurde gelöscht.", fg=typer.colors.GREEN)
            logger.info(f"Videodatei '{video_file}' wurde gelöscht.")
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
        if konvertierte_bilder or video_file.exists() or title_image:
            proceed = typer.confirm("Möchtest du die Quelldateien nach erfolgreicher Integration löschen?")
            if proceed:
                try:
                    video_file.unlink()
                    typer.secho(f"Videodatei '{video_file}' wurde gelöscht.", fg=typer.colors.GREEN)
                    logger.info(f"Videodatei '{video_file}' wurde gelöscht.")
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
    logger.info(f"Familienfilm '{video_file}' erfolgreich integriert in '{ziel_jahr_dir}'.")
    raise typer.Exit(code=0)
