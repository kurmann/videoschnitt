# src/metadata_manager/cli/commands/validate_file.py

import typer
from pathlib import Path
from metadata_manager.aggregator import aggregate_metadata

def validate_file(
    file_path: Path = typer.Argument(..., help="Pfad zur Mediendatei, die validiert werden soll"),
    include_source: bool = typer.Option(False, "--include-source", "-s", help="Überprüft die Quelle der Metadaten")
):
    """
    Validiert die Mediendatei, indem überprüft wird, ob die Metadaten erfolgreich geladen werden können.
    """
    try:
        # Aggregiere Metadaten
        metadata = aggregate_metadata(str(file_path), include_source=include_source)
        typer.secho("Die Datei wurde erfolgreich validiert. Metadaten konnten geladen werden.", fg=typer.colors.GREEN)
    except FileNotFoundError:
        typer.secho(f"Die Datei '{file_path}' wurde nicht gefunden.", fg=typer.colors.RED)
    except ValueError as e:
        typer.secho(str(e), fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein unerwarteter Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command()(validate_file)