# mediaset_manager/commands/create_homemovie_metadata_file.py

import typer
import yaml
from datetime import datetime
from pathlib import Path
from typing import Optional
from mediaset_manager.utils import generate_ulid
import subprocess
import json
import re

app = typer.Typer()

def extract_metadata(file_path):
    command = ['exiftool', '-j', str(file_path)]
    result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    if result.returncode != 0:
        raise Exception(f"Error running exiftool: {result.stderr}")
    metadata = json.loads(result.stdout)[0]
    return metadata

def parse_date(date_str):
    for fmt in ('%Y:%m:%d %H:%M:%S', '%Y:%m:%d', '%Y-%m-%d'):
        try:
            return datetime.strptime(date_str, fmt).date()
        except ValueError:
            continue
    return None

def extract_date_and_title_from_name(name):
    # Match patterns like 'YYYY-MM-DD Titel'
    match = re.match(r'(\d{4}-\d{2}-\d{2})\s+(.*)', name)
    if match:
        date_part = match.group(1)
        title_part = match.group(2)
        date_obj = parse_date(date_part)
        if date_obj:
            return date_obj.strftime('%Y-%m-%d'), title_part
    return None, name

@app.command("create-homemovie-metadata-file")
def create_homemovie_metadata_file(
    titel: Optional[str] = typer.Option(None, help="Titel des Mediensets"),
    jahr: Optional[int] = typer.Option(None, help="Jahr des Mediensets"),
    typ: Optional[str] = typer.Option(None, help="Typ des Mediensets"),
    untertyp: Optional[str] = typer.Option(None, help="Untertyp des Mediensets (Ereignis/Rückblick)",
                                     callback=lambda ctx, param, value: value.capitalize() if value else value),
    aufnahmedatum: Optional[str] = typer.Option(None, help="Aufnahmedatum (YYYY-MM-DD) für Untertyp 'Ereignis'"),
    zeitraum: Optional[str] = typer.Option(None, help="Zeitraum für Untertyp 'Rückblick'"),
    beschreibung: Optional[str] = typer.Option(None, help="Beschreibung des Mediensets"),
    notiz: Optional[str] = typer.Option(None, help="Interne Bemerkungen zum Medienset"),
    schluesselwoerter: Optional[str] = typer.Option(None, help="Schlüsselwörter zur Kategorisierung, durch Komma getrennt"),
    album: Optional[str] = typer.Option(None, help="Name des Albums oder der Sammlung"),
    videoschnitt: Optional[str] = typer.Option(None, help="Personen für den Videoschnitt, durch Komma getrennt"),
    kamerafuehrung: Optional[str] = typer.Option(None, help="Personen für die Kameraführung, durch Komma getrennt"),
    dauer_in_sekunden: Optional[int] = typer.Option(None, help="Gesamtdauer des Films in Sekunden"),
    studio: Optional[str] = typer.Option(None, help="Studio oder Ort der Produktion"),
    filmfassung_name: Optional[str] = typer.Option(None, help="Name der Filmfassung"),
    filmfassung_beschreibung: Optional[str] = typer.Option(None, help="Beschreibung der Filmfassung"),
    metadata_source: Optional[Path] = typer.Option(None, help="Datei zur Extraktion von Metadaten"),
    output: Optional[Path] = typer.Option(None, "--output", "-o", help="Ausgabepfad inklusive Dateiname (z.B., /path/to/Metadaten.yaml)")
):
    """
    Erstellt die Metadaten.yaml-Datei im angegebenen Verzeichnis oder im aktuellen Verzeichnis.
    """
    # Metadaten aus Datei extrahieren, falls angegeben
    metadata = {}
    if metadata_source:
        try:
            metadata = extract_metadata(metadata_source)
        except Exception as e:
            typer.secho(f"Fehler beim Extrahieren der Metadaten: {e}", fg=typer.colors.RED)
            raise typer.Exit(code=1)
    
    # Fülle fehlende Felder mit Metadaten
    if not titel:
        name = metadata.get('Title') or metadata.get('DisplayName') or metadata.get('Name')
        if name:
            extracted_date, extracted_title = extract_date_and_title_from_name(name)
            if extracted_date and not aufnahmedatum:
                aufnahmedatum = extracted_date
            else:
                # Wenn kein Datum aus dem Titel extrahiert werden kann
                if not aufnahmedatum and metadata_source:
                    typer.secho(
                        "Warnung: Kein Aufnahmedatum aus dem Titel extrahiert. Verwende das Änderungsdatum der Datei.",
                        fg=typer.colors.YELLOW,
                    )
                    modification_time = datetime.fromtimestamp(
                        metadata_source.stat().st_mtime
                    )
                    aufnahmedatum = modification_time.strftime("%Y-%m-%d")
            titel = extracted_title
        else:
            titel = None

    if not jahr:
        if aufnahmedatum:
            jahr = parse_date(aufnahmedatum).year
        else:
            date_fields = ['ContentCreateDate', 'CreateDate', 'ModifyDate', 'MediaCreateDate', 'MediaModifyDate', 'CreationDate']
            for date_field in date_fields:
                date_str = metadata.get(date_field)
                if date_str:
                    date_obj = parse_date(date_str)
                    if date_obj:
                        jahr = date_obj.year
                        if not aufnahmedatum:
                            aufnahmedatum = date_obj.strftime('%Y-%m-%d')
                        break
    if not beschreibung:
        beschreibung = metadata.get('Description')
    if not schluesselwoerter:
        keywords = metadata.get('Keywords') or metadata.get('Keyword')
        if keywords:
            if isinstance(keywords, list):
                schluesselwoerter = ','.join(keywords)
            else:
                schluesselwoerter = keywords
    if not album:
        album = metadata.get('Album')
    if not videoschnitt:
        videoschnitt = metadata.get('Producer')
    if not kamerafuehrung:
        director = metadata.get('Director')
        if director:
            kamerafuehrung = director
    if not dauer_in_sekunden:
        duration_str = metadata.get('Duration')
        if duration_str:
            try:
                if ':' in duration_str:
                    # Dauer in Form von '0:01:19'
                    time_parts = list(map(float, duration_str.split(':')))
                    if len(time_parts) == 3:
                        h, m, s = time_parts
                    elif len(time_parts) == 2:
                        h = 0
                        m, s = time_parts
                    else:
                        h = m = 0
                        s = time_parts[0]
                    dauer_in_sekunden = int(h * 3600 + m * 60 + s)
                else:
                    # Dauer in Form von '19.83 s'
                    dauer_in_sekunden = int(float(duration_str.replace(' s', '').strip()))
            except ValueError:
                pass
    if not studio:
        studio = metadata.get('Studio')
    if not filmfassung_name:
        filmfassung_name = metadata.get('FilmfassungName')
    if not filmfassung_beschreibung:
        filmfassung_beschreibung = metadata.get('FilmfassungBeschreibung')
    if not untertyp:
        untertyp = metadata.get('AppleProappsShareCategory')
        if untertyp:
            untertyp = untertyp.capitalize()
    if not typ:
        typ = metadata.get('Genre')
    if not typ:
        typ = "Familienfilm"  # Standardwert

    # Bestimme den Ausgabepfad
    if output:
        metadata_path = output
    else:
        metadata_path = Path("Metadaten.yaml")

    # Generiere eine neue ULID
    medienset_id = generate_ulid()

    # Überprüfe erforderliche Felder
    fehlende_felder = []
    if not titel:
        fehlende_felder.append("titel")
    if not jahr:
        fehlende_felder.append("jahr")
    if not typ:
        fehlende_felder.append("typ")
    if not untertyp:
        fehlende_felder.append("untertyp")
    if untertyp == "Ereignis" and not aufnahmedatum:
        fehlende_felder.append("aufnahmedatum")
    if untertyp == "Rückblick" and not zeitraum:
        fehlende_felder.append("zeitraum")
    if fehlende_felder:
        typer.secho(f"Die folgenden erforderlichen Felder fehlen: {', '.join(fehlende_felder)}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    metadaten = {
        "Spezifikationsversion": "1.0",
        "Id": medienset_id,
        "Titel": titel,
        "Typ": typ,
        "Untertyp": untertyp,
        "Jahr": str(jahr),
        "Version": 1
    }

    if untertyp == "Ereignis":
        metadaten["Aufnahmedatum"] = aufnahmedatum
    elif untertyp == "Rückblick":
        metadaten["Zeitraum"] = zeitraum

    if beschreibung:
        metadaten["Beschreibung"] = beschreibung
    if notiz:
        metadaten["Notiz"] = notiz
    if schluesselwoerter:
        metadaten["Schlüsselwörter"] = [kw.strip() for kw in schluesselwoerter.split(",")]
    if album:
        metadaten["Album"] = album
    if videoschnitt:
        metadaten["Videoschnitt"] = [vs.strip() for vs in videoschnitt.replace(',', ';').split(";")]
    if kamerafuehrung:
        metadaten["Kameraführung"] = [kf.strip() for kf in kamerafuehrung.replace(',', ';').split(";")]
    if dauer_in_sekunden:
        metadaten["Dauer_in_Sekunden"] = dauer_in_sekunden
    if studio:
        metadaten["Studio"] = studio
    if filmfassung_name:
        metadaten["Filmfassung_Name"] = filmfassung_name
    if filmfassung_beschreibung:
        metadaten["Filmfassung_Beschreibung"] = filmfassung_beschreibung

    # Erstelle das Ausgabeverzeichnis, falls es nicht existiert
    if metadata_path.parent and not metadata_path.parent.exists():
        metadata_path.parent.mkdir(parents=True, exist_ok=True)

    # Schreibe die Metadaten.yaml-Datei
    try:
        with open(metadata_path, 'w', encoding='utf-8') as yaml_file:
            yaml.dump(metadaten, yaml_file, allow_unicode=True, sort_keys=False, default_flow_style=False)
        typer.secho(f"Metadaten.yaml wurde erstellt unter '{metadata_path}'.", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der Metadaten.yaml: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)