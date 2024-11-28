import shutil
from datetime import datetime
from pathlib import Path
import typer

# Standardwerte für Quell- und Zielverzeichnisse
DEFAULT_SOURCE_DIR = "/Volumes/SD_Card/DCIM/DJI_001"
DEFAULT_TARGET_DIR = "/Volumes/Crucial-P3/Import/Originalmedien"

def import_dji_files(
    source_dir: str = DEFAULT_SOURCE_DIR,
    target_dir: str = DEFAULT_TARGET_DIR
):
    """
    Importiert Medien von einer DJI Action Kamera und organisiert sie in einem strukturierten Verzeichnis.

    - Quelle: Dateien aus `source_dir` werden analysiert.
    - Ziel: Dateien werden nach ISO-Datum in `target_dir` organisiert.
    - `.LRF`-Dateien werden nach erfolgreicher Integration gelöscht.

    :param source_dir: Quellverzeichnis mit den DJI-Dateien.
    :param target_dir: Zielverzeichnis für organisierte Dateien.
    """
    source_path = Path(source_dir)
    target_path = Path(target_dir)

    if not source_path.exists():
        typer.echo(f"Quellverzeichnis {source_path} existiert nicht.")
        raise typer.Exit(1)

    # Erstelle Zielverzeichnis falls es nicht existiert
    target_path.mkdir(parents=True, exist_ok=True)

    # Verarbeite alle MP4-Dateien
    for mp4_file in source_path.glob("*.MP4"):
        # Extrahiere Datum/Zeit aus dem Dateinamen
        filename_parts = mp4_file.stem.split("_")
        if len(filename_parts) < 3:
            typer.echo(f"Ungültiges Dateiformat: {mp4_file.name}")
            continue

        # Datum/Zeit in ISO-Format umwandeln
        try:
            timestamp = datetime.strptime(filename_parts[1], "%Y%m%d%H%M%S")
            iso_date = timestamp.strftime("%Y-%m-%d")
            new_filename = timestamp.strftime("%Y-%m-%d_%H-%M-%S.mov")
        except ValueError:
            typer.echo(f"Konnte Datum/Zeit aus {mp4_file.name} nicht extrahieren.")
            continue

        # Zielverzeichnis erstellen
        daily_target_path = target_path / iso_date
        daily_target_path.mkdir(parents=True, exist_ok=True)

        # Datei verschieben
        new_file_path = daily_target_path / new_filename
        typer.echo(f"Verschiebe {mp4_file} nach {new_file_path}")
        shutil.move(str(mp4_file), new_file_path)

        # Lösche zugehörige .LRF-Datei, falls vorhanden
        lrf_file = mp4_file.with_suffix(".LRF")
        if lrf_file.exists():
            typer.echo(f"Lösche zugehörige .LRF-Datei: {lrf_file}")
            lrf_file.unlink()

    typer.echo("Import abgeschlossen.")
