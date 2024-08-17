import os
from file_utils import move_and_rename_to_target, delete_file
from video_utils import get_video_codec, get_bitrate
from date_utils import get_creation_datetime

def are_sb_files_present(hevc_a_path):
    """
    Überprüft, ob temporäre .sb-Dateien vorhanden sind, die auf eine laufende Komprimierung hinweisen.
    Diese Prüfung erfolgt auf das Vorkommen von ".sb-" im Dateinamen.
    """
    directory = os.path.dirname(hevc_a_path)
    base_name = os.path.basename(hevc_a_path).replace('.mov', '')

    for filename in os.listdir(directory):
        if base_name in filename and ".sb-" in filename:
            print(f"Temporäre Datei gefunden: {filename}. Warte auf Abschluss der Komprimierung.")
            return True

    return False

def is_hevc_a(source_file):
    """
    Bestimmt, ob es sich bei der Datei um HEVC-A handelt, basierend auf der Bitrate.
    Dateien mit einer Bitrate über 80 Mbit/s werden als HEVC-A betrachtet.
    """
    bitrate = get_bitrate(source_file)
    return bitrate and bitrate > 80 * 1024 * 1024  # 80 Mbit in Bit umrechnen

def process_file(source_file, comp_dir, original_media_dir):
    """
    Verarbeitet eine ProRes-Datei, verschiebt sie ins Komprimierungsverzeichnis und startet die Komprimierung.
    """
    codec = get_video_codec(source_file)
    if codec is None:
        print(f"Warnung: Konnte den Codec für {source_file} nicht ermitteln. Überspringe die Datei.")
        return

    print(f"Verarbeitung der Datei {source_file} mit Codec {codec}")

    if codec == 'prores':
        print(f"Verschiebe ProRes-Datei in das Komprimierungsverzeichnis {comp_dir}")
        move_and_rename_to_target(source_file, comp_dir, os.path.basename(source_file))
    else:
        creation_time = get_creation_datetime(source_file)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
        if codec.lower() == "prores":
            codec = "ProRes"
        elif is_hevc_a(source_file):  # Prüft, ob es sich um HEVC-A handelt
            codec = "HEVC-A"
        else:
            codec = ''  # Keine zusätzliche Kennzeichnung für reguläre HEVC-Dateien

        new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S{f"-{codec}" if codec else ""}.mov')
        destination_dir = os.path.join(original_media_dir, date_path)
        
        move_and_rename_to_target(source_file, destination_dir, new_filename)

def process_completed_hevca_and_delete_prores(comp_output_dir, comp_dir, original_media_dir):
    """
    Überprüft, ob komprimierte HEVC-A-Dateien abgeschlossen sind, und löscht die entsprechenden ProRes-Dateien.
    """
    for filename in os.listdir(comp_output_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov') or '-hevc-a' not in filename.lower():
            continue

        hevc_a_path = os.path.join(comp_output_dir, filename)

        if are_sb_files_present(hevc_a_path):
            continue

        if os.path.getsize(hevc_a_path) == 0:
            print(f"Überspringe leere Datei: {hevc_a_path}")
            continue

        base_name = filename.replace('-HEVC-A', '').replace('-hevc-a', '').replace('.mov', '')
        prores_path = os.path.join(comp_dir, f"{base_name}.mov")

        if os.path.exists(prores_path):
            print(f"Entsprechende ProRes-Datei gefunden: {prores_path}. Lösche die Datei.")
            delete_file(prores_path)

        creation_time = get_creation_datetime(hevc_a_path)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
        new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-HEVC-A.mov')
        destination_dir = os.path.join(original_media_dir, date_path)
        
        move_and_rename_to_target(hevc_a_path, destination_dir, new_filename)