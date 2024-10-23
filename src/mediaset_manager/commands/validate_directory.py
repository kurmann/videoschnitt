# mediaset_manager/commands/validate_directory.py

import typer
from pathlib import Path
from mediaset_manager.utils import validate_video_files
import logging

app = typer.Typer()
logger = logging.getLogger(__name__)

# Modulvariable für Mindestgröße von Videodateien (in Bytes)
MIN_VIDEO_SIZE = 100 * 1024  # 100 KB

# Erwartete Dateien für ein Familienfilm-Medienset
REQUIRED_FILES = ["Titelbild.png", "Metadaten.yaml"]
OPTIONAL_FILES = [
    "Video-Internet-4K.m4v",
    "Video-Internet-HD.m4v",
    "Video-Internet-SD.m4v",
    "Video-Medienserver.mov",
    "Projekt.tar"
]
VIDEO_EXTENSIONS = {'.mp4', '.mov', '.m4v', '.avi', '.mkv'}

@app.command("validate-directory")
def validate_directory(
    directory_path: Path = typer.Argument(..., help="Pfad zum Medienset-Verzeichnis")
):
    """
    Überprüft, ob ein Verzeichnis ein gültiges Medienset ist.
    """
    if not directory_path.is_dir():
        typer.secho(f"Der Pfad '{directory_path}' ist kein Verzeichnis.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Überprüfe Pflichtdateien
    missing_files = [f for f in REQUIRED_FILES if not (directory_path / f).is_file()]
    if missing_files:
        typer.secho("Fehlende Pflichtdateien:", fg=typer.colors.RED)
        for f in missing_files:
            typer.secho(f" - {f}", fg=typer.colors.RED)
    else:
        typer.secho("Alle Pflichtdateien sind vorhanden.", fg=typer.colors.GREEN)
    
    # Überprüfe Videodateien
    video_files = [f for f in directory_path.iterdir() if f.suffix.lower() in VIDEO_EXTENSIONS]
    if not video_files:
        typer.secho("Keine Videodateien gefunden. Mindestens eine Videodatei ist erforderlich.", fg=typer.colors.RED)
    else:
        typer.secho("Gefundene Videodateien:", fg=typer.colors.GREEN)
        for video in video_files:
            size = video.stat().st_size
            if size < MIN_VIDEO_SIZE:
                typer.secho(f" - {video.name} (zu klein: {size} Bytes)", fg=typer.colors.RED)
            else:
                typer.secho(f" - {video.name} ({size} Bytes)", fg=typer.colors.GREEN)
    
    # Zusammenfassung
    small_files = validate_video_files(video_files, MIN_VIDEO_SIZE)
    if missing_files or not video_files or small_files:
        typer.secho("Das Verzeichnis ist kein gültiges Medienset.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    else:
        typer.secho("Das Verzeichnis ist ein gültiges Medienset.", fg=typer.colors.GREEN)