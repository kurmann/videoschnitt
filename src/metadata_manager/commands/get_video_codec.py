import subprocess
import json
from typing import Optional
import typer
import logging
import os

logger = logging.getLogger(__name__)

def get_video_codec(file_path: str) -> Optional[str]:
    """
    Führt ffprobe aus, um den Codec der Videodatei zu ermitteln.
    
    Args:
        file_path (str): Der Pfad zur Videodatei.
    
    Returns:
        Optional[str]: Der Videocodec, z.B. 'prores', oder None, wenn nicht ermittelt werden konnte.
    """
    
    # Überprüfen, ob die Datei existiert
    if not os.path.isfile(file_path):
        logger.error(f"Die Datei '{file_path}' existiert nicht.")
        typer.secho(f"Fehler: Die Datei '{file_path}' konnte nicht gefunden werden.", fg=typer.colors.RED)
        return None

    try:
        cmd = [
            'ffprobe',
            '-v', 'error',
            '-select_streams', 'v:0',
            '-show_entries', 'stream=codec_name',
            '-of', 'json',
            file_path
        ]
        logger.debug(f"Führe ffprobe mit folgendem Befehl aus: {' '.join(cmd)}")
        result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        
        logger.debug(f"ffprobe Return Code: {result.returncode}")
        logger.debug(f"ffprobe stderr: {result.stderr.strip()}")
        logger.debug(f"ffprobe stdout: {result.stdout.strip()}")
        
        if result.returncode != 0:
            logger.error(f"ffprobe Fehler beim Lesen des Videocodecs: {result.stderr.strip()}")
            typer.secho(f"ffprobe Fehler beim Lesen des Videocodecs für {file_path}: {result.stderr.strip()}", fg=typer.colors.RED)
            return None
        
        exif_json = json.loads(result.stdout.strip())
        logger.debug(f"Parsed JSON: {exif_json}")
        
        if exif_json and 'streams' in exif_json and len(exif_json['streams']) > 0:
            codec = exif_json['streams'][0].get('codec_name')
            if codec:
                logger.debug(f"Ermittelter Videocodec für {file_path}: {codec}")
                return codec
            else:
                logger.error(f"codec_name nicht gefunden in den ffprobe-Ausgaben für {file_path}.")
        else:
            logger.error(f"Keine Streams gefunden in den ffprobe-Ausgaben für {file_path}.")
    except json.JSONDecodeError as jde:
        logger.error(f"JSON-Parsing-Fehler: {jde}")
        typer.secho(f"JSON-Parsing-Fehler beim Lesen des Videocodecs für {file_path}: {jde}", fg=typer.colors.RED)
    except Exception as e:
        logger.error(f"Fehler beim Abrufen des Videocodecs mit ffprobe: {e}")
        typer.secho(f"Fehler beim Abrufen des Videocodecs für {file_path}: {e}", fg=typer.colors.RED)
    
    return None

def get_video_codec_command(
    file_path: str = typer.Argument(..., help="Pfad zur MOV-Datei, um den Videocodec auszulesen")
):
    """
    Gibt den Videocodec einer MOV-Datei aus.
    """
    codec = get_video_codec(file_path)
    if codec:
        typer.secho(f"Videocodec: {codec}", fg=typer.colors.GREEN)
    else:
        typer.secho("Videocodec konnte nicht ermittelt werden.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

if __name__ == "__main__":
    typer.run(get_video_codec_command)