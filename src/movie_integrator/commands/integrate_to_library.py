import typer
import os
import shutil
from pathlib import Path
from typing import List

app = typer.Typer()

SUPPORTED_EXTENSIONS = ['.jpg', '.jpeg', '.png']
EXACT_IGNORE_NAMES = ['folder.jpg', 'folder.jpeg', 'folder.png']

def get_human_readable_size(size_in_bytes):
    """Konvertiert die Dateigröße in ein menschenlesbares Format."""
    for unit in ['B', 'KB', 'MB', 'GB', 'TB']:
        if size_in_bytes < 1024.0:
            return f"{size_in_bytes:.1f} {unit}"
        size_in_bytes /= 1024.0
    return f"{size_in_bytes:.1f} PB"

def should_ignore(file: Path) -> bool:
    """Entscheidet, ob eine Datei ignoriert werden soll."""
    if file.name.lower() in EXACT_IGNORE_NAMES:
        typer.secho(f"Datei '{file.name}' wird ignoriert (exakter Ignorierungsname).", fg=typer.colors.GREEN)
        return True
    return False

def create_fanart_and_poster_files(file: Path, destination_dir: Path):
    """Erstellt Kopien der Datei als 'fanart.jpg' und 'poster.jpg' im Zielverzeichnis."""
    try:
        fanart_path = destination_dir / "fanart.jpg"
        poster_path = destination_dir / "poster.jpg"
        
        shutil.copy2(file, fanart_path)
        shutil.copy2(file, poster_path)
        
        typer.secho(f"Fanart und Poster erstellt: '{fanart_path}' und '{poster_path}'", fg=typer.colors.GREEN)
        return True
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen von Fanart und Poster für '{file.name}': {e}", fg=typer.colors.RED)
        return False

def integrate_to_library_command(
    source_directory: Path = typer.Argument(..., help="Pfad zum Quellverzeichnis"),
    target_directory: Path = typer.Argument(..., help="Pfad zum Zielverzeichnis")
):
    """
    Verschiebt alle Dateien aus dem Quellverzeichnis in das Zielverzeichnis,
    erhält die relative Verzeichnisstruktur und integriert Dateien mit Fanart- und Poster-Kopien.
    """
    if not source_directory.is_dir():
        typer.secho(f"Das Quellverzeichnis '{source_directory}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    if not target_directory.exists():
        target_directory.mkdir(parents=True, exist_ok=True)

    typer.secho(f"Integriere von '{source_directory}' nach '{target_directory}'\n", fg=typer.colors.BLUE)

    files = [f for f in source_directory.rglob('*') if f.is_file() and not f.name.startswith('.')]
    if not files:
        typer.secho("Keine Dateien zum Integrieren gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    total_files = len(files)
    file_counter = 0
    num_digits = len(str(total_files))

    for file_path in files:
        # Berechne den relativen Pfad des Quellverzeichnisses
        relative_path = file_path.relative_to(source_directory)
        base_name = file_path.stem
        destination_dir = target_directory / relative_path.parent / base_name

        # Erstelle das Verzeichnis für die Datei im Ziel
        destination_dir.mkdir(parents=True, exist_ok=True)

        # Überprüfe, ob die Zieldatei bereits existiert
        destination_file = destination_dir / file_path.name
        if destination_file.exists():
            overwrite = typer.confirm(f"Die Datei '{destination_file}' existiert bereits. Möchten Sie sie überschreiben?")
            if not overwrite:
                typer.secho(f"Überspringe Datei '{destination_file}'.", fg=typer.colors.YELLOW)
                continue

        # Datei verschieben
        shutil.move(str(file_path), str(destination_file))

        # Artwork-Dateien kopieren und umbenennen, wenn es sich um unterstützte Dateien handelt
        if destination_file.suffix.lower() in SUPPORTED_EXTENSIONS and not should_ignore(destination_file):
            create_fanart_and_poster_files(destination_file, destination_dir)

        # Dateigröße ermitteln und Ausgabe der Verschiebung
        size_in_bytes = destination_file.stat().st_size
        human_readable_size = get_human_readable_size(size_in_bytes)
        file_number = str(file_counter + 1).zfill(num_digits)
        
        typer.secho(f"Datei {file_number} von {total_files} verschoben: {destination_file.name} ({human_readable_size})", fg=typer.colors.GREEN)
        file_counter += 1

    typer.secho("\nIntegration abgeschlossen.", fg=typer.colors.GREEN)

if __name__ == "__main__":
    typer.run(integrate_to_library_command)