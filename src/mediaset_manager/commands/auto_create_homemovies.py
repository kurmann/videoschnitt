# mediaset_manager/commands/auto_create_homemovies.py

import typer
from pathlib import Path
from typing import Optional
import subprocess
import json
from mediaset_manager.commands.create_homemovie import create_homemovie

app = typer.Typer()

def extract_metadata(file_path: Path) -> dict:
    """
    Extrahiert Metadaten einer Datei mithilfe von exiftool.
    """
    command = ['exiftool', '-j', str(file_path)]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        raise Exception(f"Error running exiftool on '{file_path}': {result.stderr}")
    metadata = json.loads(result.stdout)[0]
    return metadata

def is_supported_video_file(file_path: Path) -> bool:
    """
    Überprüft, ob die Datei eine unterstützte Video-Datei ist.
    """
    return file_path.suffix.lower() in ['.mov', '.mp4', '.m4v']

def is_supported_image_file(file_path: Path) -> bool:
    """
    Überprüft, ob die Datei eine unterstützte Bild-Datei ist.
    """
    return file_path.suffix.lower() in ['.png', '.jpg', '.jpeg']

@app.command("auto-create-homemovies")
def auto_create_homemovies(
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
    ),
    no_prompt: bool = typer.Option(
        False,
        "--no-prompt",
        help="Unterdrückt die Nachfrage beim Verschieben der Dateien."
    ),
):
    """
    Sucht im angegebenen Verzeichnis nach Mediendateien und erstellt automatisch Mediensets basierend auf den Metadaten.
    """
    typer.secho(f"Suche nach Mediendateien in '{search_dir}'...", fg=typer.colors.BLUE)

    # Schritt 1: Sammeln aller Videodateien
    media_files = []
    for file_path in search_dir.iterdir():
        if file_path.is_file() and is_supported_video_file(file_path):
            try:
                metadata = extract_metadata(file_path)
                media_files.append((file_path, metadata))
            except Exception as e:
                typer.secho(f"Fehler beim Verarbeiten von '{file_path}': {e}", fg=typer.colors.RED)
                continue
        else:
            # Bilddateien und andere Dateien ignorieren wir hier
            pass

    if not media_files:
        typer.secho("Keine Mediendateien gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    # Schritt 2: Metadatenextraktion und Gruppierung
    groups = {}
    for file_path, metadata in media_files:
        title = metadata.get('Title') or metadata.get('DisplayName') or metadata.get('Name')
        if not title:
            typer.secho(f"Keine Titel-Metadaten in '{file_path}' gefunden. Datei wird übersprungen.", fg=typer.colors.YELLOW)
            continue
        if title not in groups:
            groups[title] = []
        groups[title].append((file_path, metadata))

    if not groups:
        typer.secho("Keine Gruppen von Mediendateien mit gemeinsamen Titeln gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    # Schritt 3: Auswahl der Metadatenquelle pro Gruppe
    medienset_sources = []
    for title, files in groups.items():
        # Priorisiere .mov-Dateien
        mov_files = [f for f in files if f[0].suffix.lower() == '.mov']
        if mov_files:
            # Wähle die größte .mov-Datei
            source_file = max(mov_files, key=lambda f: f[0].stat().st_size)
        else:
            # Wähle die größte .mp4 oder .m4v-Datei
            other_files = [f for f in files if f[0].suffix.lower() in ['.mp4', '.m4v']]
            if other_files:
                source_file = max(other_files, key=lambda f: f[0].stat().st_size)
            else:
                typer.secho(f"Keine geeignete Metadatenquelle für Gruppe '{title}' gefunden.", fg=typer.colors.YELLOW)
                continue
        medienset_sources.append((title, source_file[0]))
        typer.secho(f"Metadatenquelle für Gruppe '{title}': '{source_file[0]}'", fg=typer.colors.GREEN)

    if not medienset_sources:
        typer.secho("Keine Mediensets zum Erstellen gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    # Schritt 4: Benutzerbestätigung
    if not no_prompt:
        typer.secho("\nDie folgenden Mediensets werden erstellt:", fg=typer.colors.BLUE)
        for title, source_file in medienset_sources:
            typer.echo(f" - '{title}' mit Quelle '{source_file}'")
        proceed = typer.confirm("\nMöchten Sie fortfahren und alle Dateien verschieben?")
        if not proceed:
            typer.secho("Abgebrochen.", fg=typer.colors.RED)
            raise typer.Exit()
    else:
        typer.secho("Erstelle Mediensets ohne weitere Nachfrage...", fg=typer.colors.YELLOW)

    # Schritt 5: Sequenzielle Verarbeitung jeder Gruppe
    for title, source_file in medienset_sources:
        typer.secho(f"\nErstelle Medienset für '{title}'...", fg=typer.colors.BLUE)
        try:
            # Rufe das bestehende Kommando 'create_homemovie' auf
            create_homemovie(
                metadata_source=source_file,
                additional_media_dir=additional_media_dir,
                no_prompt=True  # Unterdrückt die Nachfrage beim Verschieben der Dateien
            )
        except Exception as e:
            typer.secho(f"Fehler beim Erstellen des Mediensets für '{title}': {e}", fg=typer.colors.RED)
            continue  # Fahre mit dem nächsten Medienset fort

    typer.secho("\nAutomatische Erstellung der Mediensets abgeschlossen.", fg=typer.colors.GREEN)