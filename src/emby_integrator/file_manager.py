import os
from dotenv import load_dotenv

# Lade die .env Datei
load_dotenv()

class FileManager:
    def __init__(self):
        self.compressor_profile_path = os.getenv('COMPRESSOR_PROFILE_PATH', '/default/path/to/compressor_profile.compressorsetting')
        # Weitere Konfigurationen können hier hinzugefügt werden

    def get_mediaserver_files(self, source_dir: str):
        print(f"get_mediaserver_files called with: source_dir={source_dir}")
        # Implementierung folgt später

    def compress_masterfile(self, input_file: str, delete_master_file: bool = False):
        print(f"compress_masterfile called with: input_file={input_file}, delete_master_file={delete_master_file}")
        print(f"Using compressor_profile_path: {self.compressor_profile_path}")
        # Implementierung folgt später

    def convert_image_to_adobe_rgb(self, image_file: str):
        print(f"convert_image_to_adobe_rgb called with: image_file={image_file}")
        # Implementierung folgt später

    def get_images_for_artwork(self, directory: str):
        print(f"get_images_for_artwork called with: directory={directory}")
        # Implementierung folgt später