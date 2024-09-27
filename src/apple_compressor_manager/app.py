# src/apple_compressor_manager/app.py

import typer
from config_manager.config_loader import load_app_env
from apple_compressor_manager.commands import (
    add_tag_command,
    compress_file_command,
    list_profiles_command
)
import logging

# Lade die .env Datei
env_path = load_app_env()

# Initialisiere das Logging
logging.basicConfig(
    level=logging.INFO,  # Setze auf DEBUG für detaillierte Logs während der Fehlerbehebung
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("apple_compressor_manager.log"),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

app = typer.Typer(help="Apple Compressor Manager")

# Registriere die Befehle
app.command()(add_tag_command)
app.command()(compress_file_command)
app.command()(list_profiles_command)

if __name__ == "__main__":
    app()