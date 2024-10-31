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
    force: bool = typer.Option(False, "--force", help="Unterdrückt alle Rückfragen und führt den Workflow automatisch aus.")
):
    """
    Führt den Workflow aus: Integration in Emby Mediathek und Cleanup von zugehörigen Dateien.
    Löscht die Originaldateien nach erfolgreicher Integration in Emby Mediathek.
    """
    typer.secho("Willkommen zum FCP Workflow!", fg=typer.colors.GREEN)
    typer.secho("Der Workflow umfasst folgende Schritte:", fg=typer.colors.BLUE)
    typer.secho("1. Integration der Videos in Emby Mediathek (Originaldateien werden gelöscht).", fg=typer.colors.BLUE)
    typer.secho("2. Cleanup von zugehörigen Dateien (PNG, M4A)", fg=typer.colors.BLUE)
    
    if not force:
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
            delete_source_files=True
        )
    except Exception as e:
        logger.exception("Fehler bei der Integration in Emby Mediathek")
        typer.secho(f"Fehler bei der Integration in Emby Mediathek: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    # Schritt 2: Cleanup der zugehörigen Dateien
    typer.secho("\nStarte Cleanup der zugehörigen Dateien...", fg=typer.colors.BLUE)
    logger.info("Starte Cleanup der zugehörigen Dateien...")
    cleanup_related_files(search_dir)
    if additional_dir:
        cleanup_related_files(additional_dir)
    
    typer.secho("\nWorkflow abgeschlossen.", fg=typer.colors.GREEN)
    logger.info("Workflow abgeschlossen.")

def cleanup_related_files(directory: Path):
    """
    Löscht zugehörige Dateien wie PNG und M4A basierend auf den Dateinamen im Verzeichnis.
    """
    for video_file in directory.glob("*.mov"):
        base_name = video_file.stem  # Dateiname ohne Erweiterung
        
        # Definiere die zugehörigen Dateipfade
        png_file = directory / f"{base_name}.png"
        m4a_file = directory / f"{base_name}.m4a"
        
        # Lösche PNG-Datei, falls vorhanden
        if png_file.exists():
            try:
                png_file.unlink()
                typer.secho(f"Lösche zugehörige Datei: {png_file}", fg=typer.colors.GREEN)
                logger.info(f"Lösche zugehörige Datei: {png_file}")
            except Exception as e:
                typer.secho(f"Fehler beim Löschen der Datei {png_file}: {e}", fg=typer.colors.RED)
                logger.error(f"Fehler beim Löschen der Datei {png_file}: {e}")
        
        # Lösche M4A-Datei, falls vorhanden, diese dient als Markierung für beendete Bearbeitung
        # Hinweis: Aus Final Cut Pro kann ich nur mit Export von Masterdatei oder Masteraudio ein Script nach Ende der Kompression ausführen
        if m4a_file.exists():
            try:
                m4a_file.unlink()
                typer.secho(f"Lösche zugehörige Datei: {m4a_file}", fg=typer.colors.GREEN)
                logger.info(f"Lösche zugehörige Datei: {m4a_file}")
            except Exception as e:
                typer.secho(f"Fehler beim Löschen der Datei {m4a_file}: {e}", fg=typer.colors.RED)
                logger.error(f"Fehler beim Löschen der Datei {m4a_file}: {e}")

if __name__ == "__main__":
    app()