# src/original_media_integrator/app.py

import typer
from original_media_integrator.commands.import_by_created_date import import_by_created_date
from original_media_integrator.commands.import_by_exif_creation_date import import_by_exif_creation_date
from config_manager.config_loader import load_app_env
import logging

# Lade die .env Datei
env_path = load_app_env()

# Initialisiere das Logging
logging.basicConfig(
    level=logging.INFO,  # Setze auf INFO, kann bei Bedarf auf DEBUG geändert werden
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("original_media_integrator.log"),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

app = typer.Typer(help="Original Media Integrator")

# Registriere die Commands direkt
app.command(name="import-by-create-date")(import_by_created_date)
app.command(name="import-by-exif-creation-date")(import_by_exif_creation_date)

if __name__ == "__main__":
    app()