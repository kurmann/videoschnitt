# mediaset_manager/commands/list_mediafiles.py

import re
import unicodedata
import typer
from pathlib import Path
from typing import Optional, List, Dict
import subprocess
import json
import logging

app = typer.Typer()

# Konfiguriere das Logging
logging.basicConfig(
    filename='list_mediafiles.log',
    filemode='a',
    format='%(asctime)s - %(levelname)s - %(message)s',
    level=logging.DEBUG  # Setze auf DEBUG für detailliertes Logging
)
logger = logging.getLogger(__name__)

# Unterstützte Dateiformate
SUPPORTED_VIDEO_FORMATS = ['.mov', '.mp4', '.m4v']
SUPPORTED_IMAGE_FORMATS = ['.jpg', '.jpeg', '.png']

# ProRes-Codec-Bezeichnung (Anpassung je nach ExifTool-Ausgabe)
PRORES_CODECS = ['Apple ProRes 422', 'Apple ProRes 422 HQ', 'Apple ProRes 4444', 'Apple ProRes 4444 XQ']

def sanitize_filename(filename: str) -> str:
    """
    Entfernt ungültige Zeichen aus dem Dateinamen, erlaubt jedoch Umlaute und bestimmte Sonderzeichen.
    Normalisiert den Unicode.
    """
    # Unicode-Normalisierung
    filename = unicodedata.normalize('NFC', filename)
    
    # Definiere eine Whitelist für erlaubte Zeichen, einschließlich Umlaute und bestimmte Sonderzeichen
    whitelist = re.compile(r'[^A-Za-z0-9 äöüÄÖÜß.\-_()]')
    
    sanitized = whitelist.sub('', filename).rstrip()
    
    logger.debug(f"Original filename: '{filename}' -> Sanitized filename: '{sanitized}'")
    
    return sanitized

def extract_metadata(file_path: Path) -> dict:
    """
    Extrahiert Metadaten einer Datei mithilfe von ExifTool.
    """
    command = ['exiftool', '-j', str(file_path)]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        logger.error(f"Error running exiftool on '{file_path}': {result.stderr}")
        raise Exception(f"Error running exiftool on '{file_path}': {result.stderr}")
    try:
        metadata = json.loads(result.stdout)[0]
        return metadata
    except (json.JSONDecodeError, IndexError) as e:
        logger.error(f"Invalid JSON output from exiftool for '{file_path}': {e}")
        raise Exception(f"Invalid JSON output from exiftool for '{file_path}': {e}")

def is_prores(metadata: dict) -> bool:
    """
    Überprüft, ob die Datei den ProRes-Codec verwendet.
    """
    codec = metadata.get('VideoCodec', '').strip()
    return codec in PRORES_CODECS

def is_supported_video_file(file_path: Path) -> bool:
    """
    Überprüft, ob die Datei eine unterstützte Video-Datei ist.
    """
    return file_path.suffix.lower() in SUPPORTED_VIDEO_FORMATS

def is_supported_image_file(file_path: Path) -> bool:
    """
    Überprüft, ob die Datei eine unterstützte Bild-Datei ist.
    """
    return file_path.suffix.lower() in SUPPORTED_IMAGE_FORMATS

@app.command("list-mediafiles")
def list_mediafiles(
    search_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Das Verzeichnis, in dem nach Mediendateien gesucht werden soll."
    ),
    additional_media_dir: Optional[Path] = typer.Option(
        None,
        "--additional-media-dir",
        "-amd",
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Zusätzliches Verzeichnis zur Suche nach Mediendateien."
    )
):
    """
    Sucht im angegebenen Verzeichnis (und optional einem weiteren) nach Mediendateien, gruppiert sie nach Titel und listet zusammengehörende Dateien auf.
    """
    typer.secho(f"Suche nach Mediendateien in '{search_dir}'...", fg=typer.colors.BLUE)
    directories = [search_dir]
    if additional_media_dir:
        directories.append(additional_media_dir)
        typer.secho(f"Zusätzliches Verzeichnis hinzugefügt: '{additional_media_dir}'", fg=typer.colors.BLUE)
    
    # Schritt 1: Sammeln aller unterstützten Videodateien ohne ProRes
    media_files: List[Path] = []
    for dir_path in directories:
        typer.secho(f"Durchsuche Verzeichnis: '{dir_path}'", fg=typer.colors.BLUE)
        for file_path in dir_path.rglob('*'):
            if file_path.is_file() and is_supported_video_file(file_path):
                try:
                    metadata = extract_metadata(file_path)
                    if not is_prores(metadata):
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
            title = sanitize_filename(title)
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
    
    # Schritt 3: Suche nach zugehörigen Titelbildern
    for title, files in groups.items():
        # Suche nach Bilddateien mit demselben Titel
        for dir_path in directories:
            image_file = dir_path / f"{title}.jpg"
            if image_file.exists() and is_supported_image_file(image_file):
                groups[title]['images'].append(image_file)
                typer.secho(f"Gefundenes Titelbild für '{title}': '{image_file}'", fg=typer.colors.GREEN)
                logger.info(f"Gefundenes Titelbild für '{title}': '{image_file}'")
            else:
                # Suche nach anderen unterstützten Bildformaten
                for ext in SUPPORTED_IMAGE_FORMATS:
                    image_file = dir_path / f"{title}{ext}"
                    if image_file.exists() and is_supported_image_file(image_file):
                        groups[title]['images'].append(image_file)
                        typer.secho(f"Gefundenes Titelbild für '{title}': '{image_file}'", fg=typer.colors.GREEN)
                        logger.info(f"Gefundenes Titelbild für '{title}': '{image_file}'")
                        break  # Stoppe nach dem ersten gefundenen Bild
    
    # Schritt 4: Ausgabe der gruppierten Mediendateien
    typer.secho("\nGefundene Mediensets:", fg=typer.colors.BLUE)
    for title, files in groups.items():
        typer.secho(f"\nTitel: {title}", fg=typer.colors.CYAN, bold=True)
        typer.secho("  Videos:", fg=typer.colors.GREEN)
        for video in files['videos']:
            typer.echo(f"    - {video}")
        if files['images']:
            typer.secho("  Titelbilder:", fg=typer.colors.GREEN)
            for image in files['images']:
                typer.echo(f"    - {image}")
        else:
            typer.secho("  Titelbilder: Keine gefunden.", fg=typer.colors.YELLOW)
    
    typer.secho("\nAuflisten der Mediendateien abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Auflisten der Mediendateien abgeschlossen.")