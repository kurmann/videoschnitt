# mediaset_manager/commands/validate_metadata.py

import typer
import yaml
import jsonschema
import requests
import logging
from pathlib import Path
from mediaset_manager.utils import sanitize_filename

app = typer.Typer()
logger = logging.getLogger(__name__)

@app.command("validate-metadata")
def validate_metadata(
    metadata_path: Path = typer.Argument(..., help="Pfad zur Metadaten.yaml Datei")
):
    """
    Validiert die Metadaten.yaml-Datei gegen das YAML-Schema.
    """
    if not metadata_path.is_file():
        typer.secho(f"Die Datei '{metadata_path}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    schema_url = "https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml"
    
    try:
        response = requests.get(schema_url)
        response.raise_for_status()
        schema = response.json()
    except requests.RequestException as e:
        typer.secho(f"Fehler beim Abrufen des Schemas: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    try:
        with open(metadata_path, 'r', encoding='utf-8') as file:
            metadata = yaml.safe_load(file)
    except yaml.YAMLError as e:
        typer.secho(f"Fehler beim Parsen der Metadaten.yaml: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    try:
        jsonschema.validate(instance=metadata, schema=schema)
        typer.secho("Metadaten.yaml ist gültig.", fg=typer.colors.GREEN)
    except jsonschema.exceptions.ValidationError as err:
        typer.secho("Metadaten.yaml ist ungültig:", fg=typer.colors.RED)
        typer.secho(str(err), fg=typer.colors.RED)
        raise typer.Exit(code=1)