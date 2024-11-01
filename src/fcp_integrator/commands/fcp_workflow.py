import os
import typer
from pathlib import Path
from typing import Optional
import logging

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
    search_dir: Path = typer.Argument(..., exists=True, file_okay=False, dir_okay=True, readable=True, help="Das Hauptverzeichnis mit Mediendateien."),
    additional_dir: Optional[Path] = typer.Option(None, "--additional-dir", "-ad", exists=True, file_okay=False, dir_okay=True, readable=True, help="Zusätzliches Verzeichnis."),
    emby_dir: Path = typer.Argument(..., exists=True, file_okay=False, dir_okay=True, writable=True, readable=True, help="Das Zielverzeichnis in der Emby Mediathek."),
    overwrite_existing: bool = typer.Option(False, "--overwrite-existing", help="Überschreibt bestehende Dateien ohne Rückfrage, wenn diese existieren."),
    force: bool = typer.Option(False, "--force", help="Unterdrückt alle Rückfragen und führt den Workflow automatisch aus."),
    delete_source_files: bool = typer.Option(False, "--delete-source-files", help="Löscht die Quelldateien nach erfolgreicher Integration.")
):
    """
    Führt den Workflow aus: Integration in Emby Mediathek und Cleanup von zugehörigen Dateien.
    Löscht die Originaldateien nach erfolgreicher Integration in Emby Mediathek, wenn --delete-source-files gesetzt ist.
    """
    typer.secho("Willkommen zum FCP Workflow!", fg=typer.colors.GREEN)
    typer.secho("Der Workflow umfasst folgende Schritte:", fg=typer.colors.BLUE)
    typer.secho("1. Integration der Videos in Emby Mediathek.", fg=typer.colors.BLUE)
    typer.secho("2. (Optional) Cleanup von zugehörigen Dateien (PNG, M4A)", fg=typer.colors.BLUE)
    
    if not force and not delete_source_files:
        confirm = typer.confirm("Möchtest du den gesamten Workflow starten und die Quelldateien nach der Emby-Integration löschen?")
        if not confirm:
            typer.secho("Workflow abgebrochen.", fg=typer.colors.YELLOW)
            raise typer.Exit()
    
    # Schritt 1: Integration in Emby Mediathek
    typer.secho("\nStarte Integration in Emby Mediathek...", fg=typer.colors.BLUE)
    logger.info("Starte Integration in Emby Mediathek...")
    
    try:
        integrate_into_emby(
            search_dir=search_dir,
            additional_dir=additional_dir,
            mediathek_dir=emby_dir,
            overwrite_existing=overwrite_existing,
            delete_source_files=delete_source_files  # Änderung hier
        )
    except Exception as e:
        logger.exception("Fehler bei der Integration in Emby Mediathek")
        typer.secho(f"Fehler bei der Integration in Emby Mediathek: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    typer.secho("\nWorkflow abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Workflow abgeschlossen.")

if __name__ == "__main__":
    app()