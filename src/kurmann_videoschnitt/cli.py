import click
from apple_compressor_manager.cli import cli as apple_compressor_manager_cli

@click.group()
def cli():
    """Kurmann Videoschnitt - Zentrale CLI"""
    pass

# Integriere die CLI-Befehle des Apple Compressor Managers
cli.add_command(apple_compressor_manager_cli, name='compressor')

if __name__ == "__main__":
    cli()