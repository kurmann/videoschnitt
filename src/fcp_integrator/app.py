#src/fcp_integrator/app.py

import typer
from fcp_integrator.commands.fcp_workflow import run_workflow

app = typer.Typer(help="Final Cut Pro Integrator")

# Registriere alle Befehle
app.command("run")(run_workflow)

if __name__ == '__main__':
    app()