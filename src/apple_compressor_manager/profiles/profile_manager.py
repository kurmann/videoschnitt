# src/apple_compressor_manager/profiles/profile_manager.py

import os
from typing import List
import logging

logger = logging.getLogger(__name__)

# Standardverzeichnis für Compressor-Profile auf MacOS
DEFAULT_PROFILE_DIR = os.path.expanduser("~/Library/Application Support/Compressor/Settings/")

# Dateiendung für Compressor-Profile
PROFILE_EXTENSION = ".compressorsetting"

def get_profile_path(profile_name: str, profile_dir: str = DEFAULT_PROFILE_DIR) -> str:
    """
    Gibt den vollständigen Pfad zum Compressor-Profil basierend auf dem Profilnamen zurück.

    ## Argumente:
    - **profile_name** (*str*): Der Name des Compressor-Profils ohne Pfad und Dateiendung.
    - **profile_dir** (*str, optional*): Das Verzeichnis, in dem die Compressor-Profile gespeichert sind. 
                                     Standard ist das MacOS-Standardverzeichnis.

    ## Rückgabe:
    - **str**: Der vollständige Pfad zum Compressor-Profil.

    ## Beispielaufruf:
    ```python
    profile_path = get_profile_path("MeinCompressorProfil")
    ```
    """
    filename = f"{profile_name}{PROFILE_EXTENSION}"
    profile_path = os.path.join(profile_dir, filename)
    logger.debug(f"Aufgelöster Profilpfad: {profile_path}")
    return profile_path

def list_profiles(profile_dir: str = DEFAULT_PROFILE_DIR) -> List[str]:
    """
    Listet alle verfügbaren Compressor-Profile im angegebenen Verzeichnis auf.

    ## Argumente:
    - **profile_dir** (*str, optional*): Das Verzeichnis, in dem die Compressor-Profile gespeichert sind.
                                         Standard ist das MacOS-Standardverzeichnis.

    ## Rückgabe:
    - **List[str]**: Eine Liste der verfügbaren Profilnamen ohne Pfade und Dateiendungen.

    ## Beispielaufruf:
    ```python
    profiles = list_profiles()
    ```
    """
    if not os.path.isdir(profile_dir):
        logger.error(f"Profilverzeichnis existiert nicht: {profile_dir}")
        return []
    
    profiles = []
    for file in os.listdir(profile_dir):
        if file.endswith(PROFILE_EXTENSION):
            profile_name = os.path.splitext(file)[0]
            profiles.append(profile_name)
            logger.debug(f"Gefundenes Profil: {profile_name}")
    
    logger.info(f"Gefundene Profile: {profiles}")
    return profiles

def validate_profile(profile_name: str, profile_dir: str = DEFAULT_PROFILE_DIR) -> bool:
    """
    Überprüft, ob das angegebene Compressor-Profil existiert und lesbar ist.

    ## Argumente:
    - **profile_name** (*str*): Der Name des Compressor-Profils ohne Pfad und Dateiendung.
    - **profile_dir** (*str, optional*): Das Verzeichnis, in dem die Compressor-Profile gespeichert sind.
                                         Standard ist das MacOS-Standardverzeichnis.

    ## Rückgabe:
    - **bool**: True, wenn das Profil existiert und lesbar ist, sonst False.

    ## Beispielaufruf:
    ```python
    ist_gueltig = validate_profile("MeinCompressorProfil")
    ```
    """
    profile_path = get_profile_path(profile_name, profile_dir)
    if not os.path.exists(profile_path):
        logger.error(f"Profil existiert nicht: {profile_path}")
        return False
    if not os.access(profile_path, os.R_OK):
        logger.error(f"Profil ist nicht lesbar: {profile_path}")
        return False
    logger.info(f"Profil ist gültig: {profile_path}")
    return True
