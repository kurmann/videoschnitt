import os
import subprocess
import logging
from typing import List
import typer

logger = logging.getLogger(__name__)

app = typer.Typer()

OUTPUT_EXTENSION = ".m4v"


def convert_to_apple_compatible(
    input_file: str = typer.Argument(..., help="Pfad zur Eingabedatei"),
    keep_original: bool = typer.Option(False, "--keep-original", help="Behalte die Originaldatei."),
    constant_quality: int = typer.Option(89, help="Constant Quality für Videotoolbox (CQ-Wert).")
):
    """
    Konvertiert eine Videodatei in ein Apple-kompatibles Format.
    Videostreams werden remuxt, wenn möglich. DTS wird zu E-AC3 umgewandelt.
    Nicht-kompatible Untertitel werden extrahiert, kompatible übernommen.
    """
    # Pfade und Dateinamen
    base_name, extension = os.path.splitext(os.path.basename(input_file))
    directory = os.path.dirname(input_file)
    output_file = os.path.join(directory, f"{base_name}{OUTPUT_EXTENSION}")

    if not os.path.isfile(input_file):
        typer.secho(f"Die Datei '{input_file}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Prüfen, ob die Datei bereits konvertiert wurde
    if os.path.exists(output_file):
        typer.secho(f"Die Ausgabedatei '{output_file}' existiert bereits. Überspringe Konvertierung.", fg=typer.colors.YELLOW)
        return

    # FFmpeg-Kommando aufbauen
    cmd = [
        "ffmpeg", "-i", input_file,
        # Video: Passthrough, falls HEVC oder H.264, sonst neu encodieren
        "-c:v", "copy" if extension.lower() in [".hevc", ".h264"] else "hevc_videotoolbox",
        "-q:v", str(constant_quality),
        # Audio: DTS zu E-AC3
        "-c:a", "eac3",
        # Untertitel: Nur kompatible übernehmen, andere ignorieren
        "-scodec", "mov_text",
        # MP4-Format setzen
        "-f", "mp4", output_file
    ]

    typer.secho(f"Starte Konvertierung von '{input_file}' nach Apple-kompatiblem Format...", fg=typer.colors.BLUE)
    logger.debug(f"FFmpeg-Kommando: {' '.join(cmd)}")

    # Prozess starten
    process = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if process.returncode != 0:
        if "hdmv_pgs_subtitle" in process.stderr:
            typer.secho("PGS-Untertitel erkannt. Extrahiere diese separat...", fg=typer.colors.YELLOW)
            extract_pgs_subtitles(input_file, directory)
        else:
            typer.secho(f"Fehler bei der Konvertierung: {process.stderr}", fg=typer.colors.RED)
            raise typer.Exit(code=1)

    typer.secho(f"Konvertierung abgeschlossen: '{output_file}'", fg=typer.colors.GREEN)

    # Originaldatei löschen, falls nicht beibehalten
    if not keep_original:
        os.remove(input_file)
        typer.secho(f"Originaldatei '{input_file}' wurde gelöscht.", fg=typer.colors.GREEN)


def extract_pgs_subtitles(input_file: str, output_dir: str):
    """
    Extrahiert PGS-Untertitel aus der Eingabedatei.
    """
    base_name, _ = os.path.splitext(os.path.basename(input_file))
    output_file = os.path.join(output_dir, f"{base_name}_pgs.sup")

    cmd = [
        "ffmpeg", "-i", input_file,
        "-map", "0:s:0",  # Wähle den ersten Untertitel-Stream
        "-c:s", "copy",  # Keine Neukodierung der Untertitel
        output_file
    ]

    typer.secho(f"Extrahiere PGS-Untertitel nach '{output_file}'...", fg=typer.colors.BLUE)
    process = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)

    if process.returncode != 0:
        typer.secho(f"Fehler beim Extrahieren der Untertitel: {process.stderr}", fg=typer.colors.RED)
    else:
        typer.secho(f"Untertitel erfolgreich extrahiert: '{output_file}'", fg=typer.colors.GREEN)


def main():
    typer.run(convert_to_apple_compatible)


if __name__ == "__main__":
    main()