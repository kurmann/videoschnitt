import os
from collections import defaultdict
from emby_integrator.video_manager import is_video_file, remove_suffix

SUPPORTED_IMAGE_FORMATS = [".jpg"]

def get_mediaserver_files(source_dir: str):
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
        FileNotFoundError: Wenn das Verzeichnis nicht existiert.
        ValueError: Wenn keine Mediendateien gefunden werden.
    """
    if not os.path.exists(source_dir):
        raise FileNotFoundError(f"Das Verzeichnis '{source_dir}' wurde nicht gefunden.")

    media_sets = defaultdict(lambda: {"videos": [], "image": None})

    # Dateien im Verzeichnis durchsuchen
    files_in_directory = os.listdir(source_dir)

    for file in files_in_directory:
        base_name = remove_suffix(file)

        # Überprüfe, ob es sich um eine Videodatei handelt
        if is_video_file(file):
            media_sets[base_name]["videos"].append(file)

    # Suche nach den zugehörigen JPG-Dateien
    for file in files_in_directory:
        if any(file.lower().endswith(ext) for ext in SUPPORTED_IMAGE_FORMATS):
            base_name = os.path.splitext(file)[0]
            if base_name in media_sets:
                media_sets[base_name]["image"] = file

    if not media_sets:
        raise ValueError(f"Keine Mediendateien im Verzeichnis '{source_dir}' gefunden.")

    return media_sets