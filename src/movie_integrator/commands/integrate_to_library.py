import typer
import os
import shutil
from pathlib import Path
from typing import List

app = typer.Typer()

SUPPORTED_EXTENSIONS = ['.jpg', '.jpeg', '.png']
POSTFIX = '-poster'
IGNORE_SUFFIX = '-poster'
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

def rename_artwork_file(file: Path) -> bool:
    """Benennt eine Datei gemäß den definierten Regeln um."""
    if should_ignore(file) or file.suffix.lower() not in SUPPORTED_EXTENSIONS:
        return False

    if not file.stem.lower().endswith(IGNORE_SUFFIX):
        new_stem = file.stem + POSTFIX
        new_name = new_stem + file.suffix
        new_file = file.with_name(new_name)

        if not new_file.exists():
            try:
                file.rename(new_file)
                typer.secho(f"Erfolgreich umbenannt: '{file.name}' -> '{new_name}'", fg=typer.colors.GREEN)
                return True
            except Exception as e:
                typer.secho(f"Fehler beim Umbenennen von '{file.name}': {e}", fg=typer.colors.RED)
                return False
    return False

def integrate_to_library_command(
    source_directory: Path = typer.Argument(..., help="Pfad zum Quellverzeichnis"),
    target_directory: Path = typer.Argument(..., help="Pfad zum Zielverzeichnis")
):
    """
    Verschiebt alle Dateien aus dem Quellverzeichnis in das Zielverzeichnis,
    erhält die relative Verzeichnisstruktur und integriert Artwork-Dateien.
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
        destination_file = target_directory / relative_path

        # Erstelle das Verzeichnis für die Datei im Ziel
        destination_file.parent.mkdir(parents=True, exist_ok=True)

        # Überprüfe, ob die Zieldatei bereits existiert
        if destination_file.exists():
            overwrite = typer.confirm(f"Die Datei '{destination_file}' existiert bereits. Möchten Sie sie überschreiben?")
            if not overwrite:
                typer.secho(f"Überspringe Datei '{destination_file}'.", fg=typer.colors.YELLOW)
                continue

        # Datei verschieben
        shutil.move(str(file_path), str(destination_file))

        # Artwork-Dateien umbenennen, nachdem sie an den Zielort verschoben wurden
        if destination_file.suffix.lower() in SUPPORTED_EXTENSIONS:
            rename_artwork_file(destination_file)

        # Dateigröße ermitteln und Ausgabe der Verschiebung
        size_in_bytes = destination_file.stat().st_size
        human_readable_size = get_human_readable_size(size_in_bytes)
        file_number = str(file_counter + 1).zfill(num_digits)
        
        typer.secho(f"Datei {file_number} von {total_files} verschoben: {destination_file.name} ({human_readable_size})", fg=typer.colors.GREEN)
        file_counter += 1

    typer.secho("\nIntegration abgeschlossen.", fg=typer.colors.GREEN)

if __name__ == "__main__":
    typer.run(integrate_to_library_command)