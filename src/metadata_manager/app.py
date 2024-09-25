# src/metadata_manager/app.py

import typer
import json
from pathlib import Path
from metadata_manager.loader import load_metadata, get_metadata
from metadata_manager.parser import parse_recording_date
from metadata_manager.utils import get_video_codec, get_bitrate, is_hevc_a
from metadata_manager.exif import get_creation_datetime
from metadata_manager.models import Metadata
from datetime import datetime

app = typer.Typer(help="Metadata Manager CLI für Kurmann Videoschnitt")

@app.command("show-metadata")
def show_metadata(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen"),
    json_output: bool = typer.Option(False, "--json", "-j", help="Gebe die Metadaten im JSON-Format aus")
):
    """
    Zeigt die Metadaten einer Datei an.

    \b
    **Beispiele:**
        metadata-manager show-metadata /path/to/video.mov
        metadata-manager show-metadata /path/to/video.mov --json

    \b
    **Beispielausgabe (Text):**
        File name: video.mov
        Directory: /path/to/
        File size: 104857600
        File modification datetime: 2023-09-15 12:34:56
        File type: MOV
        Mime type: video/quicktime
        Create date: 2023-09-15 10:34:56
        Duration: 00:10:00
        Audio format: AAC
        Image width: 1920
        Image height: 1080
        Compressor id: com.apple.prores
        Compressor name: Apple ProRes 422
        Bit depth: 10
        Video frame rate: 29.97
        Title: Sample Video
        Album: Sample Album
        Description: This is a sample video file.
        Copyright: © 2023 Kurmann
        Author: John Doe
        Keywords: sample, video
        Avg bitrate: 100000000
        Producer: Jane Smith
        Studio: Kurmann Studios
        Producers: ['Jane Smith']
        Directors: ['John Doe']
        Published: 2023-09-15 10:34:56+02:00

    \b
    **Beispielausgabe (JSON):**
    {
        "file_name": "video.mov",
        "directory": "/path/to/",
        "file_size": 104857600,
        "file_modification_datetime": "2023-09-15T12:34:56",
        "file_type": "MOV",
        "mime_type": "video/quicktime",
        "create_date": "2023-09-15T10:34:56",
        "duration": "00:10:00",
        "audio_format": "AAC",
        "image_width": 1920,
        "image_height": 1080,
        "compressor_id": "com.apple.prores",
        "compressor_name": "Apple ProRes 422",
        "bit_depth": 10,
        "video_frame_rate": "29.97",
        "title": "Sample Video",
        "album": "Sample Album",
        "description": "This is a sample video file.",
        "copyright": "© 2023 Kurmann",
        "author": "John Doe",
        "keywords": "sample, video",
        "avg_bitrate": 100000000,
        "producer": "Jane Smith",
        "studio": "Kurmann Studios",
        "producers": ["Jane Smith"],
        "directors": ["John Doe"],
        "published": "2023-09-15T10:34:56+02:00"
    }
    """
    try:
        metadata = get_metadata(str(file_path))
        if json_output:
            # Ausgabe als JSON
            print(json.dumps(metadata.__dict__, indent=4, default=str))
        else:
            # Ausgabe als lesbarer Text
            for field, value in metadata.__dict__.items():
                print(f"{field.replace('_', ' ').capitalize()}: {value}")
                
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except ValueError as e:
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein unerwarteter Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("export-metadata")
def export_metadata(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten exportiert werden sollen"),
    output_path: Path = typer.Argument(..., help="Pfad zur Ausgabedatei (JSON oder TXT)"),
):
    """
    Exportiert die Metadaten einer Datei in eine JSON- oder Textdatei.

    \b
    **Beispiele:**
        metadata-manager export-metadata /path/to/video.mov /path/to/output.json
        metadata-manager export-metadata /path/to/video.mov /path/to/output.txt

    \b
    **Beispielausgabe (JSON):**
    {
        "file_name": "video.mov",
        "directory": "/path/to/",
        "file_size": 104857600,
        "file_modification_datetime": "2023-09-15T12:34:56",
        "file_type": "MOV",
        "mime_type": "video/quicktime",
        "create_date": "2023-09-15T10:34:56",
        "duration": "00:10:00",
        "audio_format": "AAC",
        "image_width": 1920,
        "image_height": 1080,
        "compressor_id": "com.apple.prores",
        "compressor_name": "Apple ProRes 422",
        "bit_depth": 10,
        "video_frame_rate": "29.97",
        "title": "Sample Video",
        "album": "Sample Album",
        "description": "This is a sample video file.",
        "copyright": "© 2023 Kurmann",
        "author": "John Doe",
        "keywords": "sample, video",
        "avg_bitrate": 100000000,
        "producer": "Jane Smith",
        "studio": "Kurmann Studios",
        "producers": ["Jane Smith"],
        "directors": ["John Doe"],
        "published": "2023-09-15T10:34:56+02:00"
    }

    \b
    **Beispielausgabe (TXT):**
    File name: video.mov
    Directory: /path/to/
    File size: 104857600
    File modification datetime: 2023-09-15 12:34:56
    File type: MOV
    Mime type: video/quicktime
    Create date: 2023-09-15 10:34:56
    Duration: 00:10:00
    Audio format: AAC
    Image width: 1920
    Image height: 1080
    Compressor id: com.apple.prores
    Compressor name: Apple ProRes 422
    Bit depth: 10
    Video frame rate: 29.97
    Title: Sample Video
    Album: Sample Album
    Description: This is a sample video file.
    Copyright: © 2023 Kurmann
    Author: John Doe
    Keywords: sample, video
    Avg bitrate: 100000000
    Producer: Jane Smith
    Studio: Kurmann Studios
    Producers: ['Jane Smith']
    Directors: ['John Doe']
    Published: 2023-09-15 10:34:56+02:00
    """
    try:
        metadata = get_metadata(str(file_path))
        
        # Bestimme das Ausgabeformat basierend auf der Dateiendung
        output_suffix = output_path.suffix.lower()
        if output_suffix == '.json':
            with open(output_path, 'w', encoding='utf-8') as f:
                json.dump(metadata.__dict__, f, indent=4, default=str)
            typer.secho(f"Metadaten erfolgreich als JSON exportiert nach: {output_path}", fg=typer.colors.GREEN)
        elif output_suffix in ['.txt', '.md']:
            with open(output_path, 'w', encoding='utf-8') as f:
                for field, value in metadata.__dict__.items():
                    f.write(f"{field.replace('_', ' ').capitalize()}: {value}\n")
            typer.secho(f"Metadaten erfolgreich als Text exportiert nach: {output_path}", fg=typer.colors.GREEN)
        else:
            typer.secho("Das Ausgabeformat wird nicht unterstützt. Bitte verwende .json oder .txt.", fg=typer.colors.RED)
    
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except ValueError as e:
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein unerwarteter Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("validate-file")
def validate_file(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, die validiert werden soll")
):
    """
    Validiert die Mediendatei, indem überprüft wird, ob die Metadaten erfolgreich geladen werden können.

    \b
    **Beispiel:**
        metadata-manager validate-file /path/to/video.mov

    \b
    **Beispielausgabe (Erfolgreich):**
        Die Datei wurde erfolgreich validiert. Metadaten konnten geladen werden.

    \b
    **Beispielausgabe (Fehlgeschlagen):**
        Die Datei '/path/to/video.mov' wurde nicht gefunden.
    """
    try:
        metadata = get_metadata(str(file_path))
        typer.secho("Die Datei wurde erfolgreich validiert. Metadaten konnten geladen werden.", fg=typer.colors.GREEN)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except ValueError as e:
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein unerwarteter Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("get-creation-datetime")
def cli_get_creation_datetime(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, deren Erstellungsdatum abgerufen werden soll")
):
    """
    Gibt das Erstellungsdatum einer Mediendatei zurück.

    \b
    **Beispiel:**
        metadata-manager get-creation-datetime /path/to/video.mov

    \b
    **Beispielausgabe (Datum gefunden):**
        Erstellungsdatum: 2023-09-15 12:34:56+02:00

    \b
    **Beispielausgabe (Datum nicht gefunden, Fallback):**
        Erstellungsdatum konnte nicht ermittelt werden.
    """
    try:
        creation_datetime = get_creation_datetime(str(file_path))
        if creation_datetime:
            print(f"Erstellungsdatum: {creation_datetime}")
        else:
            typer.secho("Erstellungsdatum konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("get-video-codec")
def cli_get_video_codec(
    file_path: Path = typer.Argument(..., help="Pfad zur Videodatei, deren Codec abgerufen werden soll")
):
    """
    Gibt den Videocodec einer Datei zurück.

    \b
    **Beispiel:**
        metadata-manager get-video-codec /path/to/video.mov

    \b
    **Beispielausgabe (Codec gefunden):**
        Videocodec: hevc

    \b
    **Beispielausgabe (Codec nicht gefunden):**
        Videocodec konnte nicht ermittelt werden.
    """
    try:
        codec = get_video_codec(str(file_path))
        if codec:
            print(f"Videocodec: {codec}")
        else:
            typer.secho("Videocodec konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("get-bitrate")
def cli_get_bitrate(
    file_path: Path = typer.Argument(..., help="Pfad zur Videodatei, deren Bitrate abgerufen werden soll")
):
    """
    Gibt die Bitrate einer Videodatei zurück.

    \b
    **Beispiel:**
        metadata-manager get-bitrate /path/to/video.mov

    \b
    **Beispielausgabe (Bitrate gefunden):**
        Bitrate: 100000000 bit/s (100.00 Mbit/s)

    \b
    **Beispielausgabe (Bitrate nicht gefunden):**
        Bitrate konnte nicht ermittelt werden.
    """
    try:
        bitrate = get_bitrate(str(file_path))
        if bitrate:
            bitrate_mbps = bitrate / (1024 * 1024)
            print(f"Bitrate: {bitrate} bit/s ({bitrate_mbps:.2f} Mbit/s)")
        else:
            typer.secho("Bitrate konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("is-hevc-a")
def cli_is_hevc_a(
    file_path: Path = typer.Argument(..., help="Pfad zur Videodatei, die überprüft werden soll")
):
    """
    Überprüft, ob eine Videodatei HEVC-A ist (Bitrate > 80 Mbit/s).

    \b
    **Beispiel:**
        metadata-manager is-hevc-a /path/to/video.mov

    \b
    **Beispielausgabe (HEVC-A):**
        Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).

    \b
    **Beispielausgabe (Nicht HEVC-A):**
        Die Datei ist nicht HEVC-A (Bitrate <= 80 Mbit/s).
    """
    try:
        hevc_a = is_hevc_a(str(file_path))
        if hevc_a:
            typer.secho("Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).", fg=typer.colors.GREEN)
        else:
            typer.secho("Die Datei ist nicht HEVC-A (Bitrate <= 80 Mbit/s).", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

if __name__ == "__main__":
    app()
