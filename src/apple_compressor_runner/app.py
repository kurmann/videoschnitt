# src/apple_compressor_runner/app.py

import typer
import logging
from config_manager.config_loader import load_app_env
from apple_compressor_runner.commands.run_job_command import run_job_command

# Lade die .env Datei
env_path = load_app_env()

# Initialisiere das Logging
logging.basicConfig(
    level=logging.INFO,  # Setze auf DEBUG für detaillierte Logs während der Fehlerbehebung
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("apple_compressor_runner.log"),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

app = typer.Typer(help="Apple Compressor Runner")

# Registriere den Run-Job Befehl
app.command()(run_job_command)

if __name__ == "__main__":
    app()