#src/iclouddrive_integrator/app.py

import typer
from iclouddrive_integrator.commands.homemovie_integrator import integrate_homemovie, integrate_homemovies

app = typer.Typer(help="iCloud Integrator")

# Registriere alle Befehle
app.command()(integrate_homemovie)
app.command()(integrate_homemovies)

if __name__ == '__main__':
    app()