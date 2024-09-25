# online_medialibrary_manager/app.py

"""
Das 'app' Modul enthält die CLI-Befehle für den Online Medialibrary Manager.
Es ermöglicht die Interaktion mit der Anwendung über die Kommandozeile und nutzt die Funktionen
und Klassen der anderen Module.
"""

import os
import typer
from online_medialibrary_manager.html_generator import generate_html
from emby_integrator.metadata_manager import get_metadata
from online_medialibrary_manager.image_manager import create_og_image

app = typer.Typer(help="Online Medialibrary Manager für Familienvideos")

@app.command()
def create_html(
    metadata_source: str = typer.Argument(..., help="Pfad zur Videodatei, aus der die Metadaten extrahiert werden sollen"),
    high_res_file: str = typer.Argument(..., help="Pfad zur hochauflösenden Videodatei (4K HEVC)"),
    mid_res_file: str = typer.Argument(..., help="Pfad zur mittelauflösenden Videodatei (HD)"),
    artwork_image: str = typer.Argument(..., help="Pfad zum Vorschaubild"),
    output_file: str = typer.Option('index.html', help="Name der Ausgabedatei für das HTML (Standard: index.html)"),
    download_file: str = typer.Option(None, help="Optionaler Pfad zur Download-Datei (z.B. ZIP-Datei)"),
    base_url: str = typer.Option('', help="Basis-URL für die OG-Metadaten (z.B. https://example.com/videos)"),
):
    """
    Generiert eine statische HTML-Seite für das Familienvideo und erstellt ein OpenGraph-Bild.
    """
    try:
        html_content = generate_html(metadata_source, high_res_file, mid_res_file, artwork_image, download_file, base_url)
        
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(html_content)
        
        typer.secho(f"HTML-Datei wurde erfolgreich erstellt: {output_file}", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der HTML-Datei: {e}", fg=typer.colors.RED)

@app.command(name="create-og-image")
def create_og_image_command(
    artwork_image: str = typer.Argument(..., help="Pfad zum Vorschaubild"),
    output_image: str = typer.Option('og-image.jpg', help="Name der Ausgabedatei für das OpenGraph-Bild (Standard: og-image.jpg)")
):
    """
    Erzeugt ein OpenGraph-Bild aus dem gegebenen Vorschaubild.
    """
    try:
        create_og_image(artwork_image, output_image)
        typer.secho(f"OpenGraph-Bild wurde erfolgreich erstellt: {output_image}", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen des OpenGraph-Bildes: {e}", fg=typer.colors.RED)

if __name__ == '__main__':
    app()