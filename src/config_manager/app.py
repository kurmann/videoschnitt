# src/config_manager/app.py

import typer
from pathlib import Path
from dotenv import load_dotenv, set_key, unset_key
import os

app = typer.Typer(help="Config Manager CLI für Kurmann Videoschnitt")

# Pfad zur .env-Datei
ENV_FILE_PATH = Path.home() / "Library/Application Support/Kurmann/Videoschnitt/.env"

def ensure_env_file_exists():
    """
    Stellt sicher, dass die .env-Datei existiert. Erstellt sie andernfalls.
    """
    if not ENV_FILE_PATH.parent.exists():
        ENV_FILE_PATH.parent.mkdir(parents=True, exist_ok=True)
    if not ENV_FILE_PATH.exists():
        ENV_FILE_PATH.touch()

@app.command("set")
def set_config(
    key: str = typer.Argument(..., help="Der Schlüssel, den du setzen möchtest."),
    value: str = typer.Argument(..., help="Der Wert, der dem Schlüssel zugewiesen werden soll.")
):
    """
    Setzt einen Konfigurationswert in der .env-Datei.
    """
    ensure_env_file_exists()
    load_dotenv(dotenv_path=ENV_FILE_PATH)
    
    try:
        set_key(str(ENV_FILE_PATH), key, value)
        typer.echo(f"Setze {key}={value} in {ENV_FILE_PATH}")
    except Exception as e:
        typer.secho(f"Fehler beim Setzen der Konfiguration: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

@app.command("delete")
def delete_config(
    key: str = typer.Argument(..., help="Der Schlüssel, den du löschen möchtest.")
):
    """
    Löscht einen Konfigurationswert aus der .env-Datei.
    """
    if not ENV_FILE_PATH.exists():
        typer.secho(f".env-Datei existiert nicht unter {ENV_FILE_PATH}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    load_dotenv(dotenv_path=ENV_FILE_PATH)
    
    if not os.getenv(key):
        typer.secho(f"Schlüssel '{key}' existiert nicht in der .env-Datei.", fg=typer.colors.YELLOW)
        raise typer.Exit(code=1)
    
    try:
        unset_key(str(ENV_FILE_PATH), key)
        typer.echo(f"Lösche {key} aus {ENV_FILE_PATH}")
    except Exception as e:
        typer.secho(f"Fehler beim Löschen der Konfiguration: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

@app.command("list")
def list_configs():
    """
    Listet alle Konfigurationswerte in der .env-Datei auf.
    """
    if not ENV_FILE_PATH.exists():
        typer.secho(f".env-Datei existiert nicht unter {ENV_FILE_PATH}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    load_dotenv(dotenv_path=ENV_FILE_PATH)
    env_vars = {key: value for key, value in os.environ.items() if key.startswith("original_media_") or key.startswith("online_media_")}
    
    if not env_vars:
        typer.echo("Keine Konfigurationswerte gefunden.")
        raise typer.Exit()
    
    for key, value in env_vars.items():
        typer.echo(f"{key}={value}")

if __name__ == "__main__":
    app()