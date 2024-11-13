from pathlib import Path
import tomllib
from typing import Dict
from venv import logger

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
        logger.error(f"Konfigurationsdatei '{config_path}' wurde nicht gefunden.")
        raise typer.Exit(code=1)
    
    try:
        with config_path.open('rb') as f:
            config = tomllib.load(f)
        logger.debug(f"Konfigurationsdaten geladen: {config}")
    except tomllib.TOMLDecodeError as e:
        typer.secho(
            f"❌ Fehler beim Parsen der Konfigurationsdatei: {e}",
            fg=typer.colors.RED,
            bold=True
        )
        logger.error(f"Fehler beim Parsen der Konfigurationsdatei '{config_path}': {e}")
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
        logger.error(f"Fehlende Sektionen in der Konfigurationsdatei '{config_path}': {', '.join(missing_sections)}")
        raise typer.Exit(code=1)
    
    # Überprüfe, ob 'name_mappings' korrekt ist
    logger.debug("Überprüfe 'name_mappings' in der Konfiguration.")
    for short_name, full_name in config['name_mappings'].items():
        logger.debug(f"Mapping: {short_name} -> {full_name}")
    
    # Überprüfe, ob 'default_roles' korrekt ist
    logger.debug("Überprüfe 'default_roles' in der Konfiguration.")
    for name, role in config['default_roles'].items():
        logger.debug(f"Role: {name} -> {role}")
    
    # Überprüfe, ob 'groups' korrekt ist
    logger.debug("Überprüfe 'groups' in der Konfiguration.")
    for group, members in config['groups'].items():
        logger.debug(f"Group: {group} -> Members: {members}")
    
    return config