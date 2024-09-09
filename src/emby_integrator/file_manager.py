import os
import logging
from dotenv import load_dotenv
from collections import defaultdict
from apple_compressor_manager.compressor_helpers import process_batch

# Lade die .env Datei
load_dotenv()

# Modulvariablen
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/4K60-Medienserver.compressorsetting"

class FileManager:
    SUPPORTED_VIDEO_FORMATS = [".mp4", ".mov"]
    COMPRESSOR_SUFFIXES = ["4K60-Medienserver", "4K30-Medienserver"]

    def __init__(self):
        # Lade die Konfigurationswerte aus der .env-Datei
        self.check_interval = int(os.getenv('CHECK_INTERVAL', 60))  # Prüfintervall in Sekunden
        self.max_checks = int(os.getenv('MAX_CHECKS', 10))  # Maximale Anzahl von Prüfungen

    def compress_masterfile(self, input_file: str, delete_master_file: bool = False, callback=None):
        """
        Komprimiere eine Master-Datei und prüfe periodisch, ob die Kompression abgeschlossen wurde.
        :param input_file: Pfad zur Eingabedatei
        :param delete_master_file: Gibt an, ob die Originaldatei nach der Komprimierung gelöscht werden soll
        :param callback: Funktion, die nach erfolgreicher Kompression aufgerufen wird
        """
        # Ableiten des Profilnamens (ohne Dateiendung) aus dem Compressor-Settings-Path
        profile_name = os.path.splitext(os.path.basename(COMPRESSOR_PROFILE_PATH))[0]

        # Ausgabedatei mit dem Profilnamen als Suffix
        output_file = f"{os.path.splitext(input_file)[0]}-{profile_name}.mov"
        files = [(input_file, output_file)]

        logging.info(f"Starte Kompression für {input_file} mit dem Profil {profile_name}...")
        process_batch(files, COMPRESSOR_PROFILE_PATH, self.check_interval, self.max_checks)

        # Wenn die Kompression abgeschlossen ist, führe den Callback aus
        if callback:
            callback(input_file, output_file)

        # Lösche die Originaldatei, falls die Option gesetzt ist
        if delete_master_file:
            try:
                os.remove(input_file)
                logging.info(f"Originaldatei gelöscht: {input_file}")
            except Exception as e:
                logging.error(f"Fehler beim Löschen der Originaldatei: {e}")

        logging.info(f"Komprimierung abgeschlossen für {input_file}")

    def _remove_suffix(self, filename: str):
        """
        Entfernt bekannte Suffixe aus dem Dateinamen, um den Mediensetnamen zu erhalten.
        """
        base_name = os.path.splitext(filename)[0]
        for suffix in self.COMPRESSOR_SUFFIXES:
            if base_name.endswith(f"-{suffix}"):
                return base_name[:-(len(suffix) + 1)]  # Suffix entfernen und Name zurückgeben
        return base_name  # Kein Suffix gefunden, originaler Basisname bleibt

    def get_mediaserver_files(self, source_dir: str):
        """
        Gruppiere Mediendateien nach Mediensets und überprüfe, ob passende Titelbilder existieren.

        Args:
            source_dir (str): Das Verzeichnis, in dem nach Mediendateien und Bildern gesucht wird.

        Returns:
            dict: Ein Dictionary, das Mediensets als Schlüssel enthält und die zugehörigen 
                Videos und optionalen Titelbilder als Wert hat.

                Beispiel:
                {
                    "Medienset_Name": {
                        "videos": ["video1.mp4", "video2.mov"],
                        "image": "titelbild.jpg"  # oder None, wenn kein Bild vorhanden ist
                    }
                }

        Raises:
            FileNotFoundError: Wenn das angegebene Verzeichnis nicht existiert.
            ValueError: Wenn keine Mediendateien gefunden werden.
        """
        # Dictionary zur Gruppierung der Mediensets
        media_sets = defaultdict(lambda: {"videos": [], "image": None})

        # Dateien im Verzeichnis durchsuchen
        for file in os.listdir(source_dir):
            file_path = os.path.join(source_dir, file)
            base_name = self._remove_suffix(file)  # Entferne Suffixe für die Medienset-Gruppe

            # Überprüfe, ob es sich um eine Videodatei handelt
            if any(file.lower().endswith(ext) for ext in self.SUPPORTED_VIDEO_FORMATS):
                media_sets[base_name]["videos"].append(file)

        return media_sets
