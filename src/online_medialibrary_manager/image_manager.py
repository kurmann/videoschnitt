# onlinemedialibrarymanager/image_manager.py

"""
Das Modul 'image_manager' enthält Funktionen zur Bildverarbeitung,
einschließlich der Erstellung eines OpenGraph-Bildes für die Verwendung in sozialen Medien.
"""

import os
import subprocess
import logging

def create_og_image(input_image: str, output_image: str = 'og-image.jpg') -> None:
    """
    Erzeugt ein OpenGraph-Bild aus dem gegebenen Eingabebild.
    Das Ausgabeformat ist JPEG mit einer Auflösung von 1536x804 Pixel.
    Wenn nötig, wird das Bild zugeschnitten, um das Seitenverhältnis beizubehalten.
    Das resultierende Bild wird komprimiert, um die Dateigröße unter 300 KB zu halten.

    Args:
        input_image (str): Pfad zum Eingabebild.
        output_image (str): Pfad zum Ausgabebild (Standard: 'og-image.jpg').
    """
    if not os.path.exists(input_image):
        logging.error(f"Eingabebild {input_image} existiert nicht.")
        return

    # Zielauflösung
    target_width = 1536
    target_height = 804

    # Temporäre Datei für die Verarbeitung
    temp_output = output_image + '.temp.jpg'

    # Zuschneiden und Skalieren des Bildes auf die Zielauflösung
    command = [
        'sips',
        '--resampleHeightWidth', str(target_height), str(target_width),
        input_image,
        '--out', temp_output
    ]

    try:
        subprocess.run(command, check=True)
    except subprocess.CalledProcessError as e:
        logging.error(f"Fehler beim Verarbeiten des Bildes: {e}")
        return

    # Komprimierung anwenden, um Dateigröße unter 300 KB zu halten
    # Wir verwenden die Qualitätseinstellung von sips
    quality = 90
    max_size_kb = 290

    while True:
        # Setze die Qualität
        compress_command = [
            'sips',
            '--setProperty', 'formatOptions', str(quality),
            temp_output,
            '--out', output_image
        ]

        try:
            subprocess.run(compress_command, check=True)
        except subprocess.CalledProcessError as e:
            logging.error(f"Fehler beim Komprimieren des Bildes mit Qualität {quality}: {e}")
            break

        # Überprüfen der Dateigröße
        file_size_kb = os.path.getsize(output_image) / 1024
        if file_size_kb <= max_size_kb or quality <= 10:
            break
        else:
            # Qualität reduzieren und erneut versuchen
            quality -= 5

    # Temporäre Datei löschen
    os.remove(temp_output)

    logging.info(f"OpenGraph-Bild erstellt: {output_image} (Größe: {file_size_kb:.2f} KB)")