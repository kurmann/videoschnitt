import os
import time
import subprocess
from dotenv import load_dotenv
from collections import defaultdict
from apple_compressor_manager.compressor_helpers import process_batch, send_macos_notification

# Lade die .env Datei
load_dotenv()

# Modulvariablen
ADOBE_RGB_PROFILE = "/System/Library/ColorSync/Profiles/AdobeRGB1998.icc"
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/4K60-Medienserver.compressorsetting"

class FileManager:
    SUPPORTED_VIDEO_FORMATS = [".mp4", ".mov"]
    SUPPORTED_IMAGE_FORMATS = [".jpg"]
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
            
            # Überprüfe, ob es sich um eine Bilddatei handelt (JPG)
            elif any(file.lower().endswith(ext) for ext in self.SUPPORTED_IMAGE_FORMATS):
                media_sets[base_name]["image"] = file
        
        # Ausgabe der Mediensets
        for set_name, data in media_sets.items():
            print(f"Medienset: {set_name}")
            print(f"  Videos: {', '.join(data['videos']) if data['videos'] else 'Keine Videodateien gefunden.'}")
            print(f"  Titelbild: {data['image'] if data['image'] else 'Kein Titelbild gefunden.'}")
            print("-" * 40)  # Trennlinie für die Übersicht

        return media_sets

    def get_images_for_artwork(self, directory: str):
        """
        Rufe JPG-Bilder im Adobe RGB-Farbprofil ab, die mit den Mediendateien übereinstimmen.
        :param directory: Verzeichnis, in dem nach Bildern für Artwork gesucht wird.
        """
        # Erhalte alle Mediendateien im Verzeichnis
        media_files = self.get_mediaserver_files(directory)

        # Entferne Suffixe, um den Mediensetnamen zu erhalten
        media_set_names = [self._remove_suffix(file) for file in media_files]
        artwork_files = []

        for file in os.listdir(directory):
            if any(file.lower().endswith(ext) for ext in self.SUPPORTED_IMAGE_FORMATS):
                # Prüfe, ob ein Bild zu einem Medienset passt
                image_name = os.path.splitext(file)[0]
                if image_name in media_set_names:
                    artwork_files.append(file)
        
        if artwork_files:
            print(f"Gefundene Bilder für Artwork im Verzeichnis {directory}: {artwork_files}")
        else:
            print(f"Keine passenden Bilder für Artwork im Verzeichnis {directory} gefunden.")
        
        return artwork_files

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