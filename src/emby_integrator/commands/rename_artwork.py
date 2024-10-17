# emby_integrator/commands/rename_artwork.py

import typer
import os
from pathlib import Path
from typing import List
import logging

app = typer.Typer()

# Modulvariablen für Konfiguration
SUPPORTED_EXTENSIONS = ['.jpg', '.jpeg', '.png']
POSTFIX = '-poster'
REPLACE_SUFFIX = '-fanart'
IGNORE_SUFFIX = '-poster'
EXACT_IGNORE_NAMES = ['folder.jpg', 'folder.jpeg', 'folder.png']
SNAPSHOT_IDENTIFIER = '#snapshot'  # Identifiziert Snapshot-Verzeichnisse

# Initialisiere Logging
logging.basicConfig(
    filename='rename_artwork.log',
    filemode='a',
    format='%(asctime)s - %(levelname)s - %(message)s',
    level=logging.INFO
)
logger = logging.getLogger(__name__)

def is_snapshot_path(file: Path) -> bool:
    """
    Überprüft, ob die Datei sich in einem Snapshot-Verzeichnis befindet.
    """
    return SNAPSHOT_IDENTIFIER in file.parts

def should_ignore(file: Path) -> bool:
    """
    Entscheidet, ob eine Datei ignoriert werden soll.
    Ignoriert Dateien, die exakt "folder.jpg", "folder.jpeg", "folder.png" heißen
    oder sich in einem Snapshot-Verzeichnis befinden.
    """
    # Ignoriere Dateien, die exakt "folder.jpg", "folder.jpeg" oder "folder.png" heißen
    if file.name.lower() in EXACT_IGNORE_NAMES:
        typer.secho(f"Datei '{file.name}' wird ignoriert (exakter Ignorierungsname).", fg=typer.colors.GREEN)
        return True
    
    # Ignoriere Dateien in Snapshot-Verzeichnissen
    if is_snapshot_path(file):
        typer.secho(f"Datei '{file.name}' wird ignoriert (Snapshot-Verzeichnis).", fg=typer.colors.GREEN)
        return True
    
    return False

def rename_file(file: Path) -> bool:
    """
    Benennt eine Datei gemäß den definierten Regeln um.
    Gibt True zurück, wenn die Datei erfolgreich umbenannt wurde, sonst False.
    """
    original_name = file.name
    new_name = original_name

    if file.suffix.lower() not in SUPPORTED_EXTENSIONS:
        return False

    if should_ignore(file):
        return False

    if file.stem.lower().endswith(REPLACE_SUFFIX):
        new_stem = file.stem[:-len(REPLACE_SUFFIX)] + POSTFIX
        new_name = new_stem + file.suffix
    elif not file.stem.lower().endswith(IGNORE_SUFFIX):
        new_stem = file.stem + POSTFIX
        new_name = new_stem + file.suffix
    else:
        typer.secho(f"Datei '{original_name}' wird übersprungen (bereits mit '{IGNORE_SUFFIX}' versehen).", fg=typer.colors.YELLOW)
        return False

    new_file = file.with_name(new_name)

    if new_file.exists():
        typer.secho(f"Ausgabedatei '{new_name}' existiert bereits. Datei wird übersprungen.", fg=typer.colors.YELLOW)
        return False

    try:
        file.rename(new_file)
        typer.secho(f"Erfolgreich umbenannt: '{original_name}' -> '{new_name}'", fg=typer.colors.GREEN)
        logger.info(f"Umbenannt: '{original_name}' -> '{new_name}'")
        return True
    except PermissionError as pe:
        typer.secho(f"Fehler beim Umbenennen von '{original_name}': {pe}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Umbenennen von '{original_name}': {pe}")
        return False
    except Exception as e:
        typer.secho(f"Fehler beim Umbenennen von '{original_name}': {e}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Umbenennen von '{original_name}': {e}")
        return False

@app.command()
def rename_artwork(
    directory: str = typer.Argument(..., help="Pfad zum Verzeichnis mit den Artwork-Dateien"),
):
    """
    Benennt alle JPG, JPEG und PNG-Dateien in einem Verzeichnis (inkl. Unterverzeichnisse) um,
    indem das Suffix '-poster' hinzugefügt oder ersetzt wird.
    """
    dir_path = Path(directory)

    if not dir_path.is_dir():
        typer.secho(f"Das Verzeichnis '{directory}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    typer.secho(f"Starte das Umbenennen der Artwork-Dateien in '{directory}'...", fg=typer.colors.BLUE)

    # Finde alle unterstützten Dateien, schließe Snapshot-Verzeichnisse aus
    artwork_files: List[Path] = list(dir_path.rglob("*"))
    artwork_files = [f for f in artwork_files if f.suffix.lower() in SUPPORTED_EXTENSIONS]

    total_files = len(artwork_files)
    if total_files == 0:
        typer.secho("Keine JPG, JPEG oder PNG-Dateien zum Umbenennen gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    typer.secho(f"Insgesamt {total_files} Artwork-Datei(en) gefunden. Beginne mit dem Umbenennen...", fg=typer.colors.BLUE)

    processed = 0
    skipped = 0
    renamed = 0

    for index, file in enumerate(artwork_files, start=1):
        typer.secho(f"[{index}/{total_files}] Verarbeite '{file.name}'...", fg=typer.colors.CYAN, nl=False)
        success = rename_file(file)
        if success:
            typer.secho(" Umbenannt.", fg=typer.colors.GREEN)
            renamed += 1
        else:
            typer.secho(" Übersprungen.", fg=typer.colors.YELLOW)
            skipped += 1
        processed += 1

    typer.secho("\n\nUmbenennung abgeschlossen.", fg=typer.colors.GREEN)
    typer.secho(f"Gesamt verarbeitet: {processed}")
    typer.secho(f"Erfolgreich umbenannt: {renamed}", fg=typer.colors.GREEN)
    typer.secho(f"Übersprungen: {skipped}", fg=typer.colors.YELLOW)

if __name__ == "__main__":
    app()