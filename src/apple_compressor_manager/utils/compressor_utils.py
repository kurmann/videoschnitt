#apple_compressor_manager/compressor_utils.py

import os

from metadata_manager import get_video_codec
        
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

def is_output_file_valid(output_file):
    """Prüft, ob die Ausgabedatei gültig ist."""
    if not os.path.exists(output_file):
        return False
    if os.path.getsize(output_file) < 100 * 1024:  # Beispielwert: 100 KB
        return False
    codec = get_video_codec(output_file)
    if codec != "hevc":
        return False
    return True