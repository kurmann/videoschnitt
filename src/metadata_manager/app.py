# src/metadata_manager/app.py

import typer
import json
from pathlib import Path
from metadata_manager import aggregate_metadata
from metadata_manager.utils import get_video_codec, get_bitrate, is_hevc_a
from metadata_manager.exif import get_creation_datetime
from datetime import datetime

app = typer.Typer(help="Metadata Manager CLI für Kurmann Videoschnitt")

@app.command("show-metadata")
def show_metadata(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen"),
    json_output: bool = typer.Option(False, "--json", "-j", help="Gebe die Metadaten im JSON-Format aus"),
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Gibt die Quelle jeder Eigenschaft mit aus")
):
    """
    Zeigt die Metadaten einer Datei an.

    ... [Bestehende Dokumentation bleibt unverändert] ...
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

    ... [Bestehende Dokumentation bleibt unverändert] ...
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

@app.command("validate-file")
def validate_file(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, die validiert werden soll"),
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Überprüft die Quelle der Metadaten")
):
    """
    Validiert die Mediendatei, indem überprüft wird, ob die Metadaten erfolgreich geladen werden können.

    ... [Bestehende Dokumentation bleibt unverändert] ...
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

    ... [Bestehende Dokumentation bleibt unverändert] ...
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

    ... [Bestehende Dokumentation bleibt unverändert] ...
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

    ... [Bestehende Dokumentation bleibt unverändert] ...
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

    ... [Bestehende Dokumentation bleibt unverändert] ...
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