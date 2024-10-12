# src/mediaset_manager/app.py

import typer
import logging
from mediaset_manager.commands.list_mediasets import list_mediasets_command
from metadata_manager.commands.get_resolution import get_resolution_command
from metadata_manager.commands.get_recording_date import get_recording_date_command

# Logging-Konfiguration
logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

app = typer.Typer()

# Registriere die Befehle direkt mit benannten Commands
app.command("list-mediasets")(list_mediasets_command)
app.command("get-resolution")(get_resolution_command)
app.command("get-recording-date")(get_recording_date_command)

if __name__ == "__main__":
    app()