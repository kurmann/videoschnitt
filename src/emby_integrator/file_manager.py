import os
import time
import subprocess
from dotenv import load_dotenv
from apple_compressor_manager.compressor_helpers import process_batch, send_macos_notification

# Lade die .env Datei
load_dotenv()

# Modulvariablen
COMPRESSOR_SUFFIXES = ["4K60-Medienserver", "4K30-Medienserver"]
ADOBE_RGB_PROFILE = "/System/Library/ColorSync/Profiles/AdobeRGB1998.icc"
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

    def convert_image_to_adobe_rgb(self, input_file: str, output_file: str):
        """
        Konvertiere ein PNG-Bild in das Adobe RGB-Farbprofil und speichere es als JPEG.
        :param input_file: Pfad zur Eingabedatei (PNG)
        :param output_file: Pfad zur Ausgabedatei (JPEG)
        """
        if not input_file.lower().endswith(".png"):
            raise ValueError("Eingabedatei muss eine PNG-Datei sein.")
        
        command = [
            "sips", "-m", ADOBE_RGB_PROFILE, input_file, "--out", output_file
        ]
        
        try:
            subprocess.run(command, check=True)
            print(f"Erfolgreich konvertiert: {input_file} -> {output_file}")
        except subprocess.CalledProcessError as e:
            print(f"Fehler beim Konvertieren von {input_file}: {e}")

    def _find_related_video(self, image_file: str, media_dir: str):
        """
        Überprüfe, ob eine passende Videodatei für das gegebene Bild existiert.
        :param image_file: Der Name der Bilddatei
        :param media_dir: Das Verzeichnis, in dem nach der zugehörigen Videodatei gesucht wird
        :return: True, wenn eine zugehörige Videodatei existiert, sonst False
        """
        base_name = os.path.splitext(os.path.basename(image_file))[0]

        # Entferne bekannte Suffixe vom Bildnamen, um den Mediensetnamen zu erhalten
        for suffix in COMPRESSOR_SUFFIXES:
            if base_name.endswith(f"-{suffix}"):
                base_name = base_name[:-(len(suffix) + 1)]
                break

        # Prüfe, ob eine Videodatei mit dem Mediensetnamen existiert
        for file in os.listdir(media_dir):
            if file.startswith(base_name) and file.lower().endswith((".mp4", ".mov")):
                return True
        return False

    def convert_images_to_adobe_rgb(self, image_files: list, media_dir: str):
        """
        Konvertiere eine Liste von PNG-Bildern in Adobe RGB, falls eine passende Videodatei existiert.
        :param image_files: Liste von Bilddateien (PNG)
        :param media_dir: Verzeichnis, in dem nach zugehörigen Videodateien gesucht wird
        """
        for image_file in image_files:
            if self._find_related_video(image_file, media_dir):
                output_file = f"{os.path.splitext(image_file)[0]}.jpg"
                self.convert_image_to_adobe_rgb(image_file, output_file)
            else:
                print(f"Keine zugehörige Videodatei für {image_file} gefunden. Keine Konvertierung durchgeführt.")