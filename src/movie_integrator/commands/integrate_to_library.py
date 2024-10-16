# src/movie_integrator/commands/integrate_to_library.py

import typer
import os
import shutil
from typing import Dict, List
from pathlib import Path

app = typer.Typer()

def get_human_readable_size(size_in_bytes):
    """Konvertiert die Dateigröße in ein menschenlesbares Format."""
    for unit in ['B', 'KB', 'MB', 'GB', 'TB']:
        if size_in_bytes < 1024.0:
            return f"{size_in_bytes:.1f} {unit}"
        size_in_bytes /= 1024.0
    return f"{size_in_bytes:.1f} PB"

def integrate_to_library_command(
    source_directory: Path = typer.Argument(..., help="Pfad zum Quellverzeichnis"),
    target_directory: Path = typer.Argument(..., help="Pfad zum Zielverzeichnis")
):
    """
    Verschiebt alle Dateien aus dem Quellverzeichnis in das Zielverzeichnis, erhält die relative Verzeichnisstruktur
    und gruppiert Dateien mit demselben Namen (unterschiedlichen Erweiterungen) in Ordnern mit dem Dateinamen ohne Erweiterung.
    """
    # Überprüfen, ob das Quellverzeichnis existiert
    if not source_directory.is_dir():
        typer.secho(f"Das Quellverzeichnis '{source_directory}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Überprüfen, ob das Zielverzeichnis existiert, wenn nicht, erstelle es
    if not target_directory.exists():
        target_directory.mkdir(parents=True, exist_ok=True)

    # Ausgabe der Pfade
    typer.secho(f"Integriere von '{source_directory}' nach '{target_directory}'\n", fg=typer.colors.BLUE)

    # Sammle alle Dateien aus dem Quellverzeichnis (inklusive Unterverzeichnisse), ignoriere versteckte Dateien
    files = [f for f in source_directory.rglob('*') if f.is_file() and not f.name.startswith('.')]
    total_files = len(files)
    if total_files == 0:
        typer.secho("Keine Dateien zum Integrieren gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    # Dictionary zum Gruppieren von Dateien mit demselben Basename
    grouped_files: Dict[Path, List[Path]] = {}

    for file_path in files:
        # Relativen Pfad zum Quellverzeichnis ermitteln
        relative_path = file_path.relative_to(source_directory)

        # Überprüfe, ob der Pfad versteckte Verzeichnisse enthält, ignoriere diese
        if any(part.startswith('.') for part in relative_path.parts):
            continue

        # Verzeichnis des aktuellen Dateipfads
        parent_dir = relative_path.parent

        # Basename ohne Erweiterung
        basename = file_path.stem

        # Neuer relativer Pfad im Zielverzeichnis
        new_relative_dir = target_directory / parent_dir / basename

        # Füge die Datei zur Gruppe hinzu
        grouped_files.setdefault(new_relative_dir, []).append(file_path)

    # Dateien verschieben und gruppieren
    file_counter = 0
    total_files = sum(len(file_list) for file_list in grouped_files.values())
    num_digits = len(str(total_files))

    for group_dir, file_list in grouped_files.items():
        # Zielverzeichnis erstellen
        group_dir.mkdir(parents=True, exist_ok=True)

        for file_path in file_list:
            file_counter += 1
            # Zielpfad für die Datei
            destination_file = group_dir / file_path.name

            # Überprüfe, ob die Zieldatei bereits existiert
            if destination_file.exists():
                overwrite = typer.confirm(f"Die Datei '{destination_file}' existiert bereits. Möchten Sie sie überschreiben?")
                if not overwrite:
                    typer.secho(f"Überspringe Datei '{destination_file}'.", fg=typer.colors.YELLOW)
                    continue

            # Datei verschieben (überschreiben, falls bestätigt)
            shutil.move(str(file_path), str(destination_file))

            # Dateigröße ermitteln
            size_in_bytes = destination_file.stat().st_size
            human_readable_size = get_human_readable_size(size_in_bytes)

            # Dateinummer mit führenden Nullen
            file_number = str(file_counter).zfill(num_digits)

            # Ausgabe der Verschiebung
            typer.secho(
                f"Datei {file_number} von {total_files} verschoben: {destination_file.name} ({human_readable_size})",
                fg=typer.colors.GREEN
            )

    typer.secho("\nIntegration abgeschlossen.", fg=typer.colors.GREEN)

def main():
    typer.run(integrate_to_library_command)

if __name__ == "__main__":
    main()