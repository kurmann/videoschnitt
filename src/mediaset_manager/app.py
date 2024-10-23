# main.py

import typer
import logging

from mediaset_manager.commands.list_mediasets import list_mediasets_command
from mediaset_manager.commands.create_homemovie_metadata_file import create_homemovie_metadata_file
from mediaset_manager.commands.create_homemovie import create_homemovie
from mediaset_manager.commands.validate_metadata import validate_metadata
from mediaset_manager.commands.validate_directory import validate_directory
from mediaset_manager.commands.validate_mediaset import validate_mediaset

# Logging-Konfiguration
logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

app = typer.Typer()

# Registriere die verbleibenden Commands direkt mit benannten Commands
app.command("list-mediasets")(list_mediasets_command)
app.command("create-homemovie-metadata-file")(create_homemovie_metadata_file)
app.command("create-homemovie")(create_homemovie)
app.command("validate-metadata")(validate_metadata)
app.command("validate-directory")(validate_directory)
app.command("validate-mediaset")(validate_mediaset)

if __name__ == "__main__":
    app()