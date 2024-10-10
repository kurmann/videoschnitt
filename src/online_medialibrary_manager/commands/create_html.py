import os
import typer
from online_medialibrary_manager.html_generator import generate_html

def create_html_command(
    metadata_source: str,
    high_res_file: str,
    mid_res_file: str,
    artwork_image: str,
    output_file: str = 'index.html',
    download_file: str = None,
    base_url: str = '',
) -> None:
    """
    Generiert eine statische HTML-Seite für das Familienvideo und erstellt ein OpenGraph-Bild.

    Args:
        metadata_source (str): Pfad zur Videodatei, aus der die Metadaten extrahiert werden sollen.
        high_res_file (str): Pfad zur hochauflösenden Videodatei (4K HEVC).
        mid_res_file (str): Pfad zur mittelauflösenden Videodatei (HD).
        artwork_image (str): Pfad zum Vorschaubild.
        output_file (str): Name der Ausgabedatei für das HTML (Standard: 'index.html').
        download_file (str): Optionaler Pfad zur Download-Datei (z.B. ZIP-Datei).
        base_url (str): Basis-URL für die OG-Metadaten (z.B. https://example.com/videos).
    """
    try:
        html_content = generate_html(metadata_source, high_res_file, mid_res_file, artwork_image, download_file, base_url)
        
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(html_content)
        
        typer.secho(f"HTML-Datei wurde erfolgreich erstellt: {output_file}", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der HTML-Datei: {e}", fg=typer.colors.RED)