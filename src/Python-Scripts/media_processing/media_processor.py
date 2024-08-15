import os
from file_utils import move_file_with_postfix, move_and_rename_to_target, delete_file
from video_utils import get_video_codec
from date_utils import get_creation_datetime
from compressor_utils import start_compressor

def are_sb_files_present(hevc_a_path):
    """
    Überprüft, ob temporäre .sb-Dateien vorhanden sind, die auf eine laufende Komprimierung hinweisen.
    Diese Prüfung erfolgt auf das Vorkommen von ".sb-" im Dateinamen.
    """
    directory = os.path.dirname(hevc_a_path)
    base_name = os.path.basename(hevc_a_path).replace('.mov', '')

    # Suche nach Dateien im gleichen Verzeichnis, die ".sb-" im Namen enthalten
    for filename in os.listdir(directory):
        if base_name in filename and ".sb-" in filename:
            print(f"Temporäre Datei gefunden: {filename}. Warte auf Abschluss der Komprimierung.")
            return True

    return False

def process_file(source_file, comp_dir, original_media_dir, compressor_started=False):
    """Verarbeitet eine Datei, verschiebt sie ins Komprimierungsverzeichnis oder integriert sie direkt."""
    codec = get_video_codec(source_file)
    print(f"Verarbeitung der Datei {source_file} mit Codec {codec}")

    if codec == 'prores':
        # Stelle sicher, dass Apple Compressor läuft
        if not compressor_started:
            start_compressor()
            compressor_started = True

        # Verschiebe ProRes-Datei ins Komprimierungsverzeichnis
        print(f"Verschiebe ProRes-Datei in das Komprimierungsverzeichnis {comp_dir}")
        move_file_with_postfix(source_file, comp_dir)
    else:
        # Integriere andere QuickTime-Dateien direkt in das Originalmedien-Verzeichnis
        creation_time = get_creation_datetime(source_file)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
        new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-{codec.upper()}.mov')
        destination_dir = os.path.join(original_media_dir, date_path, codec.upper())
        
        move_and_rename_to_target(source_file, destination_dir, new_filename)

def process_completed_hevca_and_delete_prores(comp_output_dir, comp_dir, original_media_dir):
    """
    Prüft, ob fertige HEVC-A-Dateien im Komprimierungs-Ausgabeverzeichnis vorhanden sind, löscht die passende ProRes-Datei
    und integriert die HEVC-A-Dateien in das Originalmedien-Verzeichnis.
    """
    for filename in os.listdir(comp_output_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov') or '-hevc-a' not in filename.lower():
            continue  # Versteckte Dateien oder nicht HEVC-A-Dateien überspringen

        hevc_a_path = os.path.join(comp_output_dir, filename)

        # Prüfen, ob temporäre .sb-Dateien vorhanden sind, die auf eine laufende Komprimierung hinweisen
        if are_sb_files_present(hevc_a_path):
            continue  # Überspringe, wenn .sb-Dateien gefunden wurden

        # Prüfen, ob die Datei vollständig ist und nicht leer
        if os.path.getsize(hevc_a_path) == 0:
            print(f"Überspringe leere Datei: {hevc_a_path}")
            continue

        # Ermittele den Basenamen der ProRes-Datei
        base_name = filename.replace('-HEVC-A', '').replace('-hevc-a', '').replace('.mov', '')
        prores_path = os.path.join(comp_dir, f"{base_name}.mov")

        print(f"DEBUG: Suche nach ProRes-Datei: {prores_path}")

        if os.path.exists(prores_path):
            print(f"Entsprechende ProRes-Datei gefunden: {prores_path}. Lösche die Datei.")
            delete_file(prores_path)

        # Verschiebe die HEVC-A-Datei in das Originalmedien-Verzeichnis
        creation_time = get_creation_datetime(hevc_a_path)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
        new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-HEVC-A.mov')
        destination_dir = os.path.join(original_media_dir, date_path, 'HEVC-A')
        
        move_and_rename_to_target(hevc_a_path, destination_dir, new_filename)