import os
import time
import subprocess
from dotenv import load_dotenv
from apple_compressor_manager.compressor_helpers import process_batch, send_macos_notification

# Lade die .env Datei
load_dotenv()

# Modulvariable für den Pfad zum Compressor-Profil
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/4K60-Medienserver.compressorsetting"

class FileManager:
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

        print(f"Starte Kompression für {input_file} mit dem Profil {profile_name}...")
        process_batch(files, COMPRESSOR_PROFILE_PATH, self.check_interval, self.max_checks)

        # Wenn die Kompression abgeschlossen ist, führe den Callback aus
        if callback:
            callback(input_file, output_file)

        # Lösche die Originaldatei, falls die Option gesetzt ist
        if delete_master_file:
            try:
                os.remove(input_file)
                print(f"Originaldatei gelöscht: {input_file}")
            except Exception as e:
                print(f"Fehler beim Löschen der Originaldatei: {e}")

        print(f"Komprimierung abgeschlossen für {input_file}")

    def get_mediaserver_files(self, source_dir: str):
        print(f"get_mediaserver_files called with: source_dir={source_dir}")
        # Implementierung folgt später

    def convert_image_to_adobe_rgb(self, image_file: str):
        print(f"convert_image_to_adobe_rgb called with: image_file={image_file}")
        # Implementierung folgt später

    def get_images_for_artwork(self, directory: str):
        print(f"get_images_for_artwork called with: directory={directory}")
        # Implementierung folgt später