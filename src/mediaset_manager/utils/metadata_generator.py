import os
import yaml
import ulid
from datetime import datetime

def generate_ulid() -> str:
    """
    Generiert eine neue ULID.
    """
    return ulid.new().str

def generate_video_metadata(
    title: str,
    erstellungsjahr: int,
    subtype: str = None,
    aufnahmejahr: int = None,
    aufnahmedatum: str = None,
    zeitraum: str = None,
    description: str = "",
    studio: str = "",
    keywords: str = "",
    album: str = "",
    video_editor: str = "",
    photographers: str = "",
    duration_in_seconds: int = 0,
    language: str = "de-CH"
) -> str:
    """
    Generiert die Metadaten.yaml Datei für ein Video-Medienset.
    
    Args:
        title (str): Titel des Mediensets.
        erstellungsjahr (int): Jahr der Originalerstellung.
        subtype (str, optional): Untertyp des Mediensets ("Ereignis" oder "Rückblick").
        aufnahmejahr (int, optional): Aufnahmejahr für Untertyp "Ereignis".
        aufnahmedatum (str, optional): Aufnahmedatum für Untertyp "Ereignis" (YYYY-MM-DD).
        zeitraum (str, optional): Zeitraum für Untertyp "Rückblick".
        description (str, optional): Beschreibung des Mediensets.
        studio (str, optional): Studio oder Ort der Produktion.
        keywords (str, optional): Schlüsselwörter, durch Kommata getrennt.
        album (str, optional): Album-Name.
        video_editor (str, optional): Personen für den Videoschnitt, durch Kommata getrennt.
        photographers (str, optional): Personen für die Kameraführung, durch Kommata getrennt.
        duration_in_seconds (int, optional): Dauer des Videos in Sekunden.
        language (str, optional): Sprache der Metadaten-Datei (ISO-639-1). Standard: "de-CH".
    
    Returns:
        str: Pfad zur gespeicherten Metadaten.yaml Datei.
    
    Raises:
        ValueError: Wenn erforderliche Felder fehlen oder ungültig sind.
    """
    # Validierung des Subtypes
    if subtype:
        if subtype.lower() == "ereignis":
            untertyp = "Ereignis"
            if not (aufnahmejahr or aufnahmedatum):
                raise ValueError("Für Untertyp 'Ereignis' ist entweder 'Aufnahmejahr' oder 'Aufnahmedatum' obligatorisch.")
            if aufnahmedatum:
                try:
                    aufnahmedatum_obj = datetime.strptime(aufnahmedatum, "%Y-%m-%d")
                    aufnahmedatum_formatted = aufnahmedatum_obj.strftime("%Y-%m-%d")
                except ValueError:
                    raise ValueError("Ungültiges Datumsformat für Aufnahmedatum. Bitte verwenden Sie YYYY-MM-DD.")
            else:
                aufnahmedatum_formatted = None
            zeitraum = None
        elif subtype.lower() == "rückblick":
            untertyp = "Rückblick"
            if not zeitraum:
                raise ValueError("Für Untertyp 'Rückblick' ist das Feld 'Zeitraum' obligatorisch.")
            aufnahmejahr = None
            aufnahmedatum_formatted = None
        else:
            raise ValueError(f"Unbekannter Untertyp '{subtype}'. Unterstützt werden nur 'Ereignis' und 'Rückblick'.")
    else:
        untertyp = None
        aufnahmejahr = None
        aufnahmedatum_formatted = None
        zeitraum = None

    # Validierung der Sprache (derzeit nur 'de-CH' unterstützt)
    if language != "de-CH":
        raise ValueError("Derzeit wird nur die Sprache 'de-CH' unterstützt.")

    # Bestimme den Verzeichnisnamen
    directory_name = f"{erstellungsjahr}_{title.replace(' ', '_')}"
    if not os.path.exists(directory_name):
        os.makedirs(directory_name)
    
    metadata_filename = "Metadaten.yaml"
    metadata_path = os.path.join(directory_name, metadata_filename)
    
    # Überprüfe, ob die Metadaten-Datei bereits existiert
    existing_id = None
    if os.path.exists(metadata_path):
        # Lade die bestehende Metadaten-Datei und extrahiere die Id
        with open(metadata_path, 'r', encoding='utf-8') as yaml_file:
            existing_metadaten = yaml.safe_load(yaml_file)
        existing_id = existing_metadaten.get("Id")
    
    # Generiere eine ULID für das Medienset, falls keine bestehende Id vorhanden ist
    if not existing_id:
        medienset_id = generate_ulid()
    else:
        medienset_id = existing_id
    
    # Erstelle die Metadatenstruktur gemäß der Kurmann-Medienset Spezifikation in Deutsch
    metadaten = {
        "Spezifikationsversion": "0.6",
        "Id": medienset_id,
        "Titel": title,
        "Typ": "Video",
        "Erstellung": str(erstellungsjahr),
        "Mediatheksdatum": datetime.now().strftime("%Y-%m-%d"),
    }
    
    if untertyp:
        metadaten["Untertyp"] = untertyp
    
    if subtype and subtype.lower() == "ereignis":
        if aufnahmejahr:
            metadaten["Aufnahmejahr"] = str(aufnahmejahr)
        if aufnahmedatum_formatted:
            metadaten["Aufnahmedatum"] = aufnahmedatum_formatted
    elif subtype and subtype.lower() == "rückblick":
        metadaten["Zeitraum"] = zeitraum
    
    # Hinzufügen optionaler Felder
    if description:
        metadaten["Beschreibung"] = description
    if keywords:
        metadaten["Schlüsselwörter"] = [kw.strip() for kw in keywords.split(',')]
    if album:
        metadaten["Album"] = album
    if video_editor:
        metadaten["Videoschnitt"] = [ve.strip() for ve in video_editor.split(',')]
    if photographers:
        metadaten["Kameraführung"] = [ph.strip() for ph in photographers.split(',')]
    if duration_in_seconds > 0:
        metadaten["Dauer_in_Sekunden"] = duration_in_seconds
    if studio:
        metadaten["Studio"] = studio
    
    # Bereinige leere Felder und Listen
    metadaten = {k: v for k, v in metadaten.items() if v and (not isinstance(v, list) or len(v) > 0)}
    
    # Speichere die Metadaten in der YAML-Datei
    with open(metadata_path, 'w', encoding='utf-8') as yaml_file:
        yaml.dump(metadaten, yaml_file, allow_unicode=True, sort_keys=False, default_flow_style=False)
    
    return metadata_path