import typer
from .media_manager import organize_media_files
from .new_media_importer import import_and_compress_media

app = typer.Typer(help="Original Media Integrator CLI")

@app.command()
def organize_media(source_dir: str, destination_dir: str):
    """Organisiert Medien nach Datum."""
    organize_media_files(source_dir, destination_dir)

@app.command()
def import_media(
    source_dir: str, 
    destination_dir: str, 
    compression_dir: str = None, 
    keep_original_prores: bool = False):
    """Importiert neue Medien, führt die Kompression durch und organisiert die Dateien."""
    import_and_compress_media(source_dir, destination_dir, compression_dir, keep_original_prores)

if __name__ == "__main__":
    app()
