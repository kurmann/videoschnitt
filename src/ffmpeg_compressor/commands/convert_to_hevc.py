# src/ffmpeg_compressor/commands/convert_to_hevc.py

import typer
import os
from typing import Optional
from metadata_manager.commands.get_video_codec import get_video_codec
import subprocess
import logging

logger = logging.getLogger(__name__)

# Modulvariablen für Konfiguration
QUALITY_INDEX = "18"
POSTFIX = "-hevc"
OUTPUT_EXTENSION = ".mp4"  # Geändert von ".m4v" zu ".mp4"
VIDEO_EXTENSIONS = [
    ".mp4", ".mov", ".avi", ".mkv", ".flv", ".wmv", ".webm", ".mpeg", ".mpg",
    ".m4v", ".3gp", ".3g2", ".mts", ".m2ts", ".ts", ".vob", ".ogv", ".dv",
    ".f4v", ".rm", ".rmvb"
]

def convert_videos_to_hevc_command(
    directory: str = typer.Argument(..., help="Pfad zum Verzeichnis mit den Videodateien"),
    quality_index: str = typer.Option(QUALITY_INDEX, help="Qualitätsindex für ffmpeg (CRF-Wert)"),
    postfix: str = typer.Option(POSTFIX, help="Postfix für die Ausgabedateien")
):
    """
    Konvertiert alle nicht H.264 oder HEVC Videos in einem Verzeichnis nach HEVC in einem MP4-Container.
    """
    # Zähler initialisieren
    total_videos = 0
    converted_videos = 0
    skipped_videos = 0
    pending_videos = 0

    # Überprüfen, ob das Verzeichnis existiert
    if not os.path.isdir(directory):
        typer.secho(f"Das Verzeichnis '{directory}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Liste aller Dateien im Verzeichnis
    files = os.listdir(directory)

    # Filtere nur Videodateien basierend auf den Erweiterungen
    video_files = [
        f for f in files
        if os.path.isfile(os.path.join(directory, f)) and
        os.path.splitext(f)[1].lower() in VIDEO_EXTENSIONS
    ]
    total_videos = len(video_files)
    pending_videos = total_videos

    if total_videos == 0:
        typer.secho("Keine Videodateien zum Konvertieren gefunden.", fg=typer.colors.YELLOW)
        raise typer.Exit()

    typer.secho(f"Insgesamt {total_videos} Videodateien gefunden. Beginne mit der Konvertierung...", fg=typer.colors.BLUE)

    for index, video_file in enumerate(video_files, start=1):
        file_path = os.path.join(directory, video_file)
        codec = get_video_codec(file_path)

        if codec is None:
            typer.secho(f"[{index}/{total_videos}] Konnte Videocodec für '{video_file}' nicht ermitteln. Datei wird übersprungen.", fg=typer.colors.RED)
            skipped_videos += 1
            pending_videos -= 1
            continue

        if codec.lower() in ['h264', 'hevc']:
            typer.secho(f"[{index}/{total_videos}] '{video_file}' hat bereits einen kompatiblen Codec ({codec}).", fg=typer.colors.GREEN)
            skipped_videos += 1
            pending_videos -= 1
            continue

        # Erstelle den Ausgabedateinamen
        base_name, _ = os.path.splitext(video_file)
        output_file = f"{base_name}{postfix}{OUTPUT_EXTENSION}"
        output_path = os.path.join(directory, output_file)

        # Informiere den Benutzer, wenn die Ausgabedatei bereits existiert und überschreibe sie
        if os.path.exists(output_path):
            typer.secho(f"Ausgabedatei '{output_file}' existiert bereits und wird überschrieben.", fg=typer.colors.YELLOW)

        # Baue den ffmpeg Befehl
        cmd = [
            'ffmpeg',
            '-y',  # Überschreibt bestehende Dateien ohne Nachfrage
            '-i', file_path,
            '-c:v', 'libx265',
            '-crf', quality_index,
            '-c:a', 'copy',
            '-tag:v', 'hvc1',  # Wichtig für Kompatibilität mit Apple-Geräten
            output_path
        ]

        typer.secho(f"[{index}/{total_videos}] Konvertiere '{video_file}' nach HEVC...", fg=typer.colors.YELLOW)
        logger.debug(f"Führe ffmpeg mit folgendem Befehl aus: {' '.join(cmd)}")

        # Führe den ffmpeg Prozess aus und warte, bis er fertig ist
        process = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

        if process.returncode != 0:
            typer.secho(f"Fehler bei der Konvertierung von '{video_file}':\n{process.stderr}", fg=typer.colors.RED)
            skipped_videos += 1
            pending_videos -= 1
            continue

        typer.secho(f"Erfolgreich konvertiert: '{output_file}'", fg=typer.colors.GREEN)
        converted_videos += 1
        pending_videos -= 1

        # Fortschritt anzeigen
        typer.secho(f"Fortschritt: {converted_videos} konvertiert, {skipped_videos} übersprungen, {pending_videos} verbleibend.", fg=typer.colors.BLUE)

    typer.secho(f"\nKonvertierung abgeschlossen.", fg=typer.colors.GREEN)
    typer.secho(f"{converted_videos} Videos wurden konvertiert.", fg=typer.colors.GREEN)
    typer.secho(f"{skipped_videos} Videos wurden übersprungen.", fg=typer.colors.YELLOW)

def main():
    typer.run(convert_videos_to_hevc_command)

if __name__ == "__main__":
    main()