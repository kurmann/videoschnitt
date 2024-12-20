# src/emby_integrator/commands/homemovie_integrator.py

import typer
from pathlib import Path
from typing import Dict, List, Optional
import shutil
from datetime import datetime
import subprocess
import json
import xml.etree.ElementTree as ET
from emby_integrator.config_manager import load_config
import xml.dom.minidom  # Hinzugefügt für bessere XML-Formatierung

app = typer.Typer()

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
        typer.secho(f"Der Dateiname '{filename}' enthält ungültige Zeichen.", fg=typer.colors.RED)
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
        raise typer.Exit(code=1)
    
    try:
        metadata_list = json.loads(result.stdout)
        if not metadata_list:
            raise ValueError("Keine Metadaten gefunden.")
        metadata = metadata_list[0]
    except json.JSONDecodeError:
        typer.secho("Ungültige JSON-Ausgabe von ExifTool.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Filtern der relevanten Metadaten
    relevant_keys = [
        "Title", "Description", "Author", "Keywords", "Producer",
        "Director", "Album", "CreateDate", "Artist", "DisplayName",
        "CreationDate", "VideoCodec"
    ]
    filtered_metadata = {key: metadata.get(key, '') for key in relevant_keys}
    
    return filtered_metadata

def determine_target_directory(mediathek_dir: Path, metadata: dict, config: Dict) -> Path:
    """
    Bestimmt das Zielverzeichnis basierend auf den Metadaten und der Konfiguration.
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
        typer.secho(f"Unbekanntes Jahr für 'CreationDate': {metadata.get('CreationDate')}", fg=typer.colors.YELLOW)
    
    ziel_jahr_dir = ziel_album_dir / jahr
    ziel_jahr_dir.mkdir(parents=True, exist_ok=True)
    typer.secho(f"Jahr-Verzeichnis erstellt: {ziel_jahr_dir}", fg=typer.colors.GREEN)
    
    # Erstelle ein Unterverzeichnis mit dem Namen der Videodatei
    title = metadata.get('Title', "Unbekannter Titel")
    if not validate_filename(title):
        typer.secho(f"Der Titel '{title}' enthält ungültige Zeichen für das Dateisystem.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    base_filename = f"{title} ({jahr})"
    ziel_sub_dir = ziel_jahr_dir / base_filename
    ziel_sub_dir.mkdir(parents=True, exist_ok=True)
    typer.secho(f"Ziel-Unterverzeichnis erstellt: {ziel_sub_dir}", fg=typer.colors.GREEN)
    
    return ziel_sub_dir  # Rückgabe des Unterverzeichnisses

def generate_metadata_xml(metadata: dict, base_filename: str, config: Dict) -> ET.Element:
    """
    Erstellt das XML-Element für die NFO-Datei basierend auf den extrahierten Metadaten.
    Anpassungen:
    - Entferne die Hinzufügung von Gruppierungstags.
    - Erstelle Produzenten als zusätzliche Direktoren, sofern sie sich von den vorhandenen Direktoren unterscheiden.
    - Verarbeite Gruppennamen in den Actors und füge ihre Mitglieder als einzelne Actors hinzu.
    - Weiche das <role>-Tag aus, wenn keine Default-Rolle vorhanden ist.
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
            typer.secho(f"Fehler beim Parsen des 'CreationDate': {creation_date_str}", fg=typer.colors.YELLOW)
    else:
        plot_text = metadata.get('Description', '')
        typer.secho("Kein 'CreationDate' in den Metadaten gefunden.", fg=typer.colors.YELLOW)
    
    movie_plot = ET.SubElement(movie, 'plot')
    movie_plot.text = plot_text
    
    # Weitere Felder gemäß Spezifikation
    ET.SubElement(movie, 'title').text = metadata.get('Title', 'Unbekannt')
    
    try:
        year = str(datetime.strptime(metadata.get('CreationDate', ''), '%Y:%m:%d').year)
    except ValueError:
        year = 'Unknown'
        typer.secho(f"Unbekanntes Jahr für 'CreationDate': {metadata.get('CreationDate')}", fg=typer.colors.YELLOW)
    ET.SubElement(movie, 'year').text = year
    typer.secho(f"Jahr hinzugefügt: {year}", fg=typer.colors.GREEN)
    
    ET.SubElement(movie, 'sorttitle').text = metadata.get('Title', 'Unbekannt')
    
    try:
        premiered = datetime.strptime(metadata.get('CreationDate', ''), '%Y:%m:%d').strftime('%Y-%m-%d')
    except ValueError:
        premiered = ''
        typer.secho(f"Ungültiges Datum für 'premiered': {metadata.get('CreationDate')}", fg=typer.colors.YELLOW)
    ET.SubElement(movie, 'premiered').text = premiered
    ET.SubElement(movie, 'releasedate').text = premiered
    ET.SubElement(movie, 'published').text = premiered
    typer.secho(f"Premiere, ReleaseDate und Published hinzugefügt: {premiered}", fg=typer.colors.GREEN)
    
    # Direktoren: Mehrere <director> Tags ohne parent <directors>
    directors = [d.strip() for d in metadata.get('Director', '').split(';') if d.strip()]
    for director in directors:
        ET.SubElement(movie, 'director').text = director
        typer.secho(f"Hinzufügen von Director: {director}", fg=typer.colors.GREEN)
    
    # Produzenten als zusätzliche Direktoren, sofern sie sich von den bestehenden Direktoren unterscheiden
    producers = [p.strip() for p in metadata.get('Producer', '').split(',') if p.strip()]
    if not producers:
        producers = [p.strip() for p in metadata.get('Author', '').split(',') if p.strip()]
    for producer in producers:
        if producer not in directors:
            ET.SubElement(movie, 'director').text = producer
            typer.secho(f"Hinzufügen von zusätzlichem Director (Producer): {producer}", fg=typer.colors.GREEN)
        else:
            typer.secho(f"Producer '{producer}' ist bereits ein Director und wird nicht erneut hinzugefügt.", fg=typer.colors.YELLOW)
    
    # Akteure: Direkt unter <movie> mit <role> Tag
    artists = [a.strip() for a in metadata.get('Artist', '').split(';') if a.strip()]
    for artist in artists:
        if artist in config.get('groups', {}):
            # Der Artist ist eine Gruppe, füge alle Gruppenmitglieder als Akteure hinzu
            group_members = config['groups'][artist]
            for member_full_name in group_members:
                actor_elem = ET.SubElement(movie, 'actor')
                ET.SubElement(actor_elem, 'name').text = member_full_name
                # Hole die Rolle aus default_roles, wenn vorhanden
                role = config.get('default_roles', {}).get(member_full_name)
                if role:
                    ET.SubElement(actor_elem, 'role').text = role
                ET.SubElement(actor_elem, 'type').text = "Actor"
        else:
            # Der Artist ist ein einzelner Actor, verarbeite wie gewohnt
            if '(' in artist and artist.endswith(')'):
                # Extrahiere den Namen und die Rolle aus den Klammern
                name, role = artist.split('(', 1)
                name = name.strip()
                role = role[:-1].strip()  # Entfernt das schließende ')'
            else:
                name = artist
                # **Anpassung: Zuweisung der Rolle basierend auf 'full_name'**
                name_parts = name.split()
                if len(name_parts) == 1:
                    # Nur Vorname vorhanden, erweitere zu vollständigem Namen
                    full_name = config.get('name_mappings', {}).get(name, name)
                    if full_name != name:
                        typer.secho(f"Namenszusammenführung: {name} -> {full_name}", fg=typer.colors.YELLOW)
                    else:
                        typer.secho(f"Kein Mapping gefunden für Namen: {name}", fg=typer.colors.YELLOW)
                else:
                    full_name = name  # Vollständiger Name bereits vorhanden
                    typer.secho(f"Vollständiger Name verwendet: {full_name}", fg=typer.colors.GREEN)
                
                # Hole die Rolle aus default_roles, wenn vorhanden
                role = config.get('default_roles', {}).get(full_name)
                if role:
                    typer.secho(f"Artist '{full_name}' hat die Rolle: {role}", fg=typer.colors.GREEN)
                else:
                    typer.secho(f"Keine Rolle für Artist '{full_name}' gefunden.", fg=typer.colors.YELLOW)
            
            actor_elem = ET.SubElement(movie, 'actor')
            ET.SubElement(actor_elem, 'name').text = full_name
            if 'role' in locals() and role:
                ET.SubElement(actor_elem, 'role').text = role
            ET.SubElement(actor_elem, 'type').text = "Actor"
    
    # **Hinzufügen der <tag> Elemente für Keywords**
    keywords = metadata.get('Keywords', '')
    if keywords:
        # Splitte die Keywords nach Komma
        tags = [tag.strip() for tag in keywords.split(',') if tag.strip()]
        for tag in tags:
            ET.SubElement(movie, 'tag').text = tag
    
    return movie

def write_nfo(nfo_path: Path, xml_element: ET.Element):
    """
    Schreibt das XML-Element in eine NFO-Datei mit korrektem Einrücken.
    """
    # Konvertiere das ElementTree-Element in einen String
    rough_string = ET.tostring(xml_element, 'utf-8')
    
    # Verwende minidom, um das XML zu parsen und zu formatieren
    reparsed = xml.dom.minidom.parseString(rough_string)
    pretty_xml = reparsed.toprettyxml(indent="  ")
    
    # Schreibe das formatierten XML in die NFO-Datei
    with nfo_path.open('w', encoding='utf-8') as f:
        f.write(pretty_xml)
    
    typer.secho(f"NFO-Datei wurde erfolgreich erstellt: {nfo_path}", fg=typer.colors.GREEN)

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
        typer.secho(f"Bild erfolgreich konvertiert: {input_file} -> {output_file}", fg=typer.colors.GREEN)
    except subprocess.CalledProcessError as e:
        typer.secho(f"❌ Fehler beim Konvertieren von {input_file}: {e}", fg=typer.colors.RED)
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
            except Exception as e:
                typer.secho(f"Fehler beim Löschen der Audiodatei '{audio_file}': {e}", fg=typer.colors.RED)

def delete_associated_files_based_on_title(directory: Path, title: str, video_extensions: List[str] = ['.mov', '.mp4', '.m4v']):
    """
    Löscht alle Dateien im Verzeichnis, die mit dem gegebenen Titel beginnen.
    Dies umfasst PNG, M4A und andere zugehörige Dateiformate, aber schließt die Hauptvideodatei aus.
    
    :param directory: Das Verzeichnis, in dem die Dateien gesucht werden sollen.
    :param title: Der Titel, der als Präfix der zu löschenden Dateien dient.
    :param video_extensions: Liste der Videodateierweiterungen, die ausgeschlossen werden sollen.
    """
    # Erstelle ein Muster, das den Titel als Präfix hat
    pattern = f"{title}.*"
    for associated_file in directory.glob(pattern):
        try:
            # Vermeide das Löschen der Videodatei selbst, falls sie noch existiert
            if associated_file.is_file() and not any(associated_file.name.endswith(ext) for ext in video_extensions):
                associated_file.unlink()
                typer.secho(f"Lösche zugehörige Datei: {associated_file}", fg=typer.colors.GREEN)
        except Exception as e:
            typer.secho(f"Fehler beim Löschen der Datei {associated_file}: {e}", fg=typer.colors.RED)
            
def is_hidden(file_path: Path) -> bool:
    """
    Prüft, ob eine Datei versteckt ist.
    Dies funktioniert auf macOS für Dateien, die mit einem Punkt beginnen.
    """
    return file_path.name.startswith(".")

def integrate_homemovie_to_emby(
    video_file: Path,
    title_image: Optional[Path],
    emby_dir: Path,
    overwrite_existing: bool,
    delete_source_files: bool,
    config: Dict
) -> None:
    """
    Führt die Integration eines einzelnen Familienfilms in die Emby Mediathek durch.

    :param video_file: Path zur Videodatei.
    :param title_image: Optional Path zum Titelbild.
    :param emby_dir: Path zum Emby Mediathek Verzeichnis.
    :param overwrite_existing: Ob bestehende Dateien überschrieben werden sollen.
    :param delete_source_files: Ob Quelldateien nach erfolgreicher Integration gelöscht werden sollen.
    :param config: Geladene Konfigurationsdaten aus config.toml.
    """
    typer.secho(f"Integriere Familienfilm '{video_file}' in die Emby Mediathek...", fg=typer.colors.BLUE)
    
    # Schritt 1: Extrahiere Metadaten
    try:
        metadata = extract_metadata(video_file)
    except Exception as e:
        typer.secho(f"Fehler beim Extrahieren der Metadaten: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Überprüfe notwendige Metadaten
    if not metadata.get('Title') or not metadata.get('CreationDate'):
        typer.secho("Essentielle Metadaten 'Title' oder 'CreationDate' fehlen.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Schritt 2: Bestimme das Zielverzeichnis (/Album/Jahr/[Titel (Jahr)]/)
    ziel_sub_dir = determine_target_directory(emby_dir, metadata, config)
    
    # Bestimme den neuen Dateinamen: Titel (Jahr).ext
    title = metadata.get('Title')
    try:
        creation_date = datetime.strptime(metadata.get('CreationDate'), '%Y:%m:%d')
        jahr = str(creation_date.year)
    except (ValueError, TypeError):
        jahr = 'Unknown'
        typer.secho(f"Unbekanntes Jahr für 'CreationDate': {metadata.get('CreationDate')}", fg=typer.colors.YELLOW)
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
            typer.secho(f"Poster-Bild wurde konvertiert: '{output_poster}'", fg=typer.colors.GREEN)
            
            # Erstelle 'fanart.jpg' als Kopie von 'poster.jpg'
            fanart_filename = "fanart.jpg"
            output_fanart = ziel_sub_dir / fanart_filename
            shutil.copy2(output_poster, output_fanart)
            konvertierte_bilder.append(output_fanart)
            typer.secho(f"Fanart-Bild wurde erstellt: '{output_fanart}'", fg=typer.colors.GREEN)
            
            typer.secho(f"Fanart-Bild wurde erstellt: '{output_fanart}'", fg=typer.colors.GREEN)
        except Exception as e:
            typer.secho(f"Fehler bei der Bildkonvertierung: {e}", fg=typer.colors.RED)
            raise typer.Exit(code=1)
    
    # Schritt 6: Erstelle die NFO-Datei gemäß Spezifikation
    try:
        xml_element = generate_metadata_xml(metadata, base_filename=base_filename, config=config)
        nfo_filename = f"{base_filename}{NFO_SUFFIX}"
        nfo_path = ziel_sub_dir / nfo_filename
        write_nfo(nfo_path, xml_element)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der NFO-Datei: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Schritt 7: Kopiere die Videodatei ins Zielverzeichnis mit Benachrichtigung und Dateigröße
    try:
        video_ext = video_file.suffix.lower()
        target_video_path = ziel_sub_dir / f"{base_filename}{video_ext}"
        
        file_size_gb = video_file.stat().st_size / (1024 ** 3)
        typer.secho(f"Beginne mit dem Kopieren von '{video_file}' ({file_size_gb:.2f} GB)...", fg=typer.colors.BLUE)
        
        shutil.copy2(video_file, target_video_path)
        typer.secho(f"Videodatei wurde nach '{target_video_path}' kopiert.", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Kopieren der Videodatei: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Schritt 8: Optional, lösche die Quelldateien
    if delete_source_files:
        try:
            video_file.unlink()
            typer.secho(f"Videodatei '{video_file}' wurde gelöscht.", fg=typer.colors.GREEN)
            
            # Lösche die zugehörigen Audiodateien
            delete_associated_audio_files(video_file, title)
            
            # Lösche alle anderen zugehörigen Dateien basierend auf dem Titel
            delete_associated_files_based_on_title(video_file.parent, title)
            
            if title_image:
                title_image.unlink()
                typer.secho(f"Titelbild '{title_image}' wurde gelöscht.", fg=typer.colors.GREEN)
        except Exception as e:
            typer.secho(f"Fehler beim Löschen der Quelldateien: {e}", fg=typer.colors.RED)
    else:
        if konvertierte_bilder or video_file.exists() or title_image:
            proceed = typer.confirm("Möchtest du die Quelldateien nach erfolgreicher Integration löschen?")
            if proceed:
                try:
                    video_file.unlink()
                    typer.secho(f"Videodatei '{video_file}' wurde gelöscht.", fg=typer.colors.GREEN)
                    
                    # Lösche die zugehörigen Audiodateien
                    delete_associated_audio_files(video_file, title)
                    
                    # Lösche alle anderen zugehörigen Dateien basierend auf dem Titel
                    delete_associated_files_based_on_title(video_file.parent, title)
                    
                except Exception as e:
                    typer.secho(f"Fehler beim Löschen der Videodatei: {e}", fg=typer.colors.RED)
                
                if title_image:
                    try:
                        title_image.unlink()
                        typer.secho(f"Titelbild '{title_image}' wurde gelöscht.", fg=typer.colors.GREEN)
                    except Exception as e:
                        typer.secho(f"Fehler beim Löschen des Titelbildes: {e}", fg=typer.colors.RED)
            else:
                typer.secho("Quelldateien wurden nicht gelöscht.", fg=typer.colors.YELLOW)
    
    typer.secho("Familienfilm-Integration erfolgreich abgeschlossen.", fg=typer.colors.GREEN)

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
    
    # Lade die Konfiguration aus config.toml
    config_path = Path(__file__).parent.parent.parent.parent / "config.toml"
    config = load_config(config_path)
    
    directories = [search_dir]
    if additional_dir:
        directories.append(additional_dir)
        typer.secho(f"Zusätzliches Verzeichnis hinzugefügt: '{additional_dir}'", fg=typer.colors.BLUE)
    
    image_extensions = ['.png', '.jpg', '.jpeg']

    # Schritt 1: Sammeln aller unterstützten Mediendateien (ohne ProRes) – jetzt auch Bilder
    media_files: List[Path] = []
    image_files: List[Path] = []

    for dir_path in directories:
        typer.secho(f"Durchsuche Verzeichnis: '{dir_path}'", fg=typer.colors.BLUE)
        for file_path in dir_path.rglob('*'):
            if file_path.is_file():
                # Überspringe versteckte Dateien
                if is_hidden(file_path):
                    typer.secho(f"Überspringe versteckte Datei: '{file_path}'", fg=typer.colors.YELLOW)
                    continue

                # Überprüfe, ob die Datei gerade verarbeitet wird
                if is_file_being_processed(file_path):
                    typer.secho(f"Überspringe Datei, die gerade verarbeitet wird: '{file_path}'", fg=typer.colors.YELLOW)
                    continue

                # Prüfe, ob es ein Bild ist
                if file_path.suffix.lower() in image_extensions:
                    # Bilddateien werden gleich mit verarbeitet
                    image_files.append(file_path)
                    continue

                try:
                    metadata = extract_metadata(file_path)
                    video_codec = metadata.get('VideoCodec', '').strip()

                    # Überspringe ProRes-Dateien direkt
                    if video_codec in ['Apple ProRes 422', 'Apple ProRes 422 HQ', 'Apple ProRes 4444', 'Apple ProRes 4444 XQ']:
                        typer.secho(f"ProRes-Datei erkannt und übersprungen: {file_path}", fg=typer.colors.YELLOW)
                        continue

                    # Füge nur nicht-ProRes-Videodateien zur Liste hinzu
                    if file_path.suffix.lower() in ['.mov', '.mp4', '.m4v']:
                        media_files.append(file_path)
                except Exception as e:
                    typer.secho(f"Fehler beim Verarbeiten von '{file_path}': {e}", fg=typer.colors.RED)
                    continue

    # Schritt 2: Gruppierung der Mediendateien nach Titel
    groups: Dict[str, Dict[str, List[Path]]] = {}

    # Zuerst Videos gruppieren
    for file_path in media_files:
        try:
            metadata = extract_metadata(file_path)
            title = metadata.get('Title') or metadata.get('DisplayName') or file_path.stem
            if not title:
                typer.secho(f"Keine Titel-Metadaten in '{file_path}' gefunden. Datei wird übersprungen.", fg=typer.colors.YELLOW)
                continue
            if title not in groups:
                groups[title] = {
                    'videos': [],
                    'images': []
                }
            groups[title]['videos'].append(file_path)
        except Exception as e:
            typer.secho(f"Fehler beim Gruppieren von '{file_path}': {e}", fg=typer.colors.RED)
            continue

    # Nun Bilder den passenden Gruppen hinzufügen (Titel ermitteln wie bei den Videos)
    for img_path in image_files:
        try:
            # Hier setzen wir voraus, dass die Bilder den gleichen Namen wie das Video tragen oder
            # zumindest denselben Titel im Namen haben (ggf. müssen Sie hier Logik anpassen,
            # um den Titel aus dem Bildnamen abzuleiten)
            # Beispiel: Der Titel ist vom Dateinamen des Bildes abgeleitet:
            img_title = img_path.stem
            # Titel entsprechend normalisieren, wenn nötig
            # Wenn der Titel eindeutig aus Metadaten gezogen werden kann, nutzen Sie auch extract_metadata(img_path) wie oben.
            
            # Falls der Titel aus dem Bild- oder Metadaten abgeleitet werden kann:
            img_metadata = extract_metadata(img_path)
            actual_title = img_metadata.get('Title') or img_metadata.get('DisplayName') or img_title
            
            if actual_title in groups:
                groups[actual_title]['images'].append(img_path)
            else:
                # Falls es noch keine Gruppe für diesen Titel gibt, erstellen wir sie.
                groups[actual_title] = {
                    'videos': [],
                    'images': [img_path]
                }
        except Exception as e:
            typer.secho(f"Fehler beim Gruppieren von Bild '{img_path}': {e}", fg=typer.colors.RED)
            continue
    
    # Schritt 3: Integration der Mediensets
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
                delete_source_files=delete_source_files,
                config=config
            )

            # Schritt 4: Lösche zugehörige ProRes-Dateien nach erfolgreicher Integration
            prores_file = video_file.parent / f"{title}.mov"
            if prores_file.exists():
                try:
                    prores_file.unlink()
                    typer.secho(f"ProRes-Datei '{prores_file}' gelöscht.", fg=typer.colors.GREEN)
                except Exception as e:
                    typer.secho(f"Fehler beim Löschen der ProRes-Datei '{prores_file}': {e}", fg=typer.colors.RED)
        except Exception as e:
            typer.secho(f"Fehler beim Integrieren von '{title}': {e}", fg=typer.colors.RED)
            continue

    typer.secho("\nIntegration aller Mediensets abgeschlossen.", fg=typer.colors.GREEN)

if __name__ == "__main__":
    app()