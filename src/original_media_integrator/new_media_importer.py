import os
from apple_compressor_manager.compress_prores import run_compress
from original_media_integrator.media_manager import organize_media_files

def new_media_import(source_dir, destination_dir):
    """
    Startet den Medienimport, indem zuerst die ProRes-Dateien komprimiert werden,
    die Original ProRes bereinigt und dann die Dateien
    in das Zielverzeichnis organisiert werden.
    """

    def compression_finished_callback(output_file):
        print(f"Komprimierung abgeschlossen: {output_file}")
        organize_media_files(source_dir, destination_dir)

    print("Starte Komprimierung der ProRes-Dateien...")
    run_compress(source_dir, source_dir, delete_prores=True, callback=compression_finished_callback)

    print("Medienimport abgeschlossen.")

if __name__ == "__main__":
    # Testparameter
    source_dir = "/path/to/source_directory"  # Anpassen auf dein Quellverzeichnis
    destination_dir = "/path/to/destination_directory"  # Anpassen auf dein Zielverzeichnis

    new_media_import(source_dir, destination_dir)