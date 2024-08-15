import os
import sys
from file_utils import is_file_in_use
from media_processor import process_completed_hevca_and_delete_prores, process_file

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
    process_completed_hevca_and_delete_prores(comp_output_dir, comp_dir, original_media_dir)

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