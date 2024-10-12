# src/mediaset_manager/models.py

from typing import Optional, List
from dataclasses import dataclass, field

@dataclass
class Medienset:
    name: str
    masterdatei: Optional[str] = None
    medienserver_datei: Optional[str] = None
    internet_datei_4k: Optional[str] = None
    internet_datei_hd: Optional[str] = None
    titelbild: Optional[str] = None
    projektdatei: Optional[str] = None
    metadata: Optional[dict] = field(default_factory=dict)  # Neu hinzugefügt
