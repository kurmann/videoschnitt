# src/metadata_manager/cli/commands/export_metadata.py

import typer
import json
from pathlib import Path
from metadata_manager.aggregator import aggregate_metadata

def export_metadata(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten exportiert werden sollen"),
    output_path: Path = typer.Argument(..., help="Pfad zur Ausgabedatei (JSON oder TXT)"),
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Gibt die Quelle jeder Eigenschaft mit aus")
):
    """
    Exportiert die Metadaten einer Datei in eine JSON- oder Textdatei.
    """
    try:
        # Aggregiere Metadaten
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
                        # Formatierung der Bitrate in Mbps, falls gewünscht
                        if key == "Bitrate" and isinstance(value, (int, float)):
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

def register(app: typer.Typer):
    app.command()(export_metadata)