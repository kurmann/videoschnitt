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
            moved_file_path = move_and_rename_to_target(file_path, destination_dir, new_filename)

            if moved_file_path:
                # Wenn die HEVC-A-Datei erfolgreich verschoben wurde, lösche die zugehörige ProRes-Datei
                find_and_delete_prores(original_media_dir, creation_time)

def find_and_delete_prores(original_dir, creation_time):
    """Findet und löscht die entsprechende ProRes-Datei basierend auf dem Aufnahmedatum."""
    base_filename = creation_time.strftime('%Y-%m-%d_%H%M%S')
    prores_filename = base_filename + ".mov"
    
    for root, _, files in os.walk(original_dir):
        for filename in files:
            if filename == prores_filename:
                prores_path = os.path.join(root, filename)
                print(f"ProRes-Datei gefunden: {prores_path}")
                try:
                    os.remove(prores_path)
                    print(f"ProRes-Datei gelöscht: {prores_path}")
                except Exception as e:
                    print(f"Fehler beim Löschen der ProRes-Datei: {e}")
                return True

    print(f"Keine passende ProRes-Datei gefunden für: {base_filename}")
    return False