# original_media_integrator/new_media_importer.py

import os
from original_media_integrator.media_manager import organize_media_files
import logging

# Konfiguriere das Logging
logger = logging.getLogger(__name__)

def import_media_only(source_dir: str, destination_dir: str):
    """
    Funktion zum Importieren und Organisieren von Medien ohne Kompression.

    :param source_dir: Quellverzeichnis, das Medien enthält
    :param destination_dir: Zielverzeichnis, in das die Medien organisiert werden
    """
    try:
        logger.info(f"Starte das Organisieren der Medien von {source_dir} nach {destination_dir}...")
        organize_media_files(source_dir, destination_dir)
        logger.info("Import und Organisation abgeschlossen.")
    except Exception as e:
        logger.exception(f"Ein Fehler ist aufgetreten während des Imports: {e}")
        print(f"Ein Fehler ist aufgetreten während des Imports: {e}")