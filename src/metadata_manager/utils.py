# src/metadata_manager/utils.py

import subprocess
import json
import os
from typing import Any, Dict, Optional
import logging

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def get_metadata_with_ffmpeg(file_path: str) -> Dict[str, Any]:
    """
    Extrahiert relevante Metadaten aus einer Datei mithilfe von FFprobe.
    """
    metadata = {}
    video_codec = get_video_codec(file_path)
    bitrate = get_bitrate(file_path)
    hevc_a = is_hevc_a(file_path)
    if video_codec:
        metadata["VideoCodec"] = video_codec
    if bitrate:
        metadata["Bitrate"] = bitrate
    metadata["IsHEVCA"] = hevc_a
    return metadata

def get_video_codec(filepath) -> Optional[str]:
    """
    Führt ffprobe aus, um den Codec der Videodatei zu ermitteln.
    """
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=codec_name', '-of', 'json', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    
    # JSON-Antwort parsen
    probe = json.loads(result.stdout)
    
    # Überprüfen, ob der Videostream vorhanden ist und den Codec extrahieren
    if 'streams' in probe and len(probe['streams']) > 0 and 'codec_name' in probe['streams'][0]:
        return probe['streams'][0]['codec_name']
    else:
        return None

def get_bitrate(filepath: str) -> Optional[int]:
    """
    Führt ffprobe aus, um die Bitrate der Videodatei zu ermitteln.

    Args:
        filepath (str): Der Pfad zur Videodatei.

    Returns:
        int | None: Die Bitrate in Bit pro Sekunde, oder None, wenn nicht ermittelt werden konnte.
    """
    if not os.path.exists(filepath):
        logger.error(f"Datei existiert nicht: {filepath}")
        return None

    cmd = [
        'ffprobe',
        '-v', 'error',
        '-select_streams', 'v:0',
        '-show_entries', 'stream=bit_rate',
        '-of', 'json',
        filepath
    ]
    try:
        result = subprocess.run(cmd, check=True, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
        probe = json.loads(result.stdout)
        bit_rate_str = probe.get('streams', [{}])[0].get('bit_rate')
        if bit_rate_str:
            bit_rate = int(bit_rate_str)
            logger.debug(f"Bitrate für {filepath} ermittelt: {bit_rate} bit/s")
            return bit_rate
        else:
            logger.warning(f"Bitrate konnte nicht ermittelt werden für: {filepath}")
            return None
    except subprocess.CalledProcessError as e:
        logger.error(f"ffprobe Fehler für {filepath}: {e.stderr.strip()}")
        return None
    except (json.JSONDecodeError, ValueError) as e:
        logger.error(f"Fehler beim Parsen der Bitrate für {filepath}: {e}")
        return None

def is_hevc_a(filepath: str) -> bool:
    """
    Bestimmt, ob es sich bei der Datei um HEVC-A handelt, basierend auf der Bitrate.
    Dateien mit einer Bitrate über 80 Mbit/s werden als HEVC-A betrachtet.

    Args:
        filepath (str): Der Pfad zur Videodatei.

    Returns:
        bool: True, wenn die Datei HEVC-A ist, sonst False.
    """
    bitrate = get_bitrate(filepath)
    if bitrate and bitrate > 80 * 1024 * 1024:
        logger.debug(f"Datei {filepath} ist HEVC-A (Bitrate: {bitrate} bit/s)")
        return True
    logger.debug(f"Datei {filepath} ist nicht HEVC-A (Bitrate: {bitrate} bit/s)" if bitrate else f"Bitrate konnte nicht ermittelt werden für: {filepath}")
    return False
