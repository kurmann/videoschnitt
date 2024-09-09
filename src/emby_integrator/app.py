import json
import os
import typer
from emby_integrator.mediaset_manager import get_mediaserver_files
from emby_integrator.video_manager import compress_masterfile
from emby_integrator.image_manager import convert_image_to_adobe_rgb, convert_images_to_adobe_rgb, get_images_for_artwork

# Erstelle die Typer-App
app = typer.Typer(help="FileManager CLI für Emby Integrator")

@app.command()
def list_mediaserver_files(
    source_dir: str, 
    json_output: bool = typer.Option(False, help="Gebe die Ausgabe im JSON-Format aus")
):
    """
    Liste die Mediaserver-Dateien aus einem Verzeichnis auf und gruppiere sie nach Mediensets.

    Diese Methode durchsucht das angegebene Verzeichnis nach Videodateien und zugehörigen Titelbildern 
    und gruppiert diese nach Mediensets. Falls das Flag `--json-output` gesetzt wird, wird die Ausgabe 
    im JSON-Format zurückgegeben, andernfalls wird eine menschenlesbare Ausgabe erstellt, die die 
    Informationen bündig darstellt.

    Args:
        source_dir (str): Der Pfad zu dem Verzeichnis, das nach Mediendateien und Bildern durchsucht wird.
        json_output (bool): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

    Returns:
        None: Gibt die Mediensets in einer menschenlesbaren Form oder als JSON zurück, je nach dem Wert von `json_output`.
    
    Beispiel:
        $ emby-integrator list-mediaserver-files /path/to/mediadirectory

        Ausgabe:
        Medienset: 2024-08-27 Ann-Sophie Spielsachen Bett
        Videos:    2024-08-27 Ann-Sophie Spielsachen Bett.mov
        Titelbild: Kein Titelbild gefunden.
        ----------------------------------------
        Medienset: Ann-Sophie rennt (Testvideo)
        Videos:    Ann-Sophie rennt (Testvideo)-4K60-Medienserver.mov
        Titelbild: Ann-Sophie rennt (Testvideo).jpg
        ----------------------------------------
    
    Raises:
        FileNotFoundError: Wenn das angegebene Verzeichnis nicht existiert.
    """
    media_sets = get_mediaserver_files(source_dir)

    
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

@app.command(name="compress-masterfile")
def compress_masterfile_command(
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
    
    compress_masterfile(input_file, delete_master_file, callback=notify_completion)

@app.command(name="convert-image-to-adobe-rgb")
def convert_image_to_adobe_rgb_command(image_file: str):
    """Erstelle Adobe RGB-JPG-Datei"""
    # Ausgabedatei auf Basis des Eingabedateinamens (im selben Verzeichnis mit .jpg)
    output_file = f"{os.path.splitext(image_file)[0]}.jpg"
    convert_image_to_adobe_rgb(image_file, output_file)

@app.command(name="convert-images-to-adobe-rgb")
def convert_images_to_adobe_rgb_command(media_dir: str):
    """
    Konvertiere eine Liste von PNG-Bildern in Adobe RGB, falls eine passende Videodatei existiert.
    :param media_dir: Verzeichnis, das sowohl die PNG-Bilder als auch die Videodateien enthält
    """
    image_files = [os.path.join(media_dir, f) for f in os.listdir(media_dir) if f.lower().endswith(".png")]
    convert_images_to_adobe_rgb(image_files, media_dir)

@app.command(name="get-images-for-artwork")
def get_images_for_artwork_command(directory: str):
    """Rufe geeignete Bilder für Artwork aus einem Verzeichnis ab."""
    get_images_for_artwork(directory)

if __name__ == '__main__':
    app()