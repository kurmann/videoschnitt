# src/kurmann_videoschnitt/app.py

import typer
from importlib.metadata import version, PackageNotFoundError

# Import der Sub-CLIs
from apple_compressor_manager.app import app as compressor
from original_media_integrator.app import app as original_media_integrator
from emby_integrator.app import app as emby_integrator
from online_medialibrary_manager.app import app as onlinemedialibrary_manager

# Funktion zur Versionsabfrage
def get_version() -> str:
    """
    Liest die Version aus den installierten Paketmetadaten.
    """
    try:
        return version("kurmann-videoschnitt")
    except PackageNotFoundError:
        return "0.0.0"  # Standardversion, falls das Paket nicht gefunden wird

# Initialisiere die Typer-Anwendung ohne Callback
app = typer.Typer(help="Kurmann Videoschnitt CLI")

# Füge Subcommands hinzu
app.add_typer(compressor, name="compressor")
app.add_typer(original_media_integrator, name="original-media")
app.add_typer(emby_integrator, name="emby")
app.add_typer(onlinemedialibrary_manager, name="online-media")

# Separater Befehl zur Anzeige der Version
@app.command("get-version")
def get_version_command():
    """
    Zeigt die aktuelle Version des CLI-Tools an.
    """
    version_str = get_version()
    typer.echo(f"kurmann-videoschnitt Version: {version_str}")

# Separater Befehl zur Überprüfung der .env-Datei
@app.command()
def check_env():
    """
    Überprüft, ob die .env-Datei geladen wurde.
    """
    from .config import ENV_FILE  # Import innerhalb der Funktion, um Lazy Loading zu gewährleisten
    if ENV_FILE.exists():
        typer.echo(f".env-Datei geladen von: {ENV_FILE}")
    else:
        typer.echo(f"WARNUNG: Es wurde keine .env-Datei unter {ENV_FILE} gefunden.")

if __name__ == "__main__":
    app()