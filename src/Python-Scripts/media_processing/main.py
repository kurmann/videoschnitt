import os
import sys
from file_utils import is_file_in_use, get_directory_size
from video_utils import get_video_codec
from media_processor import process_completed_hevca_and_delete_prores, process_file

def main():
    if len(sys.argv) != 6:
        print("Usage: python3 main.py /path/to/source_directory /path/to/compression_directory /path/to/compression_output_directory /path/to/original_media_directory max_gb_limit")
        sys.exit(1)

    source_dir = sys.argv[1]
    comp_dir = sys.argv[2]
    comp_output_dir = sys.argv[3]
    original_media_dir = sys.argv[4]
    max_gb_limit = float(sys.argv[5])  # Die maximal erlaubte Größe in GB für das Komprimierungsverzeichnis

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

    # Berechne die aktuelle Größe des Komprimierungsverzeichnisses
    initial_compression_size = get_directory_size(comp_dir) / (1024 * 1024 * 1024)  # Umrechnung in GB

    # Verarbeite die Dateien im Quellverzeichnis
    for filename in os.listdir(source_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov'):
            continue  # Versteckte Dateien oder Dateien, die nicht MOV sind, überspringen

        file_path = os.path.join(source_dir, filename)

        if is_file_in_use(file_path):
            print(f"Datei {filename} wird noch verwendet. Überspringe.")
            continue

        # Prüfe, ob die Datei eine ProRes-Datei ist und das Limit überschritten wird
        codec = get_video_codec(file_path)
        if codec == 'prores':
            file_size_gb = os.path.getsize(file_path) / (1024 * 1024 * 1024)  # Dateigröße in GB
            if initial_compression_size + file_size_gb > max_gb_limit:
                print(f"Das Komprimierungsverzeichnis hat bereits das Limit von {max_gb_limit} GB erreicht.")
                print(f"Überspringe Datei: {filename}, da das Limit für das Komprimierungsverzeichnis erreicht ist.")
                continue  # Überspringe diese Datei, da das Limit überschritten wird

            initial_compression_size += file_size_gb

        # Prozessiere die Datei (verschiebe ins Komprimierungsverzeichnis oder direkt ins Ziel)
        process_file(file_path, comp_dir, original_media_dir, compressor_started)

if __name__ == "__main__":
    main()