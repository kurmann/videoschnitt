import typer
import os
import subprocess
import logging
from rich.progress import Progress, SpinnerColumn, BarColumn, TimeElapsedColumn

logger = logging.getLogger(__name__)

app = typer.Typer()

def analyze_source(file_path: str) -> None:
    """Analysiere die Quelle und zeige Informationen über die Streams an."""
    typer.secho(f"Analysiere Quelle: '{file_path}'\n", fg=typer.colors.BLUE)
    
    cmd = [
        "ffprobe",
        "-v", "error",
        "-show_entries", "stream=index,codec_type,codec_name,channels,bit_rate,disposition",
        "-show_format",
        "-of", "json",
        file_path,
    ]

    try:
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        if result.returncode != 0:
            typer.secho("Fehler bei der Analyse der Quelle.", fg=typer.colors.RED)
            typer.secho(result.stderr, fg=typer.colors.RED)
            raise typer.Exit(code=1)

        import json
        data = json.loads(result.stdout)

        # Streams analysieren
        for stream in data.get("streams", []):
            index = stream.get("index")
            codec_type = stream.get("codec_type", "unknown")
            codec_name = stream.get("codec_name", "unknown")
            channels = stream.get("channels", "N/A")
            disposition = stream.get("disposition", {})
            
            action = "[Remux]" if codec_type in ["video", "subtitle"] else "[Konvertiere zu E-AC3]" if codec_type == "audio" and codec_name != "eac3" else "[Unbekannt]"
            
            if codec_type == "video":
                typer.echo(f"  - Video Stream {index}: {codec_name} {action}")
            elif codec_type == "audio":
                typer.echo(f"  - Audio Stream {index}: {codec_name} ({channels} Kanäle) {action}")
            elif codec_type == "subtitle":
                typer.echo(f"  - Untertitel Stream {index}: {codec_name} {action}")
        typer.echo()

    except Exception as e:
        typer.secho(f"Fehler bei der Analyse der Quelle: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

@app.command()
def convert_to_apple(
    input_file: str = typer.Argument(..., help="Pfad zur Eingabedatei"),
    delete_original: bool = typer.Option(False, "--delete-original", help="Löscht die Originaldatei nach erfolgreicher Konvertierung."),
):
    """
    Konvertiert eine Videodatei in ein Apple-kompatibles Format (MOV-Container mit PGS-Untertiteln).
    """
    if not os.path.isfile(input_file):
        typer.secho(f"Die Datei '{input_file}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    base_name, _ = os.path.splitext(input_file)
    output_file = f"{base_name}.mov"

    if os.path.exists(output_file):
        typer.secho(f"Die Ausgabedatei '{output_file}' existiert bereits. Überspringe Konvertierung.", fg=typer.colors.YELLOW)
        return

    # Analyse der Quelle
    analyze_source(input_file)

    typer.secho(f"Starte Konvertierung von '{input_file}' nach Apple-kompatiblem Format...", fg=typer.colors.BLUE)

    cmd = [
        "ffmpeg",
        "-y",
        "-i", input_file,
        "-map", "0",  # Alle Streams übernehmen
        "-c:v", "copy",  # Video remux
        "-c:a", "eac3",  # Audio konvertieren
        "-c:s", "copy",  # PGS-Untertitel remux
        output_file,
    ]

    with Progress(
        SpinnerColumn(),
        "[progress.description]{task.description}",
        BarColumn(),
        TimeElapsedColumn(),
        transient=True
    ) as progress:
        task = progress.add_task("[green]Konvertiere...", total=None)

        try:
            process = subprocess.run(
                cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True
            )
            progress.update(task, completed=100)

            if process.returncode != 0:
                typer.secho("Fehler bei der Konvertierung:", fg=typer.colors.RED)
                typer.secho(process.stderr, fg=typer.colors.RED)
                raise typer.Exit(code=1)

            typer.secho(f"Konvertierung abgeschlossen: '{output_file}'", fg=typer.colors.GREEN)

        except KeyboardInterrupt:
            typer.secho("Abbruchsignal erhalten. Konvertierung abgebrochen.", fg=typer.colors.RED)
            raise typer.Exit()

    if delete_original:
        os.remove(input_file)
        typer.secho(f"Originaldatei '{input_file}' wurde gelöscht.", fg=typer.colors.GREEN)
    else:
        delete_confirmation = typer.confirm(f"Möchten Sie die Originaldatei '{input_file}' löschen?")
        if delete_confirmation:
            os.remove(input_file)
            typer.secho(f"Originaldatei '{input_file}' wurde gelöscht.", fg=typer.colors.GREEN)


if __name__ == "__main__":
    app()