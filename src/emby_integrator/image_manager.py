import os
import subprocess
import logging

# Modulvariablen
ADOBE_RGB_PROFILE = "/System/Library/ColorSync/Profiles/AdobeRGB1998.icc"

class ImageManager:
    SUPPORTED_IMAGE_FORMATS = [".jpg"]
    COMPRESSOR_SUFFIXES = ["4K60-Medienserver", "4K30-Medienserver"]

    def _remove_suffix(self, filename: str):
        """
        Entfernt bekannte Suffixe aus dem Dateinamen, um den Mediensetnamen zu erhalten.
        """
        base_name = os.path.splitext(filename)[0]
        for suffix in self.COMPRESSOR_SUFFIXES:
            if base_name.endswith(f"-{suffix}"):
                return base_name[:-(len(suffix) + 1)]  # Suffix entfernen und Name zurückgeben
        return base_name  # Kein Suffix gefunden, originaler Basisname bleibt

    def get_images_for_artwork(self, directory: str, media_set_names: list):
        """
        Rufe JPG-Bilder im Adobe RGB-Farbprofil ab, die mit den Mediendateien übereinstimmen.
        :param directory: Verzeichnis, in dem nach Bildern für Artwork gesucht wird.
        :param media_set_names: Liste der Medienset-Namen, zu denen Bilder gefunden werden sollen.
        """
        artwork_files = []

        for file in os.listdir(directory):
            if any(file.lower().endswith(ext) for ext in self.SUPPORTED_IMAGE_FORMATS):
                # Prüfe, ob ein Bild zu einem Medienset passt
                image_name = os.path.splitext(file)[0]
                if image_name in media_set_names:
                    artwork_files.append(file)

        if artwork_files:
            logging.info(f"Gefundene Bilder für Artwork im Verzeichnis {directory}: {artwork_files}")
        else:
            logging.info(f"Keine passenden Bilder für Artwork im Verzeichnis {directory} gefunden.")

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
            logging.info(f"Erfolgreich konvertiert: {input_file} -> {output_file}")
        except subprocess.CalledProcessError as e:
            logging.error(f"Fehler beim Konvertieren von {input_file}: {e}")

    def _find_related_video(self, image_file: str, media_dir: str):
        """
        Überprüfe, ob eine passende Videodatei für das gegebene Bild existiert.
        :param image_file: Der Name der Bilddatei
        :param media_dir: Das Verzeichnis, in dem nach der zugehörigen Videodatei gesucht wird
        :return: True, wenn eine zugehörige Videodatei existiert, sonst False
        """
        base_name = os.path.splitext(os.path.basename(image_file))[0]

        # Entferne bekannte Suffixe vom Bildnamen, um den Mediensetnamen zu erhalten
        for suffix in self.COMPRESSOR_SUFFIXES:
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
                logging.info(f"Keine zugehörige Videodatei für {image_file} gefunden. Keine Konvertierung durchgeführt.")