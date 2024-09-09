import os
import logging
from apple_compressor_manager.compressor_helpers import process_batch

# Modulvariablen
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/4K60-Medienserver.compressorsetting"
SUPPORTED_VIDEO_FORMATS = [".mp4", ".mov"]
COMPRESSOR_SUFFIXES = ["4K60-Medienserver", "4K30-Medienserver"]

def compress_masterfile(input_file: str, delete_master_file: bool = False, callback=None, check_interval: int = 60, max_checks: int = 10):
    """
    Komprimiere eine Master-Datei und prüfe periodisch, ob die Kompression abgeschlossen wurde.
    :param input_file: Pfad zur Eingabedatei
    :param delete_master_file: Gibt an, ob die Originaldatei nach der Komprimierung gelöscht werden soll
    :param callback: Funktion, die nach erfolgreicher Kompression aufgerufen wird
    :param check_interval: Intervall in Sekunden, in dem der Status überprüft wird
    :param max_checks: Maximale Anzahl von Prüfungen
    """
    profile_name = os.path.splitext(os.path.basename(COMPRESSOR_PROFILE_PATH))[0]
    output_file = f"{os.path.splitext(input_file)[0]}-{profile_name}.mov"
    files = [(input_file, output_file)]

    logging.info(f"Starte Kompression für {input_file} mit dem Profil {profile_name}...")
    process_batch(files, COMPRESSOR_PROFILE_PATH, check_interval, max_checks)

    # Callback ausführen, wenn definiert
    if callback:
        callback(input_file, output_file)

    # Lösche die Originaldatei, falls angegeben
    if delete_master_file:
        try:
            os.remove(input_file)
            logging.info(f"Originaldatei gelöscht: {input_file}")
        except Exception as e:
            logging.error(f"Fehler beim Löschen der Originaldatei: {e}")

    logging.info(f"Komprimierung abgeschlossen für {input_file}")

def is_video_file(filename: str) -> bool:
    """
    Prüfe, ob eine Datei eine Videodatei ist.
    :param filename: Der Name der Datei
    :return: True, wenn es eine unterstützte Videodatei ist, False sonst.
    """
    return any(filename.lower().endswith(ext) for ext in SUPPORTED_VIDEO_FORMATS)

def remove_suffix(filename: str) -> str:
    """
    Entfernt bekannte Suffixe aus dem Dateinamen, um den Mediensetnamen zu erhalten.
    :param filename: Der Dateiname
    :return: Der Dateiname ohne das Suffix
    """
    base_name = os.path.splitext(filename)[0]
    for suffix in COMPRESSOR_SUFFIXES:
        if base_name.endswith(f"-{suffix}"):
            return base_name[:-(len(suffix) + 1)]
    return base_name