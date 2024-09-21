# src/kurmann_videoschnitt/app.py

import typer
from importlib.metadata import version, PackageNotFoundError

# Jetzt können wir Typer und die Sub-CLIs importieren
from apple_compressor_manager.app import app as compressor
from original_media_integrator.app import app as original_media_integrator
from emby_integrator.app import app as emby_integrator
# Entferne den doppelten Import
# from original_media_integrator.app import app as original_media_integrator  # Entfernt

def get_version() -> str:
    """
    Liest die Version aus den installierten Paketmetadaten.
    """
    try:
        return version("kurmann-videoschnitt")
    except PackageNotFoundError:
        return "0.0.0"  # Standardversion, falls das Paket nicht gefunden wird

# Holen Sie die Version
version_str = get_version()

# Initialisiere die Typer-Anwendung
app = typer.Typer(help=f"Kurmann Videoschnitt CLI Version {version_str}")

# Füge Subcommands hinzu
app.add_typer(compressor, name="compressor")
app.add_typer(original_media_integrator, name="original-media")
app.add_typer(emby_integrator, name="emby")
# Füge einen separaten Typer für die Env-Prüfung hinzu, wenn gewünscht
# app.add_typer(env_checker, name="env-check")  # Beispiel

# Callback zum Anzeigen der Version
@app.callback()
def main(
    version_option: bool = typer.Option(
        False,
        "--version",
        "-v",
        help="Zeige die Version und beende das Programm."
    )
):
    if version_option:
        typer.echo(f"kurmann-videoschnitt Version: {version_str}")
        raise typer.Exit()

if __name__ == "__main__":
    app()