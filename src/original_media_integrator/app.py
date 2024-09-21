# src/kurmann_videoschnitt/original_media_integrator/app.py

import typer
from original_media_integrator.new_media_importer import import_and_compress_media

app = typer.Typer(help="Original Media Integrator")

@app.command()
def import_media(
    source_dir: str = typer.Option(
        None,
        "--source-dir",
        "-s",
        help="Pfad zum Quellverzeichnis",
        envvar="original_media_source_dir"
    ),
    destination_dir: str = typer.Option(
        None,
        "--destination-dir",
        "-d",
        help="Pfad zum Zielverzeichnis",
        envvar="original_media_destination_dir"
    ),
    compression_dir: str = typer.Option(
        None,
        "--compression-dir",
        "-c",
        help="Optionales Komprimierungsverzeichnis",
        envvar="original_media_compression_dir"
    ),
    keep_original_prores: bool = typer.Option(
        False,
        "--keep-original-prores",
        "-k",
        help="Behalte die Original-ProRes-Dateien nach der Komprimierung.",
        envvar="original_media_keep_original_prores"
    )
):
    """
    CLI-Kommando zum Importieren und Komprimieren von Medien.
    
    Dieser Befehl ruft die import_and_compress_media Funktion auf, die neue Dateien komprimiert
    und organisiert.
    """
    
    # Überprüfe, ob erforderliche Argumente vorhanden sind
    missing_args = []
    if not source_dir:
        missing_args.append("original_media_source_dir")
    if not destination_dir:
        missing_args.append("original_media_destination_dir")
    
    if missing_args:
        typer.echo(f"Fehler: Die folgenden Konfigurationswerte fehlen: {', '.join(missing_args)}.")
        typer.echo("Bitte geben Sie die fehlenden Werte entweder über die CLI oder in der .env-Datei an.")
        raise typer.Exit(code=1)
    
    # Verwende die bestehenden Funktionen mit den ermittelten Werten
    import_and_compress_media(
        source_dir=source_dir,
        destination_dir=destination_dir,
        compression_dir=compression_dir,
        keep_original_prores=keep_original_prores
    )

if __name__ == "__main__":
    app()