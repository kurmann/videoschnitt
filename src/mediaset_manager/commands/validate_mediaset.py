# mediaset_manager/commands/validate_mediaset.py

import typer
from pathlib import Path
from mediaset_manager.commands.validate_metadata import validate_metadata
from mediaset_manager.commands.validate_directory import validate_directory
import logging

app = typer.Typer()
logger = logging.getLogger(__name__)

@app.command("validate-mediaset")
def validate_mediaset(
    directory_path: Path = typer.Argument(..., help="Pfad zum Medienset-Verzeichnis")
):
    """
    Führt sowohl die Metadaten- als auch die Verzeichnisvalidierung durch.
    """
    metadata_path = directory_path / "Metadaten.yaml"
    
    typer.secho("Validierung der Metadaten.yaml...", fg=typer.colors.BLUE)
    try:
        validate_metadata(metadata_path)
    except typer.Exit:
        typer.secho("Metadatenvalidierung fehlgeschlagen.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    typer.secho("Validierung des Verzeichnisses...", fg=typer.colors.BLUE)
    try:
        validate_directory(directory_path)
    except typer.Exit:
        typer.secho("Verzeichnisvalidierung fehlgeschlagen.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Zusätzliche Überprüfungen der Verzeichnisstruktur basierend auf Metadaten
    # Beispielsweise Überprüfen von Filmfassungen und Versionen
    # Dies kann weiter ausgebaut werden
    
    typer.secho("Kombinierte Validierung abgeschlossen. Das Medienset ist gültig.", fg=typer.colors.GREEN)