import os
import shutil
from file_utils import move_and_rename_to_target
from date_utils import get_creation_datetime

def process_completed_hevca_files(comp_output_dir, original_media_dir):
    """
    Verarbeitet fertig komprimierte HEVC-A-Dateien und integriert sie in das Originalmedien-Verzeichnis.
    """
    for filename in os.listdir(comp_output_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov'):
            continue  # Versteckte Dateien oder Dateien, die nicht MOV sind, überspringen

        if '-HEVC-A' not in filename:
            continue  # Nur Dateien berücksichtigen, die als HEVC-A markiert sind

        file_path = os.path.join(comp_output_dir, filename)

        # Überprüfen, ob die Datei noch in Verwendung ist
        if os.path.exists(file_path) and not os.path.getsize(file_path):
            print(f"Datei {filename} wird noch verwendet. Überspringe.")
            continue

        # Ermitteln des Aufnahmedatums
        creation_time = get_creation_datetime(file_path)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
        new_filename = creation_time.strftime('%Y-%m-%d_%H%M%S-HEVC-A.mov')
        destination_dir = os.path.join(original_media_dir, date_path, 'HEVC-A')

        try:
            # Verschiebe und benenne die Datei um
            move_and_rename_to_target(file_path, destination_dir, new_filename)
            print(f"Verschoben und umbenannt: {file_path} -> {os.path.join(destination_dir, new_filename)}")

            # Überprüfen, ob die Datei erfolgreich verschoben wurde
            if os.path.exists(os.path.join(destination_dir, new_filename)):
                print(f"Bestätigt: Datei erfolgreich verschoben nach {os.path.join(destination_dir, new_filename)}")
                # Hier könnte jetzt die Logik zur Löschung der zugehörigen ProRes-Datei ergänzt werden
                delete_matching_prores_file(comp_output_dir, filename.replace('-HEVC-A', ''))

        except Exception as e:
            print(f"Fehler beim Verschieben der Datei {filename}: {e}")

def delete_matching_prores_file(prores_dir, base_name):
    """
    Löscht die ProRes-Datei, die zu einer verschobenen HEVC-A-Datei gehört.
    """
    for filename in os.listdir(prores_dir):
        if filename.lower().endswith('.mov') and filename.startswith(base_name):
            file_path = os.path.join(prores_dir, filename)
            try:
                os.remove(file_path)
                print(f"Gelöscht: ProRes-Datei {filename}")
            except Exception as e:
                print(f"Fehler beim Löschen der ProRes-Datei {filename}: {e}")