# src/fcp_integrator/commands/convert_images.py

import typer
from pathlib import Path
from typing import Optional
import subprocess
import time  # Import der time-Bibliothek

app = typer.Typer()

SUPPORTED_IMAGE_FORMATS = [".jpg", ".jpeg", ".png", ".tif", ".tiff"]
ADOBE_RGB_PROFILE = "/System/Library/ColorSync/Profiles/AdobeRGB1998.icc"

# Modulvariablen für die Wartezeiten (in Sekunden)
WAIT_BEFORE_CONVERSION = 5  # Wartezeit vor Beginn der Konvertierung
WAIT_BEFORE_MOVE = 5        # Wartezeit vor dem Verschieben der Datei

# Modulvariable für das Archiv-Suffix
ARCHIVE_SUFFIX = "-Archiv"

def send_notification(title: str, message: str) -> None:
    """
    Sendet eine macOS-Benachrichtigung mit dem gegebenen Titel und der Nachricht.
    
    :param title: Titel der Benachrichtigung.
    :param message: Nachricht der Benachrichtigung.
    """
    try:
        command = [
            "osascript",
            "-e",
            f'display notification "{message}" with title "{title}"'
        ]
        subprocess.run(command, check=True)
        typer.secho(f"Benachrichtigung gesendet: {title} - {message}", fg=typer.colors.GREEN)
    except subprocess.CalledProcessError as e:
        typer.secho(f"Fehler beim Senden der Benachrichtigung: {e}", fg=typer.colors.RED)

def convert_image_to_adobe_rgb(input_file: Path, output_file: Path) -> bool:
    """
    Konvertiert ein Bild in das Adobe RGB-Farbprofil und speichert es als JPEG.
    
    :param input_file: Pfad zur Eingabedatei (PNG/JPG/JPEG/TIF/TIFF).
    :param output_file: Pfad zur Ausgabedatei (JPEG).
    :return: True wenn erfolgreich, False sonst.
    """
    if input_file.suffix.lower() not in SUPPORTED_IMAGE_FORMATS:
        typer.secho("❌ Eingabedatei muss eine unterstützte Bilddatei sein (PNG, JPG, JPEG, TIF, TIFF).", fg=typer.colors.RED)
        return False
    
    if output_file.suffix.lower() != ".jpg":
        typer.secho("❌ Ausgabedatei muss eine JPG-Datei sein.", fg=typer.colors.RED)
        return False
    
    # Verwende SIPS, um das Format zu ändern und das Farbprofil anzupassen
    command = [
        "sips", "-s", "format", "jpeg", "-m", ADOBE_RGB_PROFILE, str(input_file), "--out", str(output_file)
    ]
    
    try:
        subprocess.run(command, check=True)
        typer.secho(f"✅ Bild erfolgreich konvertiert: {output_jpg.name}", fg=typer.colors.GREEN)
        return True
    except subprocess.CalledProcessError as e:
        typer.secho(f"❌ Fehler beim Konvertieren von {input_file}: {e}", fg=typer.colors.RED)
        return False

@app.command("convert-images")
def convert_images(
    source_dir: Path = typer.Argument(..., exists=True, file_okay=False, dir_okay=True, readable=True, help="Das Quellverzeichnis mit unterstützten Bilddateien (PNG, JPG, JPEG, TIF, TIFF)."),
    target_dir: Optional[Path] = typer.Option(None, "--target-dir", "-t", exists=False, file_okay=False, dir_okay=True, writable=True, readable=True, help="Das Zielverzeichnis für die konvertierten JPEG-Bilder. Wenn nicht angegeben, werden die JPEGs im Quellverzeichnis erstellt."),
    archive_directory: Optional[Path] = typer.Option(None, "--archive-directory", "-a", exists=False, file_okay=False, dir_okay=True, writable=True, readable=True, help="Das Archivverzeichnis, in das die Originaldateien nach erfolgreicher Konvertierung verschoben werden. Wenn nicht angegeben, wird automatisch ein Archivverzeichnis mit dem Suffix '-Archiv' erstellt und verwendet.")
):
    """
    Konvertiert alle unterstützten Bilddateien in einem Verzeichnis in AdobeRGB-JPEGs.
    
    Jeder unterstützte Bild wird als neues JPEG im Zielverzeichnis erstellt.
    Nach erfolgreicher Konvertierung wird die Originaldatei entweder in das angegebene Archivverzeichnis verschoben oder in ein automatisch erstelltes Archivverzeichnis mit dem Suffix '-Archiv'.
    """
    typer.secho("Starte die Konvertierung von unterstützten Bilddateien zu AdobeRGB-JPEGs...", fg=typer.colors.GREEN)
    
    if target_dir:
        target_dir.mkdir(parents=True, exist_ok=True)
        typer.secho(f"Zielverzeichnis festgelegt: {target_dir}", fg=typer.colors.BLUE)
    else:
        typer.secho("Kein Zielverzeichnis angegeben. Konvertierte JPEGs werden im Quellverzeichnis erstellt.", fg=typer.colors.BLUE)
        target_dir = source_dir
    
    # Bestimmen des Archivverzeichnisses
    if archive_directory:
        archive_directory.mkdir(parents=True, exist_ok=True)
        typer.secho(f"Archivverzeichnis festgelegt: {archive_directory}", fg=typer.colors.BLUE)
    else:
        # Automatisch ein Archivverzeichnis mit dem Suffix "-Archiv" erstellen
        archive_directory = source_dir.parent / f"{source_dir.name}{ARCHIVE_SUFFIX}"
        archive_directory.mkdir(parents=True, exist_ok=True)
        typer.secho(f"Kein Archivverzeichnis angegeben. Verwende automatisch erstelltes Archivverzeichnis: {archive_directory}", fg=typer.colors.BLUE)
    
    # Wartezeit vor Beginn der Konvertierung
    typer.secho(f"Warte {WAIT_BEFORE_CONVERSION} Sekunden vor Beginn der Konvertierung...", fg=typer.colors.YELLOW)
    time.sleep(WAIT_BEFORE_CONVERSION)
    
    # Suche nach unterstützten Bilddateien, die NICHT im Archivverzeichnis liegen
    png_files = [f for f in source_dir.rglob("*") if f.suffix.lower() in SUPPORTED_IMAGE_FORMATS and not f.is_relative_to(archive_directory)]
    
    if not png_files:
        typer.secho("Keine unterstützten Bilddateien im angegebenen Verzeichnis gefunden.", fg=typer.colors.YELLOW)
        # Hinweis: Keine gesonderte Benachrichtigung hier, weil Bilddaten häufig fehlen können, z.B. wenn gerade eine Masterdatei exportiert wird
        raise typer.Exit()
    
    typer.secho(f"Gefundene Bilddateien: {len(png_files)}", fg=typer.colors.BLUE)
    
    success_count = 0
    failed_files = []
    converted_files = []
    
    for image in png_files:
        output_jpg = target_dir / (image.stem + ".jpg")
        success = convert_image_to_adobe_rgb(image, output_jpg)
        if success:
            success_count += 1
            converted_files.append((image.name, target_dir.name))
            
            # Wartezeit vor dem Verschieben der Datei
            typer.secho(f"Warte {WAIT_BEFORE_MOVE} Sekunden bevor die Datei verarbeitet wird...", fg=typer.colors.YELLOW)
            time.sleep(WAIT_BEFORE_MOVE)
            
            # Verschieben der konvertierten Originaldatei in das Archivverzeichnis
            try:
                destination = archive_directory / image.name
                image.rename(destination)
                typer.secho(f"✅ Bild archiviert nach: {destination}", fg=typer.colors.GREEN)
            except Exception as e:
                typer.secho(f"❌ Fehler beim Archivieren von {image.name}: {e}", fg=typer.colors.RED)
                failed_files.append(image.name)
        else:
            failed_files.append(image.name)
    
    # Sende Benachrichtigung basierend auf den Ergebnissen
    if success_count > 0 and not failed_files:
        if success_count == 1:
            file_name, dir_name = converted_files[0]
            title = "Bild erfolgreich konvertiert"
            message = f"'{file_name}' wurde erfolgreich nach '{dir_name}' konvertiert."
        else:
            title = "Bilder erfolgreich konvertiert"
            message = f"{success_count} Bilddateien wurden erfolgreich nach '{target_dir.name}' konvertiert als AdobeRGB-JPG."
        send_notification(title, message)
    elif success_count > 0 and failed_files:
        if success_count == 1:
            success_message = f"{converted_files[0][0]} wurde erfolgreich nach {converted_files[0][1]} konvertiert."
        else:
            success_message = f"{success_count} Bilddateien wurden erfolgreich nach {target_dir.name} konvertiert."
        failed_message = f"{len(failed_files)} Bilddateien konnten nicht konvertiert oder archiviert werden: {', '.join(failed_files)}."
        title = "Teilweise Konvertierung abgeschlossen"
        message = f"{success_message}\n{failed_message}"
        send_notification(title, message)
    else:
        title = "Konvertierung fehlgeschlagen"
        message = "Keine Bilddateien konnten konvertiert werden."
        send_notification(title, message)
    
    typer.secho("Alle Bilddateien wurden verarbeitet.", fg=typer.colors.GREEN)