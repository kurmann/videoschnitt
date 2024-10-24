# main.py

import typer
import logging

from mediaset_manager.commands.create_homemovie import create_homemovie
from mediaset_manager.commands.auto_create_homemovies import auto_create_homemovies
from mediaset_manager.commands.integrate_mediaset import integrate_mediaset_command

# Logging-Konfiguration
logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

app = typer.Typer()

# Registriere die verbleibenden Commands direkt mit benannten Commands
app.command("create-homemovie")(create_homemovie)
app.command("auto-create-homemovies")(auto_create_homemovies)
app.command("integrate-mediaset")(integrate_mediaset_command)

if __name__ == "__main__":
    app()