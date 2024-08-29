import os
from apple_compressor_manager.compress_prores import run_compress
from original_media_integrator.media_manager import organize_media_files

def new_media_import(source_dir, target_dir, compress_dir=None):
    """
    Importiert neue Medien und verarbeitet diese.

    - Komprimiert ProRes-Dateien im Quellverzeichnis (source_dir) in das Komprimierungsverzeichnis (compress_dir).
    - Verschiebt die komprimierten HEVC-A-Dateien aus dem Komprimierungsverzeichnis in das Zielverzeichnis (target_dir).
    - Verschiebt alle verbleibenden Dateien aus dem Quellverzeichnis in das Zielverzeichnis.
    """
    if compress_dir is None:
        compress_dir = target_dir  # Wenn kein Komprimierungsverzeichnis angegeben, wird das Zielverzeichnis verwendet.

    def on_compression_complete(hevc_a_file):
        """
        Callback-Funktion, die nach Abschluss der Kompression aufgerufen wird.
        Verschiebt die HEVC-A-Datei vom Komprimierungsverzeichnis ins Zielverzeichnis.
        """
        try:
            # Bestimmen des relativen Pfades der HEVC-A-Datei
            relative_path = os.path.relpath(hevc_a_file, compress_dir)
            target_path = os.path.join(target_dir, relative_path)

            # Sicherstellen, dass das Zielverzeichnis existiert
            os.makedirs(os.path.dirname(target_path), exist_ok=True)

            # Verschieben der HEVC-A-Datei ins Zielverzeichnis
            os.rename(hevc_a_file, target_path)
            print(f"HEVC-A-Datei verschoben: {hevc_a_file} -> {target_path}")

        except Exception as e:
            print(f"Fehler beim Verschieben der HEVC-A-Datei {hevc_a_file}: {e}")

    # 1. ProRes-Dateien im Quellverzeichnis komprimieren
    run_compress(source_dir, compress_dir, callback=on_compression_complete)

    # 2. Alle verbleibenden Dateien (die nicht komprimiert wurden) verschieben
    organize_media_files(source_dir, target_dir)

if __name__ == "__main__":
    # Testparameter
    source_dir = "/path/to/source_directory"  # Anpassen auf dein Quellverzeichnis
    target_dir = "/path/to/target_directory"  # Anpassen auf dein Zielverzeichnis
    compress_dir = "/path/to/compress_directory"  # Optionales Komprimierungsverzeichnis

    new_media_import(source_dir, target_dir, compress_dir)