import click
from .media_manager import organize_media_files
from .new_media_importer import new_media_import

@click.group()
def cli():
    """Original Media Importer CLI"""
    pass

@cli.command(name="organize-media")
@click.argument('source_dir', type=click.Path(exists=True, file_okay=False))
@click.argument('destination_dir', type=click.Path(file_okay=False))
def organize_media_files_command(source_dir, destination_dir):
    """
    Organisiert und verschiebt Mediendateien aus SOURCE_DIR nach DESTINATION_DIR.

    Diese Funktion durchsucht das Quellverzeichnis nach unterstützten Medienformaten
    und verschiebt sie unter Beibehaltung der Verzeichnisstruktur in das Zielverzeichnis.
    """
    organize_media_files(source_dir, destination_dir)

@cli.command(name="import-new-media")
@click.argument('source_dir', type=click.Path(exists=True, file_okay=False))
@click.argument('destination_dir', type=click.Path(file_okay=False))
@click.option('--compress-dir', type=click.Path(file_okay=False), help="Optionales Verzeichnis für die Komprimierung. Wenn nicht angegeben, wird das Zielverzeichnis verwendet.")
def new_media_import_command(source_dir, destination_dir, compress_dir=None):
    """
    Importiert neue Medien aus SOURCE_DIR nach DESTINATION_DIR und verarbeitet diese.

    Diese Funktion führt den Neumedien-Import durch, startet den Kompressionsprozess 
    (optional in einem separaten Verzeichnis) und organisiert anschließend die Mediendateien.
    """
    new_media_import(source_dir, destination_dir, compress_dir)

if __name__ == "__main__":
    cli()