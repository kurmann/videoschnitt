import os
import shutil
from date_utils import get_creation_datetime
from file_utils import is_file_in_use
from video_utils import get_video_codec, is_hevc_a

def move_file_to_target(source_file, base_source_dir, base_destination_dir):
    """
    Verschiebt eine Datei ins Zielverzeichnis, behält die Unterverzeichnisstruktur bei und organisiert nach Datum.
    
    Args:
        source_file (str): Pfad zur Quelldatei.
        base_source_dir (str): Basis-Quellverzeichnis, um den relativen Pfad zu bestimmen.
        base_destination_dir (str): Basiszielverzeichnis, in das die Datei verschoben wird.
        
    Returns:
        str: Der endgültige Zielpfad der verschobenen Datei, oder None bei einem Fehler.
    """
    try:
        # Datum der Datei extrahieren
        creation_time = get_creation_datetime(source_file)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')

        # Relativen Pfad zum Quellverzeichnis finden (um Unterverzeichnisse beizubehalten)
        relative_dir = os.path.relpath(os.path.dirname(source_file), base_source_dir).lstrip(os.sep)

        # Zielverzeichnis basierend auf Datum und Unterverzeichnisstruktur erstellen
        destination_dir = os.path.join(base_destination_dir, date_path, relative_dir)
        
        # Dateiendung beibehalten und neuen Namen basierend auf dem Datum generieren
        extension = os.path.splitext(source_file)[1].lower()
        codec = get_video_codec(source_file)

        if codec is None:
            print(f"Warnung: Konnte den Codec für {source_file} nicht ermitteln. Behalte den Dateinamen bei.")
            filename = os.path.basename(source_file)
        else:
            if codec.lower() == "prores":
                codec = "ProRes"
            elif is_hevc_a(source_file):
                codec = "HEVC-A"
            else:
                codec = ''

            # Neuer Dateiname mit Datum und ggf. Codec
            filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S{f"-{codec}" if codec else ""}{extension}')

        # Sicherstellen, dass das Zielverzeichnis existiert
        os.makedirs(destination_dir, exist_ok=True)
        
        # Vollständiger Pfad der Zieldatei
        destination_path = os.path.join(destination_dir, filename)
        
        # Verschiebe die Datei
        shutil.move(source_file, destination_path)
        print(f"Datei verschoben: {source_file} -> {destination_path}")
        
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