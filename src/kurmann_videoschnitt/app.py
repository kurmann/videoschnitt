# kurmann_videoschnitt/app.py

import typer
from apple_compressor_manager.app import app as compressor
from original_media_integrator.app import app as original_media_integrator
from emby_integrator.app import app as emby_integrator


app = typer.Typer(help="Kurmann Videoschnitt CLI")
app.add_typer(compressor, name="compressor")
app.add_typer(original_media_integrator, name="original-media")
app.add_typer(emby_integrator, name="emby")

if __name__ == "__main__":
    app()