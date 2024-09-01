import click
from .cleanup_prores import run_cleanup
from .compress_filelist import run_compress_prores as run_compress_files
from .compress_file import run_compress_file

@click.group()
def cli():
    """Apple Compressor Manager CLI"""
    pass

@cli.command(name="cleanup-prores")
@click.argument('hevc_a_dir', type=click.Path(exists=True, file_okay=False))
@click.argument('prores_dir', type=click.Path(exists=True, file_okay=False), required=False)
@click.option('--verbose', is_flag=True, help="Aktiviere detaillierte Ausgaben.")
def cleanup_prores_command(hevc_a_dir, prores_dir=None, verbose=False):
    """Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant."""
    run_cleanup(hevc_a_dir, prores_dir, verbose)

@cli.command(name="compress-prores-files")
@click.argument('input_dir', type=click.Path(exists=True, file_okay=False))
@click.option('--output', type=click.Path(file_okay=False), help="Das Verzeichnis, in dem die Ausgabedateien gespeichert werden sollen.")
@click.option('--delete-prores', is_flag=True, help="Lösche ProRes-Dateien nach erfolgreicher Komprimierung.")
def compress_prores_files_command(input_dir, output=None, delete_prores=False):
    """Komprimiert ProRes-Dateien in einem Verzeichnis."""
    if output is None:
        output = input_dir  # Standardmäßig wird das input_dir als output_dir verwendet
    run_compress_files(input_dir, output, delete_prores=delete_prores)

@cli.command(name="compress-prores-file")
@click.argument('input_file', type=click.Path(exists=True, dir_okay=False))
@click.option('--output', type=click.Path(file_okay=False), help="Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll.")
@click.option('--delete-prores', is_flag=True, help="Lösche die ProRes-Datei nach erfolgreicher Komprimierung.")
def compress_prores_file_command(input_file, output=None, delete_prores=False):
    """Komprimiert eine einzelne ProRes-Datei."""
    run_compress_file(input_file, output_directory=output, delete_prores=delete_prores)

if __name__ == "__main__":
    cli()