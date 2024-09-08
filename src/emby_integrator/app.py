import typer
from emby_integrator.file_manager import FileManager

# Erstelle eine Instanz von FileManager
file_manager = FileManager()

# Erstelle die Typer-App
app = typer.Typer(help="FileManager CLI für Emby Integrator")

@app.command()
def get_mediaserver_files(source_dir: str):
    """Rufe die Mediaserver-Dateien aus einem Verzeichnis ab."""
    file_manager.get_mediaserver_files(source_dir)

@app.command()
def compress_masterfile(
    input_file: str, 
    delete_master_file: bool = typer.Option(False, help="Lösche die Master-Datei nach der Komprimierung.")
):
    """Komprimiere eine Master-Datei."""
    
    # Definiere einen Callback, der eine Benachrichtigung sendet, wenn die Komprimierung abgeschlossen ist
    def notify_completion(input_file, output_file):
        print(f"Komprimierung abgeschlossen für: {input_file}")
    
    file_manager.compress_masterfile(input_file, delete_master_file, callback=notify_completion)

@app.command()
def convert_image_to_adobe_rgb(image_file: str):
    """Konvertiere ein Bild in das Adobe RGB-Profil und speichere es als JPEG."""
    file_manager.convert_image_to_adobe_rgb(image_file)

@app.command()
def get_images_for_artwork(directory: str):
    """Rufe geeignete Bilder für Artwork aus einem Verzeichnis ab."""
    file_manager.get_images_for_artwork(directory)

if __name__ == '__main__':
    app()
