# src/metadata_manager/cli/commands/show_metadata.py

import typer
import json
from pathlib import Path
from metadata_manager.aggregator import aggregate_metadata

def show_metadata(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen"),
    json_output: bool = typer.Option(False, "--json", "-j", help="Gebe die Metadaten im JSON-Format aus"),
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Gibt die Quelle jeder Eigenschaft mit aus")
):
    """
    Zeigt die Metadaten einer Datei an.
    """
    try:
        # Aggregiere Metadaten
        metadata = aggregate_metadata(str(file_path), include_source=include_source)
        
        if json_output:
            typer.echo(json.dumps(metadata, indent=4, ensure_ascii=False))
        else:
            for key, value in metadata.items():
                if include_source and isinstance(value, dict) and 'value' in value and 'source' in value:
                    typer.echo(f"{key}: {value['value']} (Source: {value['source']})")
                else:
                    # Formatierung der Bitrate in Mbps, falls gewünscht
                    if key == "Bitrate" and isinstance(value, (int, float)):
                        try:
                            bitrate_mbps = float(value) / 1_000_000
                            typer.echo(f"{key}: {bitrate_mbps:.2f} Mbps")
                        except ValueError:
                            typer.echo(f"{key}: {value}")
                    else:
                        typer.echo(f"{key}: {value}")
                            
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except ValueError as e:
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein unerwarteter Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command()(show_metadata)