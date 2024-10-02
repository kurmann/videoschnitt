# src/emby_integrator/app.py

import typer
from emby_integrator.commands import register_commands

app = typer.Typer(help="Emby Integrator")

# Registriere alle Befehle
register_commands(app)

if __name__ == '__main__':
    app()