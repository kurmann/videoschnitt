#src/fcp_integrator/app.py

import typer
from fcp_integrator.commands.fcp_workflow import run_workflow
from fcp_integrator.commands.convert_images import convert_images

app = typer.Typer(help="Final Cut Pro Integrator")

# Registriere alle Befehle
app.command("run-workflow")(run_workflow)
app.command("convert-images")(convert_images)

if __name__ == '__main__':
    app()