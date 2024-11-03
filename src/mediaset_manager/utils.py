# mediaset_manager/utils.py

from pathlib import Path
from typing import List
import ulid

def generate_ulid() -> str:
    """
    Generiert eine neue ULID.
    """
    return ulid.new().str
