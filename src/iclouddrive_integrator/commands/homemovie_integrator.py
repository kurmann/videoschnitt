# src/iclouddrive_integrator/commands/homemovie_integrator.py

import subprocess
import typer
from pathlib import Path
from typing import Optional
import shutil
from datetime import datetime
import json
import logging

app = typer.Typer()

# Logging-Konfiguration
logging.basicConfig(
    filename='integrate_homemovie_icloud.log',
    filemode='a',
    format='%(asctime)s - %(levelname)s - %(message)s',
    level=logging.DEBUG
)
logger = logging.getLogger(__name__)

SUPPORTED_VIDEO_FORMATS = ['.mov', '.mp4', '.m4v']

def sanitize_filename(filename: str) -> str:
    """Bereinigt den Dateinamen, um nur erlaubte Zeichen zu enthalten."""
    return "".join(c for c in filename if c.isalnum() or c in " .-_()").rstrip()

def extract_metadata(file_path: Path) -> dict:
    """Extrahiert Metadaten aus der Datei mittels ExifTool."""
    command = ['exiftool', '-json', str(file_path)]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        typer.secho(f"Fehler beim Ausführen von ExifTool: {result.stderr}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Ausführen von ExifTool: {result.stderr}")
        raise typer.Exit(code=1)
    
    metadata = json.loads(result.stdout)[0] if result.stdout else {}
    logger.debug(f"Extrahierte Metadaten: {metadata}")
    return metadata

def determine_target_directory(icloud_dir: Path, metadata: dict) -> Path:
    """Bestimmt das Zielverzeichnis in iCloud basierend auf den Metadaten."""
    album = metadata.get("Album", "Unbekanntes Album")
    sanitized_album = sanitize_filename(album)
    ziel_album_dir = icloud_dir / sanitized_album
    ziel_album_dir.mkdir(parents=True, exist_ok=True)
    
    try:
        creation_date_str = metadata.get('CreationDate', '')
        # Versuche verschiedene Datumsformate
        for fmt in ('%Y:%m:%d', '%Y-%m-%d', '%Y/%m/%d'):
            try:
                creation_date = datetime.strptime(creation_date_str, fmt)
                jahr = str(creation_date.year)
                break
            except ValueError:
                continue
        else:
            jahr = 'Unknown'
    except Exception as e:
        logger.error(f"Fehler beim Parsen des Erstellungsdatums: {e}")
        jahr = 'Unknown'
    
    ziel_jahr_dir = ziel_album_dir / jahr
    ziel_jahr_dir.mkdir(parents=True, exist_ok=True)
    return ziel_jahr_dir

def delete_source_file(file_path: Path, delete_source: bool) -> None:
    """Löscht die Quelldatei nach Bestätigung oder ohne Rückfrage, wenn delete_source=True."""
    if delete_source:
        try:
            file_path.unlink()
            logger.info(f"Quelldatei gelöscht: {file_path}")
            typer.secho(f"Quelldatei gelöscht: {file_path}", fg=typer.colors.GREEN)
        except Exception as e:
            logger.error(f"Fehler beim Löschen der Quelldatei {file_path}: {e}")
            typer.secho(f"Fehler beim Löschen der Quelldatei {file_path}: {e}", fg=typer.colors.RED)
    else:
        confirm = typer.confirm(f"Möchtest du die Quelldatei '{file_path}' wirklich löschen?")
        if confirm:
            try:
                file_path.unlink()
                logger.info(f"Quelldatei gelöscht: {file_path}")
                typer.secho(f"Quelldatei gelöscht: {file_path}", fg=typer.colors.GREEN)
            except Exception as e:
                logger.error(f"Fehler beim Löschen der Quelldatei {file_path}: {e}")
                typer.secho(f"Fehler beim Löschen der Quelldatei {file_path}: {e}", fg=typer.colors.RED)
        else:
            typer.secho(f"Quelldatei nicht gelöscht: {file_path}", fg=typer.colors.YELLOW)
            logger.info(f"Quelldatei nicht gelöscht: {file_path}")

def integrate_homemovie_to_icloud(
    video_file: Path,
    icloud_dir: Path,
    overwrite_existing: bool,
    delete_source: bool
) -> None:
    """
    Integrates a single homemovie into iCloud Drive.

    :param video_file: Path to the video file.
    :param icloud_dir: Path to the iCloud directory.
    :param overwrite_existing: Whether to overwrite existing files.
    :param delete_source: Whether to delete the source file after integration.
    """
    typer.secho(f"Integriere Video '{video_file}' in iCloud...", fg=typer.colors.BLUE)
    logger.info(f"Beginne Integration von: {video_file}")

    metadata = extract_metadata(video_file)
    if not metadata.get('Title') or not metadata.get('CreationDate'):
        typer.secho("Essentielle Metadaten 'Title' oder 'CreationDate' fehlen.", fg=typer.colors.RED)
        logger.error("Essentielle Metadaten 'Title' oder 'CreationDate' fehlen.")
        raise typer.Exit(code=1)
    
    ziel_dir = determine_target_directory(icloud_dir, metadata)
    sanitized_title = sanitize_filename(metadata.get('Title'))
    try:
        creation_date = datetime.strptime(metadata.get('CreationDate', ''), '%Y:%m:%d')
        jahr = str(creation_date.year)
    except ValueError:
        try:
            creation_date = datetime.strptime(metadata.get('CreationDate', ''), '%Y-%m-%d')
            jahr = str(creation_date.year)
        except ValueError:
            jahr = 'Unknown'
    
    base_filename = f"{sanitized_title} ({jahr})"
    
    existing_files = list(ziel_dir.glob(f"{base_filename}*"))
    if existing_files and not overwrite_existing:
        typer.secho(f"Dateien für '{base_filename}' existieren bereits.", fg=typer.colors.YELLOW)
        if not typer.confirm(f"Bestehende Dateien überschreiben?"):
            logger.info(f"Integration abgebrochen: Bestehende Dateien für '{base_filename}' wurden nicht überschrieben.")
            raise typer.Exit(code=1)
    
    video_target = ziel_dir / f"{base_filename}{video_file.suffix}"
    try:
        shutil.copy2(video_file, video_target)
        logger.info(f"Video '{video_file}' kopiert nach '{video_target}'.")
        typer.secho(f"Video '{video_file}' kopiert nach '{video_target}'.", fg=typer.colors.GREEN)
    except Exception as e:
        logger.error(f"Fehler beim Kopieren der Datei: {e}")
        typer.secho(f"Fehler beim Kopieren der Datei: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    if delete_source:
        delete_source_file(video_file, delete_source=True)

def integrate_homemovie(
    video_file: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=True,
        dir_okay=False,
        readable=True,
        help="Der Pfad zur Videodatei, die in die Mediathek integriert werden soll."
    ),
    icloud_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        writable=True,
        readable=True,
        help="Das Zielverzeichnis in iCloud."
    ),
    overwrite_existing: bool = typer.Option(
        False,
        "--overwrite-existing",
        help="Überschreibt bestehende Dateien ohne Rückfrage, wenn diese existieren."
    ),
    delete_source: bool = typer.Option(
        False,
        "--delete-source",
        help="Löscht die Quelldateien nach erfolgreicher Integration ohne Rückfrage."
    )
):
    """
    Integriert eine einzelne Heimvideo-Datei in iCloud Drive.
    """
    integrate_homemovie_to_icloud(
        video_file=video_file,
        icloud_dir=icloud_dir,
        overwrite_existing=overwrite_existing,
        delete_source=delete_source
    )

def integrate_homemovies(
    search_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Das Verzeichnis mit Mediendateien."
    ),
    additional_dir: Optional[Path] = typer.Option(
        None,
        "--additional-dir",
        "-ad",
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Zusätzliches Verzeichnis."
    ),
    icloud_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        writable=True,
        readable=True,
        help="Das Zielverzeichnis in iCloud."
    ),
    overwrite_existing: bool = typer.Option(
        False,
        "--overwrite-existing",
        help="Überschreibt bestehende Dateien."
    ),
    delete_source: bool = typer.Option(
        False,
        "--delete-source",
        help="Löscht die Quelldateien nach erfolgreicher Integration ohne Rückfrage."
    )
):
    """
    Integriert mehrere Heimvideo-Dateien in iCloud Drive.
    """
    directories = [search_dir]
    if additional_dir:
        directories.append(additional_dir)
        typer.secho(f"Zusätzliches Verzeichnis: '{additional_dir}'", fg=typer.colors.BLUE)
        logger.info(f"Zusätzliches Verzeichnis hinzugefügt: {additional_dir}")
    
    media_files = []
    for dir_path in directories:
        for file_path in dir_path.rglob('*'):
            if file_path.is_file() and file_path.suffix.lower() in SUPPORTED_VIDEO_FORMATS:
                media_files.append(file_path)
    
    typer.secho(f"Gefundene Mediendateien: {len(media_files)}", fg=typer.colors.BLUE)
    logger.info(f"Gefundene Mediendateien: {len(media_files)}")
    
    for video_file in media_files:
        try:
            integrate_homemovie_to_icloud(
                video_file=video_file,
                icloud_dir=icloud_dir,
                overwrite_existing=overwrite_existing,
                delete_source=delete_source
            )
        except typer.Exit:
            typer.secho(f"Integration abgebrochen für '{video_file}'.", fg=typer.colors.RED)
            logger.error(f"Integration abgebrochen für '{video_file}'.")
            continue  # Fahre mit der nächsten Datei fort
        except Exception as e:
            typer.secho(f"Unbekannter Fehler bei der Integration von '{video_file}': {e}", fg=typer.colors.RED)
            logger.error(f"Unbekannter Fehler bei der Integration von '{video_file}': {e}")
            continue  # Fahre mit der nächsten Datei fort
    
    typer.secho("Integration mehrerer Heimvideo-Dateien abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Integration mehrerer Heimvideo-Dateien abgeschlossen.")

if __name__ == "__main__":
    app()