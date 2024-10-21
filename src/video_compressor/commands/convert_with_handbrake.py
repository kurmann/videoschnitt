# src/video_compressor/commands/convert_with_handbrake.py

import typer
import os
from typing import List
from metadata_manager.commands.get_video_codec import get_video_codec
import subprocess
import logging
from rich.progress import Progress, SpinnerColumn, BarColumn, TextColumn, TimeElapsedColumn

logger = logging.getLogger(__name__)

# Modulvariablen für Konfiguration
PRESET = "YouTube"  # Wähle ein geeignetes Preset oder erstelle ein eigenes
POSTFIX = "-hevc"
OUTPUT_EXTENSION = ".mp4"
VIDEO_EXTENSIONS = [
    ".mp4", ".mov", ".avi", ".mkv", ".flv", ".wmv", ".webm", ".mpeg", ".mpg",
    ".m4v", ".3gp", ".3g2", ".mts", ".m2ts", ".ts", ".vob", ".ogv", ".dv",
    ".f4v", ".rm", ".rmvb"
]

app = typer.Typer()

def convert_videos_with_handbrake_command(
    directory: str = typer.Argument(..., help="Pfad zum Verzeichnis mit den Videodateien"),
    preset_file: str = typer.Option(
        '/Users/patrickkurmann/Library/Containers/fr.handbrake.HandBrake/Data/Library/Application Support/HandBrake/UserPresets.json',
        help="Pfad zur Preset-Datei (JSON)"
    ),
    preset: str = typer.Option("YouTube", help="Name des HandBrakeCLI Presets"),
    postfix: str = typer.Option(POSTFIX, help="Postfix für die Ausgabedateien"),
    keep_original: bool = typer.Option(False, "--keep-original", help="Originaldateien behalten und nicht löschen"),
    force_remux_mp4_h264: bool = typer.Option(
        False,
        "--force-remux-mp4-h264",
        help="Erzwingt das Remuxing von MP4-Dateien mit H.264 Codec."
    )
):
    """
    Konvertiert alle nicht H.264 oder HEVC Videos in einem Verzeichnis nach HEVC mit HandBrakeCLI.
    WebM-Dateien mit H.264 oder HEVC werden ohne Neukodierung in einen MP4-Container remuxt.
    Mit der Option --force-remux-mp4-h264 werden MP4-Dateien mit H.264 Codec ebenfalls remuxt.
    """
    # Zähler initialisieren
    total_videos = 0
    converted_videos = 0
    remuxed_videos = 0
    skipped_videos = 0
    pending_videos = 0

    # Listen für spätere Aktionen
    original_files_to_delete: List[str] = []
    processed_files: List[str] = []

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

    typer.secho(f"Insgesamt {total_videos} Videodatei(en) gefunden. Beginne mit der Konvertierung...", fg=typer.colors.BLUE)

    with Progress(
        SpinnerColumn(),
        "[progress.description]{task.description}",
        BarColumn(),
        TimeElapsedColumn(),
        transient=True
    ) as progress:
        task = progress.add_task("[green]Konvertiere Videos...", total=total_videos)
        for index, video_file in enumerate(video_files, start=1):
            file_path = os.path.join(directory, video_file)
            codec = get_video_codec(file_path)

            if codec is None:
                skipped_videos += 1
                pending_videos -= 1
                progress.update(task, advance=1)
                continue

            # Erstelle den Ausgabedateinamen
            base_name, extension = os.path.splitext(video_file)
            output_file = f"{base_name}{postfix}{OUTPUT_EXTENSION}"
            output_path = os.path.join(directory, output_file)

            # Überprüfe, ob die Ausgabedatei bereits existiert
            if os.path.exists(output_path):
                skipped_videos += 1
                pending_videos -= 1
                progress.update(task, advance=1)
                continue

            # Prüfe, ob die Datei eine WebM-Datei mit HEVC oder H.264 ist
            if extension.lower() == '.webm' and codec.lower() in ['hevc', 'h264']:
                remux_type = "WebM"
                typer.echo(f"[{index}/{total_videos}] Remux '{video_file}' zu MP4 ohne Neukodierung...")
                # Baue den FFmpeg-Befehl zum Remuxen
                cmd = [
                    'ffmpeg',
                    '-y',  # Überschreibt bestehende Dateien ohne Nachfrage
                    '-i', file_path,
                    '-c', 'copy',
                    '-movflags', 'faststart',  # Optional für bessere Streaming-Performance
                    output_path
                ]

                logger.debug(f"Führe FFmpeg mit folgendem Befehl aus: {' '.join(cmd)}")

                # Führe den FFmpeg-Prozess aus
                process = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

                if process.returncode != 0:
                    skipped_videos += 1
                    pending_videos -= 1
                    progress.update(task, advance=1)
                    continue

                remuxed_videos += 1
                pending_videos -= 1

                # Speichere die Dateien für spätere Aktionen
                original_files_to_delete.append(file_path)
                processed_files.append(output_path)

            # Prüfe, ob die Datei ein MP4 mit H.264 Codec ist und das Remuxing erzwungen werden soll
            elif extension.lower() == '.mp4' and codec.lower() == 'h264' and force_remux_mp4_h264:
                remux_type = "MP4-H264 (erzwingt)"
                typer.echo(f"[{index}/{total_videos}] Remux '{video_file}' zu MP4 ohne Neukodierung (erzwingt)...")
                # Baue den FFmpeg-Befehl zum Remuxen
                cmd = [
                    'ffmpeg',
                    '-y',  # Überschreibt bestehende Dateien ohne Nachfrage
                    '-i', file_path,
                    '-c', 'copy',
                    '-movflags', 'faststart',  # Optional für bessere Streaming-Performance
                    output_path
                ]

                logger.debug(f"Führe FFmpeg mit folgendem Befehl aus: {' '.join(cmd)}")

                # Führe den FFmpeg-Prozess aus
                process = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

                if process.returncode != 0:
                    skipped_videos += 1
                    pending_videos -= 1
                    progress.update(task, advance=1)
                    continue

                remuxed_videos += 1
                pending_videos -= 1

                # Speichere die Dateien für spätere Aktionen
                original_files_to_delete.append(file_path)
                processed_files.append(output_path)

            # Überspringe bereits kompatible MP4-Dateien mit HEVC oder H.264 (ohne erzwungenes Remuxing)
            elif extension.lower() == '.mp4' and codec.lower() in ['hevc', 'h264']:
                skipped_videos += 1
                pending_videos -= 1
                progress.update(task, advance=1)
                continue

            else:
                # Ansonsten konvertiere mit HandBrakeCLI
                typer.echo(f"[{index}/{total_videos}] Konvertiere '{video_file}' mit HandBrakeCLI...")
                # Baue den HandBrakeCLI Befehl
                cmd = [
                    'HandBrakeCLI',
                    '--preset-import-file', preset_file,
                    '--preset', preset,
                    '-i', file_path,
                    '-o', output_path
                ]

                logger.debug(f"Führe HandBrakeCLI mit folgendem Befehl aus: {' '.join(cmd)}")

                try:
                    # Führe den HandBrakeCLI Prozess aus und gebe die Ausgaben aus
                    process = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)

                    for line in process.stdout:
                        print(line, end='')

                    process.wait()
                    returncode = process.returncode

                    if returncode != 0:
                        skipped_videos += 1
                        pending_videos -= 1
                        progress.update(task, advance=1)
                        continue

                    converted_videos += 1
                    pending_videos -= 1

                    # Speichere die Dateien für spätere Aktionen
                    original_files_to_delete.append(file_path)
                    processed_files.append(output_path)

                except KeyboardInterrupt:
                    typer.secho("Abbruchsignal erhalten. Beende laufenden HandBrakeCLI-Prozess...", fg=typer.colors.RED)
                    process.terminate()
                    process.wait()
                    raise typer.Exit()

            # Fortschritt anzeigen nach jeder Datei
            progress.update(task, advance=1)

    # Nach der Verarbeitung: Entscheide, ob Originale gelöscht und Dateien umbenannt werden sollen
    if not keep_original and (converted_videos + remuxed_videos) > 0:
        delete_confirm = typer.confirm(f"Möchten Sie die {converted_videos + remuxed_videos} Originaldatei(en) löschen und die verarbeiteten Dateien umbenennen?")
        if delete_confirm:
            for original_file, processed_file in zip(original_files_to_delete, processed_files):
                # Lösche die Originaldatei
                try:
                    os.remove(original_file)
                    typer.secho(f"Originaldatei '{os.path.basename(original_file)}' wurde gelöscht.", fg=typer.colors.GREEN)
                except Exception as e:
                    typer.secho(f"Fehler beim Löschen der Originaldatei '{os.path.basename(original_file)}': {e}", fg=typer.colors.RED)
                    continue  # Fahre mit der nächsten Datei fort

                # Benenne die verarbeitete Datei um (Postfix entfernen und korrekte Erweiterung setzen)
                base_name, original_extension = os.path.splitext(os.path.basename(original_file))
                new_output_file = f"{base_name}{OUTPUT_EXTENSION}"
                new_output_path = os.path.join(os.path.dirname(processed_file), new_output_file)
                try:
                    os.rename(processed_file, new_output_path)
                    typer.secho(f"Ausgabedatei wurde umbenannt zu '{new_output_file}'.", fg=typer.colors.GREEN)
                except Exception as e:
                    typer.secho(f"Fehler beim Umbenennen der Ausgabedatei '{os.path.basename(processed_file)}': {e}", fg=typer.colors.RED)

    # Abschlussmeldungen
    typer.secho(f"\nVerarbeitung abgeschlossen.", fg=typer.colors.GREEN)
    typer.secho(f"{converted_videos} Videos wurden konvertiert.", fg=typer.colors.GREEN)
    typer.secho(f"{remuxed_videos} Videos wurden remuxt.", fg=typer.colors.GREEN)
    typer.secho(f"{skipped_videos} Videos wurden übersprungen.", fg=typer.colors.YELLOW)

def main():
    typer.run(convert_videos_with_handbrake_command)

if __name__ == "__main__":
    main()