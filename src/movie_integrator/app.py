# src/movie_integrator/app.py

import typer
import logging
from movie_integrator.commands.integrate_to_library import integrate_to_library_command

# Logging-Konfiguration
logging.basicConfig(level=logging.INFO, format='%(levelname)s: %(message)s')

app = typer.Typer()

# Registriere die Befehle direkt mit benannten Commands
app.command("integrate-to-library")(integrate_to_library_command)

if __name__ == "__main__":
    app()