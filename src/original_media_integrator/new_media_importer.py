# original_media_integrator/new_media_importer.py

import os
import asyncio
from apple_compressor_manager.compress_filelist import run_compress_prores_async
from original_media_integrator.media_manager import organize_media_files
from apple_compressor_manager.video_utils import get_video_codec

# Modulkonstante für den Pfad zum HEVC-A Compressor-Profil
COMPRESSOR_PROFILE_PATH = "/Users/patrickkurmann/Library/Application Support/Compressor/Settings/HEVC-A.compressorsetting"

async def import_and_compress_media_async(source_dir, destination_dir, compression_dir=None, keep_original_prores=False):
    """
    Asynchrone Funktion zum Importieren und Komprimieren von Medien.

    :param source_dir: Quellverzeichnis, das Medien enthält
    :param destination_dir: Zielverzeichnis, in das die Medien organisiert werden
    :param compression_dir: Optionales Verzeichnis für die Kompression. Wenn angegeben, 
                            werden die HEVC-A-Dateien dort erstellt und anschließend organisiert.
    :param keep_original_prores: Wenn True, werden die Original-ProRes-Dateien nicht gelöscht.
                                 Standardmäßig werden die ProRes-Dateien gelöscht.
    """
    # Definiere einen Callback für die Kompression
    def on_compression_complete(hevc_a_file):
        print(f"Kompression abgeschlossen für: {hevc_a_file}")

    # Steuerung der Löschoption basierend auf dem Parameter keep_original_prores
    delete_prores = not keep_original_prores

    # Filtere nur ProRes-Dateien aus dem Quellverzeichnis
    prores_files = [
        os.path.join(root, file)
        for root, _, files in os.walk(source_dir)
        for file in files
        if file.lower().endswith(".mov") and get_video_codec(os.path.join(root, file)) == "prores"
    ]

    # Gib alle gefundenen ProRes-Dateien aus
    print("Gefundene ProRes-Dateien:")
    for prores_file in prores_files:
        print(prores_file)

    # Wenn ProRes-Dateien gefunden wurden, führe die Kompression asynchron durch
    if prores_files:
        if compression_dir:
            print("Starte asynchrone Komprimierung der ProRes-Dateien...")
            await run_compress_prores_async(prores_files, compression_dir, COMPRESSOR_PROFILE_PATH, delete_prores, callback=on_compression_complete, prores_dir=source_dir)
            # Organisiere die komprimierten Dateien nach Abschluss der Kompression
            print(f"Organisiere komprimierte Dateien aus: {compression_dir}")
            organize_media_files(compression_dir, destination_dir)
        else:
            # Wenn kein Kompressionsverzeichnis angegeben ist, komprimiere direkt im Quellverzeichnis
            print("Starte asynchrone Komprimierung der ProRes-Dateien im Quellverzeichnis...")
            await run_compress_prores_async(prores_files, source_dir, COMPRESSOR_PROFILE_PATH, delete_prores, callback=on_compression_complete)
            # Organisiere die komprimierten Dateien
            organize_media_files(source_dir, destination_dir)
    else:
        print("Keine ProRes-Dateien zum Komprimieren gefunden.")
        # Organisiere die übrigen Dateien
        organize_media_files(source_dir, destination_dir)

    # Teile dem Benutzer mit, dass der Vorgang abgeschlossen ist
    print("Import und Kompression abgeschlossen.")

def import_and_compress_media(source_dir, destination_dir, compression_dir=None, keep_original_prores=False):
    # Starte die asynchrone Funktion
    asyncio.run(import_and_compress_media_async(source_dir, destination_dir, compression_dir, keep_original_prores))

if __name__ == "__main__":
    # Beispielaufruf
    base_source_dir = "/path/to/source_directory"
    base_destination_dir = "/path/to/destination_directory"
    base_compression_dir = "/path/to/compression_directory"  # Optional

    import_and_compress_media(base_source_dir, base_destination_dir, base_compression_dir, keep_original_prores=True)