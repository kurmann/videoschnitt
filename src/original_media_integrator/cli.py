import click
from original_media_integrator.media_manager import organize_media_files
from original_media_integrator.new_media_importer import import_and_compress_media

@click.group()
def cli():
    """Original Media Integrator CLI"""
    pass

@cli.command(name="organize-media")
@click.argument('source_dir', type=click.Path(exists=True, file_okay=False))
@click.argument('destination_dir', type=click.Path(exists=True, file_okay=False))
def organize_media_command(source_dir, destination_dir):
    """Organisiert Medien nach Datum."""
    organize_media_files(source_dir, destination_dir)

@cli.command(name="import-media")
@click.argument('source_dir', type=click.Path(exists=True, file_okay=False))
@click.argument('destination_dir', type=click.Path(exists=True, file_okay=False))
@click.option('--compression-dir', type=click.Path(file_okay=False), help="Optionales Komprimierungsverzeichnis.")
@click.option('--keep-original-prores', is_flag=True, help="Behalte die Original-ProRes-Dateien nach der Komprimierung.")
def import_media_command(source_dir, destination_dir, compression_dir=None, keep_original_prores=False):
    """Importiert neue Medien, führt die Kompression durch und organisiert die Dateien."""
    import_and_compress_media(source_dir, destination_dir, compression_dir, keep_original_prores)

if __name__ == "__main__":
    cli()