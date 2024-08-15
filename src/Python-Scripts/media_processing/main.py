import os
import sys
from file_utils import is_file_in_use, move_file_with_postfix, move_and_rename_to_target, delete_file
from video_utils import get_video_codec
from date_utils import get_creation_datetime
from compressor_utils import start_compressor

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

def process_completed_hevca_and_delete_prores(comp_output_dir, comp_dir):
    """
    Prüft, ob fertige HEVC-A-Dateien im Komprimierungs-Ausgabeverzeichnis vorhanden sind und löscht die passende ProRes-Datei.
    """
    for filename in os.listdir(comp_output_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov') or '-hevc-a' not in filename.lower():
            continue  # Versteckte Dateien oder nicht HEVC-A-Dateien überspringen

        hevc_a_path = os.path.join(comp_output_dir, filename)

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
        else:
            print(f"DEBUG: ProRes-Datei nicht gefunden: {prores_path}")

def main():
    if len(sys.argv) != 5:
        print("Usage: python3 main.py /path/to/source_directory /path/to/compression_directory /path/to/compression_output_directory /path/to/original_media_directory")
        sys.exit(1)

    source_dir = sys.argv[1]
    comp_dir = sys.argv[2]
    comp_output_dir = sys.argv[3]
    original_media_dir = sys.argv[4]

    if not os.path.isdir(source_dir):
        print(f"Error: {source_dir} is not a valid directory")
        sys.exit(1)

    if not os.path.isdir(comp_dir):
        print(f"Error: {comp_dir} is not a valid directory")
        sys.exit(1)

    if not os.path.isdir(comp_output_dir):
        print(f"Error: {comp_output_dir} is not a valid directory")
        sys.exit(1)

    if not os.path.isdir(original_media_dir):
        print(f"Error: {original_media_dir} is not a valid directory")
        sys.exit(1)

    compressor_started = False

    # Zuerst prüfen, ob fertige HEVC-A-Dateien im Komprimierungs-Ausgabeverzeichnis vorhanden sind und die passende ProRes-Datei löschen
    process_completed_hevca_and_delete_prores(comp_output_dir, comp_dir)

    # Verarbeite die Dateien im Quellverzeichnis
    for filename in os.listdir(source_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov'):
            continue  # Versteckte Dateien oder Dateien, die nicht MOV sind, überspringen

        file_path = os.path.join(source_dir, filename)

        if is_file_in_use(file_path):
            print(f"Datei {filename} wird noch verwendet. Überspringe.")
            continue

        process_file(file_path, comp_dir, original_media_dir, compressor_started)

if __name__ == "__main__":
    main()