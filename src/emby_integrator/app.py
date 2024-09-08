import json
import os
import typer
from emby_integrator.file_manager import FileManager


# Erstelle eine Instanz von FileManager
file_manager = FileManager()

# Erstelle die Typer-App
app = typer.Typer(help="FileManager CLI für Emby Integrator")

@app.command()
def list_mediaserver_files(source_dir: str, json_output: bool = typer.Option(False, help="Gebe die Ausgabe im JSON-Format aus")):
    """
    Liste die Mediaserver-Dateien aus einem Verzeichnis auf und gruppiere sie nach Mediensets.
    """
    media_sets = file_manager.get_mediaserver_files(source_dir)
    
    if json_output:
        # JSON-Ausgabe
        print(json.dumps(media_sets, indent=4))
    else:
        # Bündige Ausgabe der Informationen
        max_label_length = 10  # Feste Länge für die Labels "Videos" und "Titelbild"
        
        for set_name, data in media_sets.items():
            print(f"Medienset: {set_name}")
            # Label "Videos" auf eine feste Breite setzen
            print(f"{'Videos:':<{max_label_length}} {', '.join(data['videos']) if data['videos'] else 'Keine Videodateien gefunden.'}")
            # Label "Titelbild" ebenfalls auf die gleiche Breite setzen
            print(f"{'Titelbild:':<{max_label_length}} {data['image'] if data['image'] else 'Kein Titelbild gefunden.'}")
            print("-" * 40)

@app.command()
def compress_masterfile(
    input_file: str, 
    delete_master_file: bool = typer.Option(False, help="Lösche die Master-Datei nach der Komprimierung.")
):
    """
    Komprimiere eine Master-Datei.

    Diese Methode startet die Kompression der angegebenen Datei und bietet die Möglichkeit, 
    nach Abschluss der Komprimierung die Originaldatei zu löschen.
    """
    
    # Definiere einen Callback, der eine Benachrichtigung sendet, wenn die Komprimierung abgeschlossen ist
    def notify_completion(input_file, output_file):
        print(f"Komprimierung abgeschlossen für: {input_file}")
    
    file_manager.compress_masterfile(input_file, delete_master_file, callback=notify_completion)

@app.command()
def convert_image_to_adobe_rgb(image_file: str):
    """Erstelle Adobe RGB-JPG-Datei"""
    # Ausgabedatei auf Basis des Eingabedateinamens (im selben Verzeichnis mit .jpg)
    output_file = f"{os.path.splitext(image_file)[0]}.jpg"
    file_manager.convert_image_to_adobe_rgb(image_file, output_file)

@app.command()
def convert_images_to_adobe_rgb(media_dir: str):
    """
    Konvertiere eine Liste von PNG-Bildern in Adobe RGB, falls eine passende Videodatei existiert.
    :param media_dir: Verzeichnis, das sowohl die PNG-Bilder als auch die Videodateien enthält
    """
    image_files = [os.path.join(media_dir, f) for f in os.listdir(media_dir) if f.lower().endswith(".png")]
    file_manager.convert_images_to_adobe_rgb(image_files, media_dir)

@app.command()
def get_images_for_artwork(directory: str):
    """Rufe geeignete Bilder für Artwork aus einem Verzeichnis ab."""
    file_manager.get_images_for_artwork(directory)

if __name__ == '__main__':
    app()