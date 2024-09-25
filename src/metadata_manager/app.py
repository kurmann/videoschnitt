# src/metadata_manager/app.py

import typer
import json
from pathlib import Path
from metadata_manager import get_relevant_metadata
from metadata_manager.utils import get_video_codec, get_bitrate, is_hevc_a
from metadata_manager.exif import get_creation_datetime
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
        FileName: video.mov
        Directory: /path/to/
        FileSize: 10 GB
        FileModifyDate: 2024:09:19 17:03:53+02:00
        FileType: MOV
        MIMEType: video/quicktime
        CreateDate: 2024:09:19 14:29:04
        Duration: 0:14:47
        AudioFormat: mp4a
        ImageWidth: 3840
        ImageHeight: 2160
        CompressorID: hvc1
        CompressorName: HEVC
        BitDepth: 24
        VideoFrameRate: 60
        Title: 2024-09-15 Paula MTB-Finale Huttwil
        Album: Familie Kurmann
        Description: Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
        Copyright: © Patrick Kurmann 2024
        Author: Patrick Kurmann
        Keywords: 17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann
        AvgBitrate: 90.7 Mbps
        Producer: Patrick Kurmann
        Studio: Kurmann Studios

    \b
    **Beispielausgabe (JSON):**
    {
        "FileName": "video.mov",
        "Directory": "/path/to/",
        "FileSize": "10 GB",
        "FileModifyDate": "2024:09:19 17:03:53+02:00",
        "FileType": "MOV",
        "MIMEType": "video/quicktime",
        "CreateDate": "2024:09:19 14:29:04",
        "Duration": "0:14:47",
        "AudioFormat": "mp4a",
        "ImageWidth": "3840",
        "ImageHeight": "2160",
        "CompressorID": "hvc1",
        "CompressorName": "HEVC",
        "BitDepth": "24",
        "VideoFrameRate": "60",
        "Title": "2024-09-15 Paula MTB-Finale Huttwil",
        "Album": "Familie Kurmann",
        "Description": "Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.",
        "Copyright": "© Patrick Kurmann 2024",
        "Author": "Patrick Kurmann",
        "Keywords": "17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann",
        "AvgBitrate": "90.7 Mbps",
        "Producer": "Patrick Kurmann",
        "Studio": "Kurmann Studios"
    }
    """
    try:
        metadata = get_relevant_metadata(str(file_path))
        if json_output:
            # Ausgabe als JSON
            print(json.dumps(metadata, indent=4, ensure_ascii=False))
        else:
            # Ausgabe als lesbarer Text
            for key, value in metadata.items():
                print(f"{key}: {value}")
                    
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
        "FileName": "video.mov",
        "Directory": "/path/to/",
        "FileSize": "10 GB",
        "FileModifyDate": "2024:09:19 17:03:53+02:00",
        "FileType": "MOV",
        "MIMEType": "video/quicktime",
        "CreateDate": "2024:09:19 14:29:04",
        "Duration": "0:14:47",
        "AudioFormat": "mp4a",
        "ImageWidth": "3840",
        "ImageHeight": "2160",
        "CompressorID": "hvc1",
        "CompressorName": "HEVC",
        "BitDepth": "24",
        "VideoFrameRate": "60",
        "Title": "2024-09-15 Paula MTB-Finale Huttwil",
        "Album": "Familie Kurmann",
        "Description": "Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.",
        "Copyright": "© Patrick Kurmann 2024",
        "Author": "Patrick Kurmann",
        "Keywords": "17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann",
        "AvgBitrate": "90.7 Mbps",
        "Producer": "Patrick Kurmann",
        "Studio": "Kurmann Studios"
    }

    \b
    **Beispielausgabe (TXT):**
    FileName: video.mov
    Directory: /path/to/
    FileSize: 10 GB
    FileModifyDate: 2024:09:19 17:03:53+02:00
    FileType: MOV
    MIMEType: video/quicktime
    CreateDate: 2024:09:19 14:29:04
    Duration: 0:14:47
    AudioFormat: mp4a
    ImageWidth: 3840
    ImageHeight: 2160
    CompressorID: hvc1
    CompressorName: HEVC
    BitDepth: 24
    VideoFrameRate: 60
    Title: 2024-09-15 Paula MTB-Finale Huttwil
    Album: Familie Kurmann
    Description: Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
    Copyright: © Patrick Kurmann 2024
    Author: Patrick Kurmann
    Keywords: 17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann
    AvgBitrate: 90.7 Mbps
    Producer: Patrick Kurmann
    Studio: Kurmann Studios
    """
    try:
        metadata = get_relevant_metadata(str(file_path))
        
        # Bestimme das Ausgabeformat basierend auf der Dateiendung
        output_suffix = output_path.suffix.lower()
        if output_suffix == '.json':
            with open(output_path, 'w', encoding='utf-8') as f:
                json.dump(metadata, f, indent=4, ensure_ascii=False)
            typer.secho(f"Metadaten erfolgreich als JSON exportiert nach: {output_path}", fg=typer.colors.GREEN)
        elif output_suffix in ['.txt', '.md']:
            with open(output_path, 'w', encoding='utf-8') as f:
                for key, value in metadata.items():
                    f.write(f"{key}: {value}\n")
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
        metadata = get_relevant_metadata(str(file_path))
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
        Erstellungsdatum: 2024-09-19 14:29:04

    \b
    **Beispielausgabe (Datum nicht gefunden, Fallback):**
        Erstellungsdatum konnte nicht ermittelt werden.
    """
    try:
        metadata = get_relevant_metadata(str(file_path))
        create_date_str = metadata.get("CreateDate")
        if create_date_str:
            try:
                creation_datetime = datetime.strptime(create_date_str, "%Y:%m:%d %H:%M:%S")
                print(f"Erstellungsdatum: {creation_datetime}")
            except ValueError:
                typer.secho("Erstellungsdatum konnte nicht geparst werden.", fg=typer.colors.YELLOW)
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
        Videocodec: HEVC

    \b
    **Beispielausgabe (Codec nicht gefunden):**
        Videocodec konnte nicht ermittelt werden.
    """
    try:
        metadata = get_relevant_metadata(str(file_path))
        codec = metadata.get("CompressorName")
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
        Bitrate: 90.7 Mbps

    \b
    **Beispielausgabe (Bitrate nicht gefunden):**
        Bitrate konnte nicht ermittelt werden.
    """
    try:
        metadata = get_relevant_metadata(str(file_path))
        avg_bitrate = metadata.get("AvgBitrate")
        if avg_bitrate:
            print(f"Bitrate: {avg_bitrate}")
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
        metadata = get_relevant_metadata(str(file_path))
        avg_bitrate_str = metadata.get("AvgBitrate")
        if avg_bitrate_str and "Mbps" in avg_bitrate_str:
            try:
                bitrate_value = float(avg_bitrate_str.split()[0])
                if bitrate_value > 80:
                    typer.secho("Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).", fg=typer.colors.GREEN)
                else:
                    typer.secho("Die Datei ist nicht HEVC-A (Bitrate <= 80 Mbit/s).", fg=typer.colors.YELLOW)
            except ValueError:
                typer.secho("Bitrate konnte nicht geparst werden.", fg=typer.colors.YELLOW)
        else:
            typer.secho("Bitrate konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

if __name__ == "__main__":
    app()