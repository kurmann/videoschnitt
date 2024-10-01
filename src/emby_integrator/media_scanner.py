# src/emby_integrator/media_scanner.py

import os
from typing import Dict, List, Tuple
import logging

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def is_iso_date(name: str) -> bool:
    """
    Überprüft, ob der gegebene Dateiname mit einem ISO-Datum (YYYY-MM-DD) beginnt.
    
    Args:
        name (str): Der Dateiname ohne Erweiterung.
    
    Returns:
        bool: True, wenn der Name mit einem ISO-Datum beginnt, sonst False.
    """
    if len(name) < 10:
        return False
    date_part = name[:10]
    return date_part.count('-') == 2 and date_part.replace('-', '').isdigit()

def scan_media_directory(directory: str) -> Tuple[Dict[str, Dict[str, str]], List[Dict[str, List[str]]], List[str]]:
    """
    Scannt ein Verzeichnis nach Bilddateien (.png, .jpg, .jpeg) und QuickTime-Dateien (.mov).
    Gruppiert die Dateien basierend auf den Bilddateien und sucht passende QuickTime-Dateien.
    
    Args:
        directory (str): Der Pfad zum zu scannenden Verzeichnis.
    
    Returns:
        Tuple[
            Dict[str, Dict[str, str]],  # complete_sets: base_name -> {'image': image_file, 'video': video_file}
            List[Dict[str, List[str]]],  # incomplete_sets: list of {'image': image_file, 'videos': [video_files]}
            List[str]  # unmatched_videos: list of videos without images
        ]
    """
    if not os.path.isdir(directory):
        logger.error(f"Das Verzeichnis {directory} existiert nicht.")
        return {}, [], []
    
    # Unterstützte Dateierweiterungen
    image_extensions = {'.png', '.jpg', '.jpeg'}
    video_extension = '.mov'
    
    # Dictionaries zur Speicherung der Dateien
    images: Dict[str, str] = {}  # base_name: image_file
    videos: List[str] = []
    
    # Durchsuche das Verzeichnis nach Bild- und Videodateien
    for entry in os.listdir(directory):
        entry_path = os.path.join(directory, entry)
        if not os.path.isfile(entry_path):
            continue

        file_ext = os.path.splitext(entry)[1].lower()
        base_name = os.path.splitext(entry)[0]

        if file_ext in image_extensions:
            # Überprüfe, ob der Dateiname mit einem ISO-Datum beginnt
            if is_iso_date(base_name):
                images[base_name] = entry
        elif file_ext == video_extension:
            # Überprüfe, ob der Dateiname mit einem ISO-Datum beginnt
            if is_iso_date(base_name):
                videos.append(entry)
    
    logger.debug(f"Gefundene Bilddateien: {images}")
    logger.debug(f"Gefundene Videodateien: {videos}")
    
    # Gruppierung basierend auf Bilddateien
    complete_sets: Dict[str, Dict[str, str]] = {}
    incomplete_sets: List[Dict[str, List[str]]] = []
    matched_videos: set = set()
    
    for image_base, image_file in images.items():
        matching_videos = [video for video in videos if video.startswith(image_base + "-")]
        if len(matching_videos) == 1:
            complete_sets[image_base] = {
                "image": image_file,
                "video": matching_videos[0]
            }
            matched_videos.add(matching_videos[0])
        else:
            # Unvollständige Gruppe (keine oder mehrere Videos)
            incomplete_sets.append({
                "image": image_file,
                "videos": matching_videos  # könnte leer oder mehrere sein
            })
    
    # Identifiziere Videos ohne passende Bilddatei
    unmatched_videos = [video for video in videos if video not in matched_videos and not any(video.startswith(image_base + "-") for image_base in images)]
    
    logger.debug(f"Vollständige Mediengruppen: {complete_sets}")
    logger.debug(f"Unvollständige Mediengruppen: {incomplete_sets}")
    logger.debug(f"Unvollständige Videodateien: {unmatched_videos}")
    
    return complete_sets, incomplete_sets, unmatched_videos