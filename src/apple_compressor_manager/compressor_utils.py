#apple_compressor_manager/compressor_utils.py

import os
        
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