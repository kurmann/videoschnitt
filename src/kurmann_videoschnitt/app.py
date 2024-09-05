import typer
from apple_compressor_manager import app as apple_compressor_manager_app
from original_media_integrator import app as original_media_integrator_app
from emby_integrator import app as emby_integrator_app

# Erstelle eine Typer-Anwendung
app = typer.Typer(help="Kurmann Videoschnitt - Zentrale CLI")

# Integriere die Sub-Commands
app.add_typer(apple_compressor_manager_app, name="compressor")
app.add_typer(original_media_integrator_app, name='integrator')
app.add_typer(emby_integrator_app, name='emby')

if __name__ == "__main__":
    app()
