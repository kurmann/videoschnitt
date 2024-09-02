import click
from apple_compressor_manager.cli import cli as apple_compressor_manager_cli
from original_media_integrator.cli import cli as original_media_integrator_cli
from emby_integrator.cli import cli as emby_integrator_cli

@click.group()
def cli():
    """Kurmann Videoschnitt - Zentrale CLI"""
    pass

# Integriere die CLI-Befehle des Apple Compressor Managers
cli.add_command(apple_compressor_manager_cli, name='compressor')
cli.add_command(original_media_integrator_cli, name='integrator')
cli.add_command(emby_integrator_cli, name='emby')

if __name__ == "__main__":
    cli()