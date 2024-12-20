from pathlib import Path
import tomllib
from typing import Dict

import typer

try:
    import tomllib  # Python 3.11+
except ModuleNotFoundError:
    import tomli as tomllib  # Für Python < 3.11

def load_config(config_path: Path) -> Dict:
    """
    Lädt die Konfiguration aus der config.toml Datei.
    """
    if not config_path.exists():
        typer.secho(
            f"❌ Konfigurationsdatei '{config_path}' wurde nicht gefunden.",
            fg=typer.colors.RED,
            bold=True
        )
        typer.secho(
            "Bitte stelle sicher, dass die 'config.toml' Datei im Root-Verzeichnis der Anwendung vorhanden ist.",
            fg=typer.colors.YELLOW
        )
        raise typer.Exit(code=1)
    
    try:
        with config_path.open('rb') as f:
            config = tomllib.load(f)
        typer.secho(f"✅ Konfigurationsdatei '{config_path}' erfolgreich geladen.", fg=typer.colors.GREEN)
    except tomllib.TOMLDecodeError as e:
        typer.secho(
            f"❌ Fehler beim Parsen der Konfigurationsdatei: {e}",
            fg=typer.colors.RED,
            bold=True
        )
        typer.secho(
            "Bitte überprüfe die Syntax der 'config.toml' Datei und korrigiere sie.",
            fg=typer.colors.YELLOW
        )
        raise typer.Exit(code=1)
    
    # Überprüfe, ob alle notwendigen Sektionen vorhanden sind
    required_sections = ['name_mappings', 'default_roles', 'groups']
    missing_sections = [section for section in required_sections if section not in config]
    
    if missing_sections:
        typer.secho(
            f"❌ Fehlende Sektionen in der Konfigurationsdatei: {', '.join(missing_sections)}",
            fg=typer.colors.RED,
            bold=True
        )
        typer.secho(
            "Bitte füge die fehlenden Sektionen hinzu, um das Skript korrekt auszuführen.",
            fg=typer.colors.YELLOW
        )
        typer.secho("Beende das Skript.", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    return config