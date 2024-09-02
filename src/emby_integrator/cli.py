import click
from emby_integrator.file_manager import FileManager

# Erstelle eine Instanz von FileManager
file_manager = FileManager()

@click.group()
def cli():
    """FileManager CLI für Emby Integrator"""
    pass

@click.command()
@click.argument('source_dir')
def get_mediaserver_files(source_dir):
    """Rufe die Mediaserver-Dateien aus einem Verzeichnis ab."""
    file_manager.get_mediaserver_files(source_dir)

@click.command()
@click.argument('input_file')
@click.option('--delete-master-file', is_flag=True, help="Lösche die Master-Datei nach der Komprimierung.")
def compress_masterfile(input_file, delete_master_file):
    """Komprimiere eine Master-Datei."""
    file_manager.compress_masterfile(input_file, delete_master_file)

@click.command()
@click.argument('image_file')
def convert_image_to_adobe_rgb(image_file):
    """Konvertiere ein Bild in das Adobe RGB-Profil und speichere es als JPEG."""
    file_manager.convert_image_to_adobe_rgb(image_file)

@click.command()
@click.argument('directory')
def get_images_for_artwork(directory):
    """Rufe geeignete Bilder für Artwork aus einem Verzeichnis ab."""
    file_manager.get_images_for_artwork(directory)

# Füge die Commands zur CLI-Gruppe hinzu
cli.add_command(get_mediaserver_files)
cli.add_command(compress_masterfile)
cli.add_command(convert_image_to_adobe_rgb)
cli.add_command(get_images_for_artwork)

if __name__ == '__main__':
    cli()