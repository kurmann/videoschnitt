# online_medialibrary_manager/app.py

"""
Das 'app' Modul enthält die CLI-Befehle für den HTML Generator.
Es ermöglicht die Interaktion mit der Anwendung über die Kommandozeile und nutzt die Funktionen
und Klassen der anderen Module.
"""

import os
import typer
from online_medialibrary_manager import generate_html

app = typer.Typer(help="HTML Generator für Familienvideos")

@app.command()
def create_html(
    original_file: str = typer.Argument(..., help="Pfad zur Originalvideodatei"),
    high_res_file: str = typer.Argument(..., help="Pfad zur hochauflösenden Videodatei (4K HEVC)"),
    mid_res_file: str = typer.Argument(..., help="Pfad zur mittelauflösenden Videodatei (HD)"),
    artwork_image: str = typer.Argument(..., help="Pfad zum Vorschaubild"),
    output_file: str = typer.Option('index.html', help="Name der Ausgabedatei für das HTML (Standard: index.html)"),
):
    """
    Generiert eine statische HTML-Seite für das Familienvideo.
    """
    try:
        html_content = generate_html(original_file, high_res_file, mid_res_file, artwork_image)
        
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(html_content)
        
        typer.secho(f"HTML-Datei wurde erfolgreich erstellt: {output_file}", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der HTML-Datei: {e}", fg=typer.colors.RED)

if __name__ == '__main__':
    app()