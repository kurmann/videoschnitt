# src/original_media_integrator/media_manager.py

import os
import shutil
from typing import Optional
from original_media_integrator.file_utils import is_file_in_use
from metadata_manager import get_creation_datetime
import logging

# Konfiguriere das Logging
logger = logging.getLogger(__name__)

def move_file_to_target(source_file: str, base_source_dir: str, base_destination_dir: str) -> Optional[str]:
    """
    Verschiebt eine Datei ins Zielverzeichnis, behält die Unterverzeichnisstruktur vom Quellverzeichnis bei und organisiert nach Datum.

    Details:
    - Wenn die Datei in einem Unterverzeichnis des Quellverzeichnisses liegt, wird dieses Unterverzeichnis im Zielverzeichnis unterhalb der Datumsstruktur beibehalten.
    - Dateien im Wurzelverzeichnis des Quellverzeichnisses werden direkt in die Datumsstruktur des Zielverzeichnisses verschoben.
    - Der Dateiname wird anhand des Erstellungsdatums der Datei generiert und enthält die Zeitzone.
    - Wenn der Dateiname "_edited" enthält, wird dies als "-edited" im neuen Dateinamen übernommen.

    Argumente:
    - source_file (str): Der vollständige Pfad zur Quelldatei.
    - base_source_dir (str): Das Wurzelverzeichnis der Quelle. Dient zur Berechnung des relativen Pfads.
    - base_destination_dir (str): Das Wurzelverzeichnis des Ziels, in das die Datei verschoben wird.

    Rückgabewert:
    - str: Der vollständige Pfad der verschobenen Datei im Zielverzeichnis.
    - None: Wenn ein Fehler auftritt.
    """
    try:
        # Datum der Datei extrahieren, inklusive Zeitzone
        creation_time = get_creation_datetime(source_file)
        if not creation_time:
            raise ValueError("Erstellungsdatum konnte nicht ermittelt werden.")

        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')

        # Relativen Pfad zum Quellverzeichnis finden (um Unterverzeichnisse beizubehalten)
        relative_dir = os.path.relpath(os.path.dirname(source_file), base_source_dir).lstrip(os.sep)

        # Zielverzeichnis basierend auf Datum und Unterverzeichnisstruktur erstellen
        destination_dir = os.path.join(base_destination_dir, date_path, relative_dir)

        # Dateiendung und Dateiname ohne Endung bestimmen
        extension = os.path.splitext(source_file)[1].lower()
        filename_without_extension = os.path.splitext(os.path.basename(source_file))[0]

        # Zeitzoneninformationen extrahieren
        timezone_suffix = creation_time.strftime('%z')

        # Prüfen, ob der Dateiname "_edited" enthält
        edited_suffix = "-edited" if "_edited" in filename_without_extension else ""

        # Neuer Dateiname mit Datum, Zeitzone und ggf. -edited
        filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S{timezone_suffix}{edited_suffix}{extension}')

        # Sicherstellen, dass das Zielverzeichnis existiert
        os.makedirs(destination_dir, exist_ok=True)

        # Vollständiger Pfad der Zieldatei
        destination_path = os.path.join(destination_dir, filename)

        # Verschiebe die Datei
        logger.info(f"Verschiebe Datei {source_file} ({os.path.getsize(source_file) / (1024 * 1024):.2f} MiB) nach {destination_path}")
        shutil.move(source_file, destination_path)
        logger.info(f"Datei verschoben nach {destination_path}")

        return destination_path
    except Exception as e:
        logger.error(f"Fehler beim Verschieben der Datei {source_file}: {e}")
        print(f"Fehler beim Verschieben der Datei {source_file}: {e}")
        return None

def organize_media_files(source_dir: str, destination_dir: str, base_source_dir: Optional[str] = None):
    """
    Organisiert Medien aus dem Quellverzeichnis ins Zielverzeichnis.

    Argumente:
    - source_dir (str): Das Verzeichnis, das die zu organisierenden Dateien enthält.
    - destination_dir (str): Das Zielverzeichnis, in das die Dateien verschoben werden.
    - base_source_dir (str): Das Wurzelverzeichnis zur Berechnung des relativen Pfads. 
                             Wenn None, wird source_dir verwendet.

    Diese Funktion durchläuft rekursiv das source_dir und verschiebt gültige Dateien
    ins destination_dir, wobei die Unterverzeichnisstruktur beibehalten wird.
    """
    if base_source_dir is None:
        base_source_dir = os.path.abspath(source_dir)  # Verwende source_dir als base_source_dir
    else:
        base_source_dir = os.path.abspath(base_source_dir)

    logger.info(f"Durchlaufe das Quellverzeichnis: {source_dir}")
    for root, _, files in os.walk(source_dir):
        for filename in files:
            if filename.startswith('.') or not filename.lower().endswith(('.mov', '.mp4', '.jpg', '.jpeg', '.png', '.heif', '.heic', '.dng')):
                logger.debug(f"Überspringe Datei {filename} aufgrund des Dateityps oder versteckter Datei.")
                continue

            file_path = os.path.join(root, filename)

            if is_file_in_use(file_path):
                logger.warning(f"Datei {filename} wird noch verwendet. Überspringe.")
                print(f"Datei {filename} wird noch verwendet. Überspringe.")
                continue

            # Nutze die Funktion `move_file_to_target` zum Verschieben und Organisieren
            move_file_to_target(file_path, base_source_dir, destination_dir)

    remove_empty_directories(source_dir)

def remove_empty_directories(root_dir: str):
    """
    Entfernt leere Verzeichnisse rekursiv.

    Argumente:
    - root_dir (str): Das Wurzelverzeichnis, in dem nach leeren Verzeichnissen gesucht wird.
    """
    logger.info(f"Starte das Entfernen leerer Verzeichnisse in: {root_dir}")
    for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
        if dirpath == root_dir:
            continue

        if not any(fname for fname in filenames if not fname.startswith('.')) and not dirnames:
            try:
                os.rmdir(dirpath)
                logger.info(f"Leeres Verzeichnis entfernt: {dirpath}")
                print(f"Leeres Verzeichnis entfernt: {dirpath}")
            except Exception as e:
                logger.error(f"Fehler beim Entfernen des Verzeichnisses {dirpath}: {e}")
                print(f"Fehler beim Entfernen des Verzeichnisses {dirpath}: {e}")