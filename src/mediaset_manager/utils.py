# mediaset_manager/utils.py

from pathlib import Path
from typing import List
import ulid

def sanitize_filename(name: str) -> str:
    """
    Ersetzt Umlaute und Sonderzeichen und ersetzt Leerzeichen durch Underscores,
    um einen Dateinamen konform zu gestalten.
    """
    replacements = {
        'ä': 'ae', 'ö': 'oe', 'ü': 'ue', 'Ä': 'Ae', 'Ö': 'Oe', 'Ü': 'Ue', 'ß': 'ss'
    }
    for original, replacement in replacements.items():
        name = name.replace(original, replacement)
    # Ersetze Leerzeichen durch Underscores
    name = name.replace(' ', '_')
    # Entferne nicht-ASCII-Zeichen außer '_' und '-'
    name = ''.join(c for c in name if c.isalnum() or c in ['_', '-'])
    return name

def generate_ulid() -> str:
    """
    Generiert eine neue ULID.
    """
    return ulid.new().str
