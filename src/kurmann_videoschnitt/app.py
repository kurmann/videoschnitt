# kurmann_videoschnitt/app.py

import typer
import os
import toml  # Stellen Sie sicher, dass das toml-Modul installiert ist
from apple_compressor_manager.app import app as compressor
from original_media_integrator.app import app as original_media_integrator
from emby_integrator.app import app as emby_integrator

def get_version():
    """
    Liest die Version aus der pyproject.toml Datei aus.
    """
    # Bestimmen Sie den Pfad zur pyproject.toml Datei
    current_dir = os.path.dirname(os.path.abspath(__file__))
    # Gehen Sie zwei Verzeichnisse nach oben, um das Projektverzeichnis zu erreichen
    project_dir = os.path.abspath(os.path.join(current_dir, '..', '..'))
    pyproject_path = os.path.join(project_dir, 'pyproject.toml')
    if not os.path.exists(pyproject_path):
        return '0.0.0'  # Standardversion, falls die Datei nicht gefunden wird
    with open(pyproject_path, 'r') as f:
        pyproject = toml.load(f)
    return pyproject.get('tool', {}).get('poetry', {}).get('version', '0.0.0')

# Holen Sie die Version
version = get_version()

app = typer.Typer(help=f"Kurmann Videoschnitt CLI Version {version}")
app.add_typer(compressor, name="compressor")
app.add_typer(original_media_integrator, name="original-media")
app.add_typer(emby_integrator, name="emby")

if __name__ == "__main__":
    app()