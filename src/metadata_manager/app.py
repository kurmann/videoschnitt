# src/metadata_manager/app.py

import typer
import json
from pathlib import Path
from metadata_manager import aggregate_metadata
from metadata_manager.loader import get_metadata_with_exiftool
from metadata_manager.utils import get_metadata_with_ffmpeg
from metadata_manager.utils import get_video_codec, get_bitrate, is_hevc_a
from metadata_manager.exif import get_album, get_creation_datetime
from metadata_manager.commands.get_recording_date import get_recording_date_command
from metadata_manager.commands.get_title import get_title_command

app = typer.Typer(help="Metadata Manager CLI für Kurmann Videoschnitt")

app.command("get-recording-date")(get_recording_date_command)
app.command("get-title")(get_title_command)

@app.command("show-metadata")
def show_metadata(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen"),
    json_output: bool = typer.Option(False, "--json", "-j", help="Gebe die Metadaten im JSON-Format aus"),
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Gibt die Quelle jeder Eigenschaft mit aus")
):
    """
    Zeigt die Metadaten einer Datei an.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Mediendatei.
    - **json_output** (*bool*): Wenn gesetzt, werden die Metadaten im JSON-Format ausgegeben.
    - **include_source** (*bool*): Wenn gesetzt, werden die Quellen der Metadaten ebenfalls angezeigt.

    ## Beispielaufrufe:
    ```bash
    metadata-manager show-metadata /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    FileName: Datei.mov
    Bitrate: 1069.47 Mbps
    VideoCodec: prores
    ```

    Mit JSON-Option:
    ```bash
    metadata-manager show-metadata /Pfad/zur/Datei.mov --json
    ```

    Ausgabe im JSON-Format:
    ```json
    {
        "FileName": "Datei.mov",
        "Bitrate": "1069.47 Mbps",
        "VideoCodec": "prores"
    }
    ```
    """
    try:
        # Immer aggregate_metadata aufrufen, um alle Metadaten zu erhalten
        metadata = aggregate_metadata(str(file_path), include_source=include_source)
        
        if json_output:
            print(json.dumps(metadata, indent=4, ensure_ascii=False))
        else:
            for key, value in metadata.items():
                if include_source and isinstance(value, dict) and 'value' in value and 'source' in value:
                    print(f"{key}: {value['value']} (Source: {value['source']})")
                else:
                    # Optional: Formatierung der Bitrate in Mbps, falls gewünscht
                    if key == "Bitrate" and value and isinstance(value, (int, float)):
                        try:
                            bitrate_mbps = float(value) / 1_000_000
                            print(f"{key}: {bitrate_mbps:.2f} Mbps")
                        except ValueError:
                            print(f"{key}: {value}")
                    else:
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
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Gibt die Quelle jeder Eigenschaft mit aus")
):
    """
    Exportiert die Metadaten einer Datei in eine JSON- oder Textdatei.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Mediendatei.
    - **output_path** (*Path*): Pfad zur Ausgabedatei (unterstützt .json, .txt, .md).
    - **include_source** (*bool*): Wenn gesetzt, werden die Quellen der Metadaten ebenfalls in die Datei geschrieben.

    ## Beispielaufrufe:
    ```bash
    metadata-manager export-metadata /Pfad/zur/Datei.mov /Pfad/zur/Ausgabedatei.json
    ```

    Ausgabe:
    ```plaintext
    Metadaten erfolgreich als JSON exportiert nach: /Pfad/zur/Ausgabedatei.json
    ```

    Mit Text-Export:
    ```bash
    metadata-manager export-metadata /Pfad/zur/Datei.mov /Pfad/zur/Ausgabedatei.txt
    ```

    Ausgabe:
    ```plaintext
    Metadaten erfolgreich als Text exportiert nach: /Pfad/zur/Ausgabedatei.txt
    ```
    """
    try:
        # Immer aggregate_metadata aufrufen, um alle Metadaten zu erhalten
        metadata = aggregate_metadata(str(file_path), include_source=include_source)
        
        # Bestimme das Ausgabeformat basierend auf der Dateiendung
        output_suffix = output_path.suffix.lower()
        if output_suffix == '.json':
            with open(output_path, 'w', encoding='utf-8') as f:
                json.dump(metadata, f, indent=4, ensure_ascii=False)
            typer.secho(f"Metadaten erfolgreich als JSON exportiert nach: {output_path}", fg=typer.colors.GREEN)
        elif output_suffix in ['.txt', '.md']:
            with open(output_path, 'w', encoding='utf-8') as f:
                for key, value in metadata.items():
                    if include_source and isinstance(value, dict) and 'value' in value and 'source' in value:
                        f.write(f"{key}: {value['value']} (Source: {value['source']})\n")
                    else:
                        # Optional: Formatierung der Bitrate in Mbps, falls gewünscht
                        if key == "Bitrate" and value and isinstance(value, (int, float)):
                            try:
                                bitrate_mbps = float(value) / 1_000_000
                                f.write(f"{key}: {bitrate_mbps:.2f} Mbps\n")
                            except ValueError:
                                f.write(f"{key}: {value}\n")
                        else:
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
        
@app.command("show-metadata-with-exiftool")
def show_metadata_with_exiftool(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen"),
    json_output: bool = typer.Option(False, "--json", "-j", help="Gebe die Metadaten im JSON-Format aus")
):
    """
    Zeigt die Metadaten einer Datei an, ermittelt mit ExifTool.
    """
    try:
        metadata = get_metadata_with_exiftool(str(file_path))
        if json_output:
            print(json.dumps(metadata, indent=4, ensure_ascii=False))
        else:
            for key, value in metadata.items():
                print(f"{key}: {value}")
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("show-metadata-with-ffmpeg")
def show_metadata_with_ffmpeg(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen"),
    json_output: bool = typer.Option(False, "--json", "-j", help="Gebe die Metadaten im JSON-Format aus")
):
    """
    Zeigt die Metadaten einer Datei an, ermittelt mit FFmpeg/FFprobe.
    """
    try:
        metadata = get_metadata_with_ffmpeg(str(file_path))
        if json_output:
            print(json.dumps(metadata, indent=4, ensure_ascii=False))
        else:
            for key, value in metadata.items():
                if key == "Bitrate" and value and isinstance(value, (int, float)):
                    bitrate_mbps = float(value) / 1_000_000
                    print(f"{key}: {bitrate_mbps:.2f} Mbps")
                else:
                    print(f"{key}: {value}")
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

@app.command("validate-file")
def validate_file(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, die validiert werden soll"),
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Überprüft die Quelle der Metadaten")
):
    """
    Validiert die Mediendatei, indem überprüft wird, ob die Metadaten erfolgreich geladen werden können.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Mediendatei, die validiert werden soll.
    - **include_source** (*bool*): Wenn gesetzt, wird die Quelle der Metadaten ebenfalls überprüft.

    ## Beispielaufruf:
    ```bash
    metadata-manager validate-file /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Die Datei wurde erfolgreich validiert. Metadaten konnten geladen werden.
    ```
    """
    try:
        # Immer aggregate_metadata aufrufen, um alle Metadaten zu erhalten
        metadata = aggregate_metadata(str(file_path), include_source=include_source)
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

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Mediendatei, deren Erstellungsdatum abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-creation-datetime /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Erstellungsdatum: 2024-09-23T19:16:33+02:00
    ```
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
        
@app.command("get-album")
def cli_get_album(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, deren Album-Tag abgerufen werden soll")
):
    """
    Gibt den Album-Tag einer Mediendatei zurück.

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Mediendatei, deren Album-Tag abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-album /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Album: MeinAlbum
    ```
    """
    try:
        album = get_album(str(file_path))
        if album:
            print(f"Album: {album}")
        else:
            typer.secho("Album-Tag konnte nicht ermittelt werden.", fg=typer.colors.YELLOW)
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

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Videodatei, deren Videocodec abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-video-codec /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Videocodec: prores
    ```
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

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Videodatei, deren Bitrate abgerufen werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager get-bitrate /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Bitrate: 1069.47 Mbps
    ```
    """
    try:
        bitrate = get_bitrate(str(file_path))
        if bitrate:
            # Umrechnung der Bitrate in Mbps für die Ausgabe
            bitrate_mbps = float(bitrate) / 1_000_000
            print(f"Bitrate: {bitrate_mbps:.2f} Mbps")
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

    ## Argumente:
    - **file_path** (*Path*): Pfad zur Videodatei, die überprüft werden soll.

    ## Beispielaufruf:
    ```bash
    metadata-manager is-hevc-a /Pfad/zur/Datei.mov
    ```

    Ausgabe:
    ```plaintext
    Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).
    ```
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