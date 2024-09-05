import typer
from .cleanup_prores import run_cleanup
from .compress_filelist import run_compress_prores as run_compress_files
from .compress_file import run_compress_file

app = typer.Typer(help="Apple Compressor Manager CLI")

@app.command("cleanup-prores")
def cleanup_prores_command(
    hevc_a_dir: str = typer.Argument(..., help="Pfad zum HEVC-A-Verzeichnis"),
    prores_dir: str = typer.Argument(None, help="Pfad zum ProRes-Verzeichnis", show_default=False),
    verbose: bool = typer.Option(False, "--verbose", help="Aktiviere detaillierte Ausgaben.")
):
    """Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant."""
    run_cleanup(hevc_a_dir, prores_dir, verbose)

@app.command("compress-prores-files")
def compress_prores_files_command(
    input_dir: str = typer.Argument(..., help="Pfad zum Quellverzeichnis der ProRes-Dateien"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedateien gespeichert werden sollen"),
    delete_prores: bool = typer.Option(False, "--delete-prores", help="Lösche ProRes-Dateien nach erfolgreicher Komprimierung")
):
    """Komprimiert ProRes-Dateien in einem Verzeichnis."""
    if output is None:
        output = input_dir  # Standardmäßig wird das input_dir als output_dir verwendet
    run_compress_files(input_dir, output, delete_prores=delete_prores)

@app.command("compress-prores-file")
def compress_prores_file_command(
    input_file: str = typer.Argument(..., help="Pfad zur ProRes-Datei"),
    output: str = typer.Option(None, help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll"),
    delete_prores: bool = typer.Option(False, "--delete-prores", help="Lösche die ProRes-Datei nach erfolgreicher Komprimierung")
):
    """Komprimiert eine einzelne ProRes-Datei."""
    run_compress_file(input_file, output_directory=output, delete_prores=delete_prores)

if __name__ == "__main__":
    app()