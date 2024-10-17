# emby_integrator/commands/group_files.py

import typer
from pathlib import Path
from typing import List
import logging
from rich.progress import Progress, SpinnerColumn, BarColumn, TextColumn, TimeElapsedColumn

app = typer.Typer()

# Modulvariablen für Konfiguration
DEFAULT_IGNORED_SUFFIXES = ['-poster', '-artwork', '-fanart']
SNAPSHOT_IDENTIFIER = '#snapshot'  # Identifiziert Snapshot-Verzeichnisse

# Initialisiere Logging
logging.basicConfig(
    filename='group_files.log',
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

def get_base_name(file: Path, ignored_suffixes: List[str]) -> str:
    """
    Gibt den Basename der Datei zurück, indem ignorierte Suffixe entfernt werden.
    """
    name = file.stem
    for suffix in ignored_suffixes:
        if name.lower().endswith(suffix.lower()):
            name = name[:-len(suffix)]
            break  # Nur das erste übereinstimmende Suffix entfernen
    return name

@app.command()
def group_files(
    directory: str = typer.Argument(..., help="Pfad zum Verzeichnis, das gruppiert werden soll"),
    ignored_suffixes: List[str] = typer.Option(
        DEFAULT_IGNORED_SUFFIXES,
        "--ignore-suffix",
        "-i",
        help="Liste von Suffixen, die beim Gruppieren ignoriert werden sollen (case-insensitive)",
        show_default=True
    )
):
    """
    Gruppiert Dateien mit gleichen Basenamen in Unterverzeichnisse.
    """
    dir_path = Path(directory)

    if not dir_path.is_dir():
        typer.secho(f"Das Verzeichnis '{directory}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    typer.secho(f"Starte das Gruppieren der Dateien in '{directory}'...", fg=typer.colors.BLUE)

    # Finde alle unterstützten Dateien, schließe Snapshot-Verzeichnisse aus
    all_files: List[Path] = list(dir_path.rglob("*"))
    grouped_files = {}

    for file in all_files:
        if file.is_file():
            if is_snapshot_path(file):
                # Dateien in Snapshot-Verzeichnissen ignorieren
                continue
            base_name = get_base_name(file, ignored_suffixes)
            if base_name not in grouped_files:
                grouped_files[base_name] = []
            grouped_files[base_name].append(file)

    # Filtere Gruppen mit mindestens zwei Dateien
    groups_to_move = {k: v for k, v in grouped_files.items() if len(v) >= 2}

    total_groups = len(groups_to_move)
    if total_groups == 0:
        typer.secho("Keine Gruppen von Dateien mit gleichen Basenamen gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    typer.secho(f"Insgesamt {total_groups} Gruppen gefunden. Beginne mit dem Gruppieren...", fg=typer.colors.BLUE)

    processed = 0
    moved = 0
    skipped = 0

    with Progress(
        SpinnerColumn(),
        "[progress.description]{task.description}",
        BarColumn(),
        TextColumn("[progress.percentage]{task.percentage:>3.0f}%"),
        TimeElapsedColumn(),
        transient=True
    ) as progress:
        task = progress.add_task("[green]Gruppiere Dateien...", total=total_groups)
        for base_name, files in groups_to_move.items():
            # Bestimme das Zielverzeichnis relativ zum aktuellen Verzeichnis der Datei
            # Annahme: Alle Dateien in der Gruppe befinden sich im gleichen Verzeichnis
            # Daher nehmen wir das erste File, um das Verzeichnis zu bestimmen
            first_file = files[0]
            target_dir = first_file.parent / base_name

            for file in files:
                # Überprüfe, ob die Datei bereits im Zielverzeichnis ist
                if file.parent.name.lower() == base_name.lower():
                    # Datei befindet sich bereits im richtigen Verzeichnis
                    skipped += 1
                    continue

                # Erstelle das Zielverzeichnis, falls es nicht existiert
                if not target_dir.exists():
                    try:
                        target_dir.mkdir(parents=True, exist_ok=True)
                        logger.info(f"Erstelle Verzeichnis '{target_dir}'.")
                    except Exception as e:
                        logger.error(f"Fehler beim Erstellen des Verzeichnisses '{target_dir}': {e}")
                        skipped += 1
                        continue

                # Bestimme den Zielpfad der Datei
                target_file = target_dir / file.name

                # Überprüfe, ob die Ziel-Datei bereits existiert
                if target_file.exists():
                    logger.warning(f"Ziel-Datei '{target_file}' existiert bereits. Überspringe Datei '{file}'.")
                    skipped += 1
                    continue

                try:
                    file.rename(target_file)
                    logger.info(f"Verschoben: '{file}' -> '{target_file}'")
                    moved += 1
                except PermissionError as pe:
                    logger.error(f"Fehler beim Verschieben von '{file}': {pe}")
                    skipped += 1
                except Exception as e:
                    logger.error(f"Unbekannter Fehler beim Verschieben von '{file}': {e}")
                    skipped += 1

            processed += 1
            progress.update(task, advance=1)

    typer.secho("\nGruppierung abgeschlossen.", fg=typer.colors.GREEN)
    typer.secho(f"Gruppen verarbeitet: {processed}", fg=typer.colors.BLUE)
    typer.secho(f"Dateien erfolgreich verschoben: {moved}", fg=typer.colors.GREEN)
    typer.secho(f"Dateien übersprungen: {skipped}", fg=typer.colors.YELLOW)

if __name__ == "__main__":
    app()