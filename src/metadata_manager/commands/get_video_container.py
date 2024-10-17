# src/metadata_manager/commands/get_video_container.py

import subprocess
import typer

def get_video_container_command(file_path: str):
    """
    Ermittelt den Container-Typ einer Videodatei mittels ffprobe und gibt ihn aus.
    """
    cmd = [
        'ffprobe',
        '-v', 'error',
        '-show_entries', 'format=format_name',
        '-of', 'default=noprint_wrappers=1:nokey=1',
        file_path
    ]
    try:
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        if result.returncode == 0:
            format_name = result.stdout.strip()
            # ffprobe gibt bei WebM oft 'matroska,webm' zurück
            container = format_name.split(',')[0]
            typer.echo(f"Container-Typ ist: {container}")
        else:
            typer.secho(f"Konnte den Container-Typ der Datei '{file_path}' nicht ermitteln.", fg=typer.colors.RED, err=True)
            raise typer.Exit(code=1)
    except Exception as e:
        typer.secho(f"Fehler beim Ermitteln des Container-Typs: {e}", fg=typer.colors.RED, err=True)
        raise typer.Exit(code=1)