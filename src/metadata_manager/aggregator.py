from typing import Dict, Any
from metadata_manager.loader import get_relevant_metadata
from metadata_manager.utils import get_video_codec, get_bitrate, is_hevc_a
from metadata_manager.exif import get_creation_datetime
import logging

# Konfiguriere das Logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

def aggregate_metadata(file_path: str, include_source: bool = False) -> Dict[str, Any]:
    """
    Aggregiert Metadaten aus mehreren Quellen (ExifTool, FFprobe) und gibt ein kombiniertes Dictionary zurück.

    Args:
        file_path (str): Der Pfad zur Mediendatei.
        include_source (bool): Wenn True, enthält jede Eigenschaft Informationen über das Herkunftstool.

    Returns:
        dict: Ein Dictionary mit aggregierten Metadaten.
              Wenn include_source True ist, enthält jede Eigenschaft einen 'value' und einen 'source'.
              Andernfalls wird ein flaches Dictionary mit den Eigenschaftswerten zurückgegeben.
    """
    try:
        # Holen der Metadaten von ExifTool
        exif_metadata = get_relevant_metadata(file_path)

        if include_source:
            aggregated_metadata = {key: {"value": value, "source": "ExifTool"} 
                                  for key, value in exif_metadata.items() if value}
        else:
            aggregated_metadata = exif_metadata.copy()

        # Holen zusätzlicher Metadaten von FFprobe über utils.py
        video_codec = get_video_codec(file_path)
        bitrate = get_bitrate(file_path)
        hevc_a = is_hevc_a(file_path)

        if include_source:
            if video_codec:
                aggregated_metadata["VideoCodec"] = {"value": video_codec, "source": "FFprobe"}
            if bitrate:
                aggregated_metadata["Bitrate"] = {"value": bitrate, "source": "FFprobe"}
            aggregated_metadata["IsHEVCA"] = {"value": hevc_a, "source": "FFprobe"}
        else:
            if video_codec:
                aggregated_metadata["VideoCodec"] = video_codec
            if bitrate:
                aggregated_metadata["Bitrate"] = bitrate
            aggregated_metadata["IsHEVCA"] = hevc_a

        # Holen des Erstellungsdatums über exif.py
        creation_datetime = get_creation_datetime(file_path)
        if creation_datetime:
            if include_source:
                aggregated_metadata["CreationDateTime"] = {
                    "value": creation_datetime.isoformat(), 
                    "source": "ExifTool"
                }
            else:
                aggregated_metadata["CreationDateTime"] = creation_datetime.isoformat()

        return aggregated_metadata

    except Exception as e:
        logger.error(f"Fehler beim Aggregieren der Metadaten: {e}")
        raise