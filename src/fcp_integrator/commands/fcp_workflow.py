# src/fcp_integrator/commands/run_workflow.py

import typer
from pathlib import Path
from typing import Optional
import logging

# Importiere die spezifischen Integrationsfunktionen
from iclouddrive_integrator.commands.homemovie_integrator import integrate_homemovies as integrate_into_icloud
from emby_integrator.commands.homemovie_integrator import integrate_homemovies as integrate_into_emby

app = typer.Typer()

# Logging-Konfiguration
logging.basicConfig(
    filename='fcp_workflow.log',
    filemode='a',
    format='%(asctime)s - %(levelname)s - %(message)s',
    level=logging.DEBUG
)
logger = logging.getLogger(__name__)

@app.command("run")
def run_workflow(
    search_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Das Hauptverzeichnis mit Mediendateien."
    ),
    additional_dir: Optional[Path] = typer.Option(
        None,
        "--additional-dir",
        "-ad",
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Zusätzliches Verzeichnis."
    ),
    icloud_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        writable=True,
        readable=True,
        help="Das Zielverzeichnis in iCloud Drive."
    ),
    emby_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        writable=True,
        readable=True,
        help="Das Zielverzeichnis in der Emby Mediathek."
    ),
    overwrite_existing: bool = typer.Option(
        False,
        "--overwrite-existing",
        help="Überschreibt bestehende Dateien ohne Rückfrage, wenn diese existieren."
    ),
    force: bool = typer.Option(
        False,
        "--force",
        help="Unterdrückt alle Rückfragen und führt den Workflow automatisch aus."
    )
):
    """
    Führt den kompletten Workflow aus: Integration in iCloud Drive und Emby Mediathek.
    Löscht die Originaldateien nach erfolgreicher Integration in Emby Mediathek.
    """
    typer.secho("Willkommen zum FCP Workflow!", fg=typer.colors.GREEN)
    typer.secho("Der Workflow umfasst folgende Schritte:", fg=typer.colors.BLUE)
    typer.secho("1. Integration der Videos in iCloud Drive (Originaldateien verbleiben).", fg=typer.colors.BLUE)
    typer.secho("2. Integration der Videos in Emby Mediathek (Originaldateien werden gelöscht).", fg=typer.colors.BLUE)
    
    if not force:
        confirm = typer.confirm("Möchtest du den gesamten Workflow starten und die Quelldateien nach der Emby-Integration löschen?")
        if not confirm:
            typer.secho("Workflow abgebrochen.", fg=typer.colors.YELLOW)
            raise typer.Exit()
    
    # Schritt 1: Integration in iCloud Drive
    typer.secho("\nStarte Integration in iCloud Drive...", fg=typer.colors.BLUE)
    logger.info("Starte Integration in iCloud Drive...")
    
    try:
        integrate_into_icloud(
            search_dir=search_dir,
            additional_dir=additional_dir,
            icloud_dir=icloud_dir,
            overwrite_existing=overwrite_existing,
            delete_source=False  # Originaldateien bleiben erhalten
        )
    except Exception as e:
        typer.secho(f"Fehler bei der Integration in iCloud Drive: {e}", fg=typer.colors.RED)
        logger.error(f"Fehler bei der Integration in iCloud Drive: {e}")
        raise typer.Exit(code=1)
    
    # Schritt 2: Integration in Emby Mediathek
    typer.secho("\nStarte Integration in Emby Mediathek...", fg=typer.colors.BLUE)
    logger.info("Starte Integration in Emby Mediathek...")
    
    try:
        integrate_into_emby(
            search_dir=search_dir,
            additional_dir=additional_dir,
            mediathek_dir=emby_dir,  # Korrigierter Parametername
            overwrite_existing=overwrite_existing,
            delete_source_files=True
        )
    except Exception as e:
        typer.secho(f"Fehler bei der Integration in Emby Mediathek: {e}", fg=typer.colors.RED)
        logger.error(f"Fehler bei der Integration in Emby Mediathek: {e}")
        raise typer.Exit(code=1)
    
    typer.secho("\nWorkflow abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Workflow abgeschlossen.")

if __name__ == "__main__":
    app()