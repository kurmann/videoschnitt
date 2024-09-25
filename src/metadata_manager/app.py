# src/metadata_manager/app.py

import typer
import json
from pathlib import Path
from metadata_manager.loader import load_metadata
from metadata_manager.parser import parse_recording_date
from metadata_manager.models import Metadata

app = typer.Typer(help="Metadata Manager CLI für Kurmann Videoschnitt")

@app.command("show-metadata")
def show_metadata(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen"),
    json_output: bool = typer.Option(False, "--json", "-j", help="Gebe die Metadaten im JSON-Format aus")
):
    """
    Zeigt die Metadaten einer Datei an.

    \b
    Beispiele:
        metadata-manager show-metadata /path/to/video.mov
        metadata-manager show-metadata /path/to/video.mov --json
    """
    try:
        metadata_dict = load_metadata(str(file_path))
        recording_date = parse_recording_date(metadata_dict)
        metadata = Metadata(
            file_name=metadata_dict.get("FileName", ""),
            directory=metadata_dict.get("Directory", ""),
            file_size=int(metadata_dict.get("FileSize", 0)),
            file_modification_datetime=metadata_dict.get("FileModificationDateTime", ""),
            file_type=metadata_dict.get("FileType", ""),
            mime_type=metadata_dict.get("MIMEType", ""),
            create_date=metadata_dict.get("CreateDate"),
            duration=metadata_dict.get("Duration"),
            audio_format=metadata_dict.get("AudioFormat"),
            image_width=int(metadata_dict.get("ImageWidth", 0)),
            image_height=int(metadata_dict.get("ImageHeight", 0)),
            compressor_id=metadata_dict.get("CompressorID"),
            compressor_name=metadata_dict.get("CompressorName"),
            bit_depth=int(metadata_dict.get("BitDepth", 0)) if metadata_dict.get("BitDepth") else None,
            video_frame_rate=metadata_dict.get("VideoFrameRate"),
            title=metadata_dict.get("Title"),
            album=metadata_dict.get("Album"),
            description=metadata_dict.get("Description"),
            copyright=metadata_dict.get("Copyright"),
            author=metadata_dict.get("Author"),
            keywords=metadata_dict.get("Keywords"),
            avg_bitrate=int(metadata_dict.get("AvgBitrate", 0)) if metadata_dict.get("AvgBitrate") else None,
            producer=metadata_dict.get("Producer"),
            studio=metadata_dict.get("Studio"),
            producers=metadata_dict.get("Producers", []),
            directors=metadata_dict.get("Directors", [])
        )
        
        # Aktualisiere das Veröffentlichungsdatum mit dem Aufnahmedatum, falls vorhanden
        if recording_date:
            metadata.published = recording_date
        
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
    Beispiele:
        metadata-manager export-metadata /path/to/video.mov /path/to/output.json
        metadata-manager export-metadata /path/to/video.mov /path/to/output.txt
    """
    try:
        metadata_dict = load_metadata(str(file_path))
        recording_date = parse_recording_date(metadata_dict)
        metadata = Metadata(
            file_name=metadata_dict.get("FileName", ""),
            directory=metadata_dict.get("Directory", ""),
            file_size=int(metadata_dict.get("FileSize", 0)),
            file_modification_datetime=metadata_dict.get("FileModificationDateTime", ""),
            file_type=metadata_dict.get("FileType", ""),
            mime_type=metadata_dict.get("MIMEType", ""),
            create_date=metadata_dict.get("CreateDate"),
            duration=metadata_dict.get("Duration"),
            audio_format=metadata_dict.get("AudioFormat"),
            image_width=int(metadata_dict.get("ImageWidth", 0)),
            image_height=int(metadata_dict.get("ImageHeight", 0)),
            compressor_id=metadata_dict.get("CompressorID"),
            compressor_name=metadata_dict.get("CompressorName"),
            bit_depth=int(metadata_dict.get("BitDepth", 0)) if metadata_dict.get("BitDepth") else None,
            video_frame_rate=metadata_dict.get("VideoFrameRate"),
            title=metadata_dict.get("Title"),
            album=metadata_dict.get("Album"),
            description=metadata_dict.get("Description"),
            copyright=metadata_dict.get("Copyright"),
            author=metadata_dict.get("Author"),
            keywords=metadata_dict.get("Keywords"),
            avg_bitrate=int(metadata_dict.get("AvgBitrate", 0)) if metadata_dict.get("AvgBitrate") else None,
            producer=metadata_dict.get("Producer"),
            studio=metadata_dict.get("Studio"),
            producers=metadata_dict.get("Producers", []),
            directors=metadata_dict.get("Directors", [])
        )
        
        # Aktualisiere das Veröffentlichungsdatum mit dem Aufnahmedatum, falls vorhanden
        if recording_date:
            metadata.published = recording_date
        
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
    
@app.command("validate-file")
def validate_file(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, die validiert werden soll")
):
    """
    Validiert die Mediendatei, indem überprüft wird, ob die Metadaten erfolgreich geladen werden können.

    \b
    Beispiele:
        metadata-manager validate-file /path/to/video.mov
    """
    try:
        metadata_dict = load_metadata(str(file_path))
        print("Die Datei wurde erfolgreich validiert. Metadaten konnten geladen werden.")
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except ValueError as e:
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein unerwarteter Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

if __name__ == "__main__":
    app()
