import os
import sys
from file_utils import is_file_in_use, move_file_with_postfix, move_and_rename_to_target
from video_utils import get_video_codec, get_creation_datetime
from compressor_utils import start_compressor
from hevca_integration import process_completed_hevca_files

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

    # Zuerst prüfen, ob fertige HEVC-A-Dateien im Komprimierungs-Ausgabeverzeichnis vorhanden sind
    process_completed_hevca_files(comp_output_dir, original_media_dir)

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