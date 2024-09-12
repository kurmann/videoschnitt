# original_media_integrator/app.py

import typer
from original_media_integrator.new_media_importer import import_and_compress_media

app = typer.Typer()

@app.command()
def import_media(
    source_dir: str = typer.Argument(..., help="Pfad zum Quellverzeichnis"),
    destination_dir: str = typer.Argument(..., help="Pfad zum Zielverzeichnis"),
    compression_dir: str = typer.Option(None, help="Optionales Komprimierungsverzeichnis"),
    keep_original_prores: bool = typer.Option(False, help="Behalte die Original-ProRes-Dateien nach der Komprimierung.")
):
    """
    CLI-Kommando zum Importieren und Komprimieren von Medien.
    
    Dieser Befehl ruft die import_and_compress_media Funktion auf, die neue Dateien komprimiert
    und organisiert.
    """
    # Verwende die bestehende Funktion
    import_and_compress_media(source_dir, destination_dir, compression_dir, keep_original_prores)

if __name__ == "__main__":
    app()