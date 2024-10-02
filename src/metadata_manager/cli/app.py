# src/metadata_manager/cli/app.py

import typer
from metadata_manager.logging_config import setup_logging

# Importiere die Register-Funktionen der Commands
from metadata_manager.cli.commands.show_metadata import register as register_show_metadata
from metadata_manager.cli.commands.export_metadata import register as register_export_metadata
from metadata_manager.cli.commands.validate_file import register as register_validate_file
from metadata_manager.cli.commands.get_creation_datetime import register as register_get_creation_datetime
from metadata_manager.cli.commands.get_album import register as register_get_album
from metadata_manager.cli.commands.get_video_codec import register as register_get_video_codec
from metadata_manager.cli.commands.get_bitrate import register as register_get_bitrate
from metadata_manager.cli.commands.is_hevc_a import register as register_is_hevc_a

def main():
    # Zentrale Logging-Konfiguration
    setup_logging()
    
    # Erstelle die Haupt-Typer-App
    app = typer.Typer(
        help="Metadata Manager CLI für Kurmann Videoschnitt",
        name="metadata-manager"
    )
    
    # Registriere die einzelnen Commands
    register_show_metadata(app)
    register_export_metadata(app)
    register_validate_file(app)
    register_get_creation_datetime(app)
    register_get_album(app)
    register_get_video_codec(app)
    register_get_bitrate(app)
    register_is_hevc_a(app)
    
    # Starte die App
    app()

if __name__ == "__main__":
    main()