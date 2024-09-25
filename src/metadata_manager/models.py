# src/metadata_manager/models.py

from dataclasses import dataclass, field
from typing import List, Optional
from datetime import datetime

@dataclass
class Metadata:
    """
    Repräsentiert die Metadaten einer Mediendatei.
    """
    file_name: str
    directory: str
    file_size: int
    file_modification_datetime: datetime
    file_type: str
    mime_type: str
    create_date: Optional[datetime] = None
    duration: Optional[str] = None
    audio_format: Optional[str] = None
    image_width: Optional[int] = None
    image_height: Optional[int] = None
    compressor_id: Optional[str] = None
    compressor_name: Optional[str] = None
    bit_depth: Optional[int] = None
    video_frame_rate: Optional[str] = None
    title: Optional[str] = None
    album: Optional[str] = None
    description: Optional[str] = None
    copyright: Optional[str] = None
    author: Optional[str] = None
    keywords: Optional[str] = None
    avg_bitrate: Optional[int] = None
    producer: Optional[str] = None
    studio: Optional[str] = None
    producers: List[str] = field(default_factory=list)
    directors: List[str] = field(default_factory=list)

    def __post_init__(self):
        # Optional: Füge Validierungen oder weitere Initialisierungen hinzu
        pass

    def __str__(self):
        return f"Metadata(file_name={self.file_name}, title={self.title}, author={self.author})"
