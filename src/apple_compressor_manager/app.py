import typer
from apple_compressor_manager.cleanup_prores import run_cleanup
from apple_compressor_manager.compress_filelist import run_compress_prores as run_compress_files
from apple_compressor_manager.compress_file import run_compress_file
from apple_compressor_manager.file_utils import add_compression_tag  # Die Funktion direkt importieren

app = typer.Typer(help="Apple Compressor Manager")

# Beispiel für ein weiteres Kommando direkt in der Haupt-CLI
@app.command("cleanup-prores")
def cleanup_prores_command(
    hevc_a_dir: str = typer.Argument(..., help="Pfad zum HEVC-A-Verzeichnis"),
    prores_dir: str = typer.Argument(None, help="Pfad zum ProRes-Verzeichnis", show_default=False),
    verbose: bool = typer.Option(False, "--verbose", help="Aktiviere detaillierte Ausgaben.")
):
    """Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant."""
    run_cleanup(hevc_a_dir, prores_dir, verbose)

# Methode für das Tagging direkt im Hauptkontext hinzufügen
@app.command("add-tag")
def add_tag_command(file_path: str = typer.Argument(..., help="Pfad zur Datei, die getaggt werden soll")):
    """Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu."""
    add_compression_tag(file_path)

@app.command("compress-prores-files")
def compress_prores_files_command(
    input_dir: str = typer.Argument(..., help="Pfad zum Quellverzeichnis der ProRes-Dateien"),
    compressor_profile_path: str = typer.Argument(..., help="Pfad zur Compressor-Settings-Datei"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedateien gespeichert werden sollen"),
    delete_prores: bool = typer.Option(False, "--delete-prores", help="Lösche ProRes-Dateien nach erfolgreicher Komprimierung")
):
    """Komprimiert ProRes-Dateien in einem Verzeichnis."""
    
    def on_completion(output_file):
        print(f"Komprimierung abgeschlossen für Datei: {output_file}")

    if output is None:
        output = input_dir  # Standardmäßig wird das input_dir als output_dir verwendet
    run_compress_files(input_dir, output, compressor_profile_path, delete_prores=delete_prores)

@app.command("compress-prores-file")
def compress_prores_file_command(
    input_file: str = typer.Argument(..., help="Pfad zur ProRes-Datei"),
    compressor_profile_path: str = typer.Argument(..., help="Pfad zur Compressor-Settings-Datei"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll"),
    delete_prores: bool = typer.Option(False, "--delete-prores", help="Lösche die ProRes-Datei nach erfolgreicher Komprimierung")
):
    """Komprimiert eine einzelne ProRes-Datei."""
    
    def on_completion(output_file):
        print(f"Komprimierung abgeschlossen für Datei: {output_file}")

    if output is None:
        output = os.path.dirname(input_file)
    run_compress_file(input_file, output_directory=output, compressor_profile_path=compressor_profile_path, delete_prores=delete_prores)

if __name__ == "__main__":
    app()