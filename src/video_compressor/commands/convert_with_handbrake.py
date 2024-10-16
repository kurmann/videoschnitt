# src/video_compressor/commands/convert_with_handbrake.py

import typer
import os
from typing import List
from metadata_manager.commands.get_video_codec import get_video_codec
import subprocess
import logging

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

def convert_videos_with_handbrake_command(
    directory: str = typer.Argument(..., help="Pfad zum Verzeichnis mit den Videodateien"),
    preset_file: str = typer.Option(
        '/Users/patrickkurmann/Library/Containers/fr.handbrake.HandBrake/Data/Library/Application Support/HandBrake/UserPresets.json',
        help="Pfad zur Preset-Datei (JSON)"
    ),
    preset: str = typer.Option("YouTube", help="Name des HandBrakeCLI Presets"),
    postfix: str = typer.Option(POSTFIX, help="Postfix für die Ausgabedateien"),
    keep_original: bool = typer.Option(False, "--keep-original", help="Originaldateien behalten und nicht löschen")
):
    """
    Konvertiert alle nicht H.264 oder HEVC Videos in einem Verzeichnis nach HEVC mit HandBrakeCLI.
    """
    # Zähler initialisieren
    total_videos = 0
    converted_videos = 0
    skipped_videos = 0
    pending_videos = 0

    # Listen für spätere Aktionen
    original_files_to_delete: List[str] = []
    compressed_files_to_rename: List[str] = []

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
        base_name, extension = os.path.splitext(video_file)
        output_file = f"{base_name}{postfix}{OUTPUT_EXTENSION}"
        output_path = os.path.join(directory, output_file)

        # Überprüfe, ob die Ausgabedatei bereits existiert
        if os.path.exists(output_path):
            typer.secho(f"[{index}/{total_videos}] Ausgabedatei '{output_file}' existiert bereits. Datei wird übersprungen.", fg=typer.colors.YELLOW)
            skipped_videos += 1
            pending_videos -= 1
            continue

        # Baue den HandBrakeCLI Befehl
        cmd = [
            'HandBrakeCLI',
            '--preset-import-file', preset_file,
            '--preset', preset,
            '-i', file_path,
            '-o', output_path
        ]

        typer.secho(f"[{index}/{total_videos}] Konvertiere '{video_file}' mit HandBrakeCLI...", fg=typer.colors.YELLOW)
        logger.debug(f"Führe HandBrakeCLI mit folgendem Befehl aus: {' '.join(cmd)}")

        try:
            # Führe den HandBrakeCLI Prozess aus und gebe die Ausgaben aus
            process = subprocess.Popen(cmd, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, text=True)

            for line in process.stdout:
                print(line, end='')

            process.wait()
            returncode = process.returncode

            if returncode != 0:
                typer.secho(f"Fehler bei der Konvertierung von '{video_file}'.", fg=typer.colors.RED)
                skipped_videos += 1
                pending_videos -= 1
                continue

            # Speichere die Dateien für spätere Aktionen
            original_files_to_delete.append(file_path)
            compressed_files_to_rename.append(output_path)

            typer.secho(f"Erfolgreich konvertiert: '{output_file}'", fg=typer.colors.GREEN)
            converted_videos += 1
            pending_videos -= 1

            # Fortschritt anzeigen
            typer.secho(f"Fortschritt: {converted_videos} konvertiert, {skipped_videos} übersprungen, {pending_videos} verbleibend.", fg=typer.colors.BLUE)

        except KeyboardInterrupt:
            typer.secho("Abbruchsignal erhalten. Beende laufenden HandBrakeCLI-Prozess...", fg=typer.colors.RED)
            process.terminate()
            process.wait()
            raise typer.Exit()

    typer.secho(f"\nKonvertierung abgeschlossen.", fg=typer.colors.GREEN)
    typer.secho(f"{converted_videos} Videos wurden konvertiert.", fg=typer.colors.GREEN)
    typer.secho(f"{skipped_videos} Videos wurden übersprungen.", fg=typer.colors.YELLOW)

    # Nach der Konvertierung: Entscheide, ob Originale gelöscht und Dateien umbenannt werden sollen
    if not keep_original and converted_videos > 0:
        delete_confirm = typer.confirm(f"Möchten Sie die {converted_videos} Originaldatei(en) löschen und die komprimierten Dateien umbenennen?")
        if delete_confirm:
            for original_file, compressed_file in zip(original_files_to_delete, compressed_files_to_rename):
                # Lösche die Originaldatei
                try:
                    os.remove(original_file)
                    typer.secho(f"Originaldatei '{os.path.basename(original_file)}' wurde gelöscht.", fg=typer.colors.GREEN)
                except Exception as e:
                    typer.secho(f"Fehler beim Löschen der Originaldatei '{os.path.basename(original_file)}': {e}", fg=typer.colors.RED)
                    continue  # Fahre mit der nächsten Datei fort

                # Benenne die komprimierte Datei um (Postfix entfernen)
                base_name, extension = os.path.splitext(os.path.basename(original_file))
                new_output_file = f"{base_name}{extension}"
                new_output_path = os.path.join(directory, new_output_file)
                try:
                    os.rename(compressed_file, new_output_path)
                    typer.secho(f"Ausgabedatei wurde umbenannt zu '{new_output_file}'.", fg=typer.colors.GREEN)
                except Exception as e:
                    typer.secho(f"Fehler beim Umbenennen der Ausgabedatei '{os.path.basename(compressed_file)}': {e}", fg=typer.colors.RED)
        else:
            typer.secho("Originaldateien wurden beibehalten. Komprimierte Dateien behalten das Postfix.", fg=typer.colors.YELLOW)
    else:
        if converted_videos > 0:
            typer.secho("Originaldateien wurden beibehalten. Komprimierte Dateien behalten das Postfix.", fg=typer.colors.YELLOW)

def main():
    typer.run(convert_videos_with_handbrake_command)

if __name__ == "__main__":
    main()