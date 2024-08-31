from apple_compressor_manager.compress import run_compress
from original_media_integrator.media_manager import organize_media_files

def import_and_compress_media(source_dir, destination_dir, compression_dir=None, keep_original_prores=False):
    """
    Importiert neue Medien, komprimiert ProRes-Dateien und organisiert alle Medien nach Datum.
    
    :param source_dir: Quellverzeichnis, das Medien enthält
    :param destination_dir: Zielverzeichnis, in das die Medien organisiert werden
    :param compression_dir: Optionales Verzeichnis für die Kompression. Wenn angegeben, 
                            werden die HEVC-A-Dateien dort erstellt und anschließend organisiert.
    :param keep_original_prores: Wenn True, werden die Original-ProRes-Dateien nicht gelöscht.
                                 Standardmäßig werden die ProRes-Dateien gelöscht.
    """
    
    # Definiere einen Callback für die Kompression, der aufgerufen wird, wenn eine Datei komprimiert wurde
    def on_compression_complete(hevc_a_file):
        print(f"Kompression abgeschlossen für: {hevc_a_file}")

    # Steuerung der Löschoption basierend auf dem Parameter keep_original_prores
    delete_prores = not keep_original_prores

    # Wenn ein Kompressionsverzeichnis angegeben ist, verwende es als `output_directory`
    if compression_dir:
        run_compress(source_dir, compression_dir, delete_prores, callback=on_compression_complete)
        
        # Wenn die Kompression abgeschlossen ist, organisiere die HEVC-A-Dateien aus dem Kompressionsverzeichnis
        print(f"Organisiere komprimierte Dateien aus: {compression_dir}")
        organize_media_files(compression_dir, destination_dir)
    else:
        # Wenn kein Kompressionsverzeichnis angegeben ist, komprimiere direkt im Quellverzeichnis
        run_compress(source_dir, source_dir, delete_prores, callback=on_compression_complete)
    
    # Organisiere alle übrigen Dateien aus dem Quellverzeichnis im Zielverzeichnis
    print(f"Organisiere übrige Dateien aus: {source_dir}")
    organize_media_files(source_dir, destination_dir)
    
    # Teile dem Benutzer mit, dass der Vorgang abgeschlossen ist
    print("Import und Kompression abgeschlossen.")

if __name__ == "__main__":
    # Beispielaufruf
    base_source_dir = "/path/to/source_directory"
    base_destination_dir = "/path/to/destination_directory"
    base_compression_dir = "/path/to/compression_directory"  # Optional

    import_and_compress_media(base_source_dir, base_destination_dir, base_compression_dir, keep_original_prores=True)