# mediaset_manager/utils.py

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

def validate_video_files(video_files: list, min_size: int = 100 * 1024) -> list:
    """
    Überprüft, ob die Videodateien größer als min_size sind.

    Args:
        video_files (list): Liste von Path-Objekten zu den Videodateien.
        min_size (int): Mindestgröße in Bytes. Standard: 100 KB.

    Returns:
        list: Liste der Dateien, die die Mindestgröße nicht erfüllen.
    """
    small_files = []
    for video in video_files:
        if video.stat().st_size < min_size:
            small_files.append(video)
    return small_files