# emby_integrator/commands/reset_permissions.py

import typer
import os
from pathlib import Path
import logging
from rich.progress import Progress, SpinnerColumn, BarColumn, TimeElapsedColumn

app = typer.Typer()

# Modulvariablen für Konfiguration
# Standardberechtigungen: Verzeichnisse 755, Dateien 644
DIR_PERMISSIONS = 0o755
FILE_PERMISSIONS = 0o644

# Initialisiere Logging
logging.basicConfig(
    filename='reset_permissions.log',
    filemode='a',
    format='%(asctime)s - %(levelname)s - %(message)s',
    level=logging.INFO
)
logger = logging.getLogger(__name__)

def reset_permissions(path: Path) -> bool:
    """
    Setzt die Berechtigungen der Datei oder des Verzeichnisses auf Standardwerte.
    Verzeichnisse erhalten 755, Dateien 644.
    Gibt True zurück, wenn erfolgreich, sonst False.
    """
    try:
        if path.is_dir():
            os.chmod(path, DIR_PERMISSIONS)
            logger.info(f"Set permissions for directory '{path}' to {oct(DIR_PERMISSIONS)}")
        elif path.is_file():
            os.chmod(path, FILE_PERMISSIONS)
            logger.info(f"Set permissions for file '{path}' to {oct(FILE_PERMISSIONS)}")
        else:
            # Symlinks oder andere Dateitypen überspringen
            logger.info(f"Skipped non-regular file '{path}'")
            return False
        return True
    except PermissionError as pe:
        typer.secho(f"Fehler beim Setzen der Berechtigungen für '{path}': {pe}", fg=typer.colors.RED)
        logger.error(f"Fehler beim Setzen der Berechtigungen für '{path}': {pe}")
        return False
    except Exception as e:
        typer.secho(f"Unbekannter Fehler beim Setzen der Berechtigungen für '{path}': {e}", fg=typer.colors.RED)
        logger.error(f"Unbekannter Fehler beim Setzen der Berechtigungen für '{path}': {e}")
        return False

@app.command()
def reset_permissions_command(
    directory: str = typer.Argument(..., help="Pfad zum Verzeichnis, dessen Berechtigungen zurückgesetzt werden sollen"),
    recursive: bool = typer.Option(True, "--recursive/--no-recursive", help="Rekursiv alle Unterverzeichnisse bearbeiten")
):
    """
    Setzt die Berechtigungen eines Verzeichnisses und optional aller Unterverzeichnisse und Dateien zurück.
    Verzeichnisse erhalten 755, Dateien 644.
    """
    dir_path = Path(directory)

    if not dir_path.is_dir():
        typer.secho(f"Das Verzeichnis '{directory}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    typer.secho(f"Starte das Zurücksetzen der Berechtigungen in '{directory}'...", fg=typer.colors.BLUE)

    if recursive:
        all_paths = list(dir_path.rglob("*"))
    else:
        all_paths = list(dir_path.iterdir())

    total_paths = len(all_paths)
    if total_paths == 0:
        typer.secho("Keine Dateien oder Verzeichnisse gefunden, die bearbeitet werden müssen.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    typer.secho(f"Insgesamt {total_paths} Datei(en)/Verzeichnis(se) gefunden. Beginne mit dem Zurücksetzen der Berechtigungen...", fg=typer.colors.BLUE)

    processed = 0
    successes = 0
    failures = 0

    with Progress(
        SpinnerColumn(),
        "[progress.description]{task.description}",
        BarColumn(),
        TimeElapsedColumn(),
        transient=True
    ) as progress:
        task = progress.add_task("[green]Setze Berechtigungen...", total=total_paths)
        for path in all_paths:
            if reset_permissions(path):
                successes += 1
            else:
                failures += 1
            processed += 1
            progress.update(task, advance=1)
    
    typer.secho("\n\nZurücksetzen der Berechtigungen abgeschlossen.", fg=typer.colors.GREEN)
    typer.secho(f"Gesamt verarbeitet: {processed}", fg=typer.colors.BLUE)
    typer.secho(f"Erfolgreich zurückgesetzt: {successes}", fg=typer.colors.GREEN)
    typer.secho(f"Fehler: {failures}", fg=typer.colors.RED)

if __name__ == "__main__":
    app()