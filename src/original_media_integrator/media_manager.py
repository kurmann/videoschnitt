# original_media_integrator/media_manager.py

import os
import shutil
from original_media_integrator.exif_utils import get_creation_datetime
from original_media_integrator.file_utils import is_file_in_use
from original_media_integrator.video_utils import get_video_codec, is_hevc_a

def move_file_to_target(source_file, base_source_dir, base_destination_dir):
    """
    Verschiebt eine Datei ins Zielverzeichnis, behält die Unterverzeichnisstruktur bei und organisiert nach Datum.
    Fügt die Zeitzone im Dateinamen hinzu. Wenn der Dateiname "_edited" enthält, wird dies als "-edited" im neuen Dateinamen übernommen.
    """
    try:
        # Datum der Datei extrahieren, inklusive Zeitzone
        creation_time = get_creation_datetime(source_file)
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

        # Codec-Überprüfung für MOV/MP4-Dateien
        codec = get_video_codec(source_file) if extension == '.mov' or extension == '.mp4' else None

        if codec is None:
            # Umbenennung für andere Dateitypen wie HEIC, JPG usw.
            filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S{timezone_suffix}{edited_suffix}{extension}')
        else:
            # Spezifische Codecs hinzufügen, falls vorhanden (z.B. ProRes oder HEVC-A)
            if codec.lower() == "prores":
                codec = "ProRes"
            elif is_hevc_a(source_file):
                codec = "HEVC-A"
            else:
                codec = ''

            # Neuer Dateiname mit Datum, Zeitzone, Codec und ggf. -edited
            filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S{timezone_suffix}{f"-{codec}" if codec else ""}{edited_suffix}{extension}')

        # Sicherstellen, dass das Zielverzeichnis existiert
        os.makedirs(destination_dir, exist_ok=True)

        # Vollständiger Pfad der Zieldatei
        destination_path = os.path.join(destination_dir, filename)

        # Verschiebe die Datei
        print(f"Verschiebe Datei {source_file} ({os.path.getsize(source_file) / (1024 * 1024):.2f} MiB) nach {destination_dir}")
        shutil.move(source_file, destination_path)
        print(f"Datei verschoben nach {destination_path}")

        return destination_path
    except Exception as e:
        print(f"Fehler beim Verschieben der Datei {source_file}: {e}")
        return None

def organize_media_files(source_dir, destination_dir):
    base_source_dir = os.path.abspath(source_dir)  # Setze base_source_dir auf den absoluten Pfad des Quellverzeichnisses

    for root, _, files in os.walk(source_dir):
        for filename in files:
            if filename.startswith('.') or not filename.lower().endswith(('.mov', '.mp4', '.jpg', '.jpeg', '.png', '.heif', '.heic', '.dng')):
                continue

            file_path = os.path.join(root, filename)

            if is_file_in_use(file_path):
                print(f"Datei {filename} wird noch verwendet. Überspringe.")
                continue

            # Nutze die Funktion `move_file_to_target` zum Verschieben und Organisieren
            move_file_to_target(file_path, base_source_dir, destination_dir)

    remove_empty_directories(source_dir)
    
def remove_empty_directories(root_dir):
    for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
        if dirpath == root_dir:
            continue

        if not any(fname for fname in filenames if not fname.startswith('.')) and not dirnames:
            try:
                os.rmdir(dirpath)
                print(f"Leeres Verzeichnis entfernt: {dirpath}")
            except Exception as e:
                print(f"Fehler beim Entfernen des Verzeichnisses {dirpath}: {e}")

if __name__ == "__main__":
    # Testparameter
    base_source_dir = "/path/to/source_directory"  # Anpassen auf dein Quellverzeichnis
    base_destination_dir = "/path/to/destination_directory"  # Anpassen auf dein Zielverzeichnis
    
    organize_media_files(base_source_dir, base_destination_dir)