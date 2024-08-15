import os
from file_utils import is_file_in_use, move_and_rename_to_target
from video_utils import get_creation_datetime

def process_completed_hevca_files(comp_output_dir, original_media_dir):
    """Überprüft das Komprimierungs-Ausgabeverzeichnis auf fertige HEVC-A-Dateien und verschiebt sie."""
    for filename in os.listdir(comp_output_dir):
        if filename.startswith('.') or filename.endswith('.sb'):
            continue  # Ignoriere versteckte Dateien und Zwischenartefakte

        if filename.lower().endswith('-hevc-a.mov'):
            file_path = os.path.join(comp_output_dir, filename)

            if is_file_in_use(file_path):
                print(f"HEVC-A-Datei {filename} wird noch verwendet. Überspringe.")
                continue

            # Bereite das Zielverzeichnis und den neuen Dateinamen vor
            creation_time = get_creation_datetime(file_path)
            date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
            new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-HEVC-A.mov')
            destination_dir = os.path.join(original_media_dir, date_path, 'HEVC-A')

            # Verschiebe die fertige HEVC-A-Datei ins Originalmedien-Verzeichnis
            print(f"Fertige HEVC-A-Datei gefunden: {filename}. Verschiebe sie ins Originalmedien-Verzeichnis.")
            move_and_rename_to_target(file_path, destination_dir, new_filename)