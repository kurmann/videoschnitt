import typer
import os
import shutil
from pathlib import Path
from collections import defaultdict

app = typer.Typer()

SUPPORTED_IMAGE_EXTENSIONS = ['.jpg', '.jpeg', '.png']

def integrate_to_library_command(
    source_directory: Path = typer.Argument(..., help="Pfad zum Quellverzeichnis"),
    target_directory: Path = typer.Argument(..., help="Pfad zum Zielverzeichnis")
):
    """
    Gruppiert Dateien im Quellverzeichnis nach Dateinamen (ohne Erweiterung),
    erstellt ein Unterverzeichnis mit diesem Namen, erstellt 'poster.jpg' und 'fanart.jpg' 
    aus vorhandenen Bilddateien und verschiebt das Unterverzeichnis in das Ziel.
    """
    if not source_directory.is_dir():
        typer.secho(f"Das Quellverzeichnis '{source_directory}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    if not target_directory.exists():
        target_directory.mkdir(parents=True, exist_ok=True)

    typer.secho(f"Integriere von '{source_directory}' nach '{target_directory}'\n", fg=typer.colors.BLUE)

    # Gruppiere Dateien nach Basename (Dateiname ohne Erweiterung)
    file_groups = defaultdict(list)
    for file_path in source_directory.rglob('*'):
        if file_path.is_file() and not file_path.name.startswith('.'):
            file_groups[file_path.stem].append(file_path)

    if not file_groups:
        typer.secho("Keine Dateien zum Integrieren gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    for base_name, file_list in file_groups.items():
        relative_path = file_list[0].relative_to(source_directory).parent
        destination_dir = target_directory / relative_path / base_name

        # Erstelle das Verzeichnis für die Gruppierung
        destination_dir.mkdir(parents=True, exist_ok=True)

        for file_path in file_list:
            # Verschiebe die Dateien ins neue Verzeichnis
            destination_file = destination_dir / file_path.name
            shutil.move(str(file_path), str(destination_file))
            typer.secho(f"Datei verschoben: {destination_file.name}", fg=typer.colors.CYAN)

            # Wenn es sich um eine unterstützte Bilddatei handelt, erstelle poster.jpg und fanart.jpg
            if file_path.suffix.lower() in SUPPORTED_IMAGE_EXTENSIONS:
                poster_path = destination_dir / "poster.jpg"
                fanart_path = destination_dir / "fanart.jpg"
                
                if not poster_path.exists():
                    destination_file.rename(poster_path)
                    typer.secho(f"Bild umbenannt in 'poster.jpg': {poster_path.name}", fg=typer.colors.GREEN)
                
                if not fanart_path.exists():
                    shutil.copy2(poster_path, fanart_path)
                    typer.secho(f"Bild dupliziert als 'fanart.jpg': {fanart_path.name}", fg=typer.colors.GREEN)

    typer.secho("\nIntegration abgeschlossen.", fg=typer.colors.GREEN)

if __name__ == "__main__":
    typer.run(integrate_to_library_command)