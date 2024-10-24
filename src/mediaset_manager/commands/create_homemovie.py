# mediaset_manager/commands/create_homemovie.py

import typer
from pathlib import Path
from typing import Optional
from mediaset_manager.utils import sanitize_filename, generate_ulid
import shutil
from datetime import datetime
import re
import subprocess
import json
import yaml

app = typer.Typer()

# Modulvariable für das Schema
SCHEMA_URL = "https://raw.githubusercontent.com/kurmann/videoschnitt/main/docs/schema/medienset/familienfilm.yaml"

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

def split_list_field(value):
    """
    Splits a string into a list using commas and semicolons as separators.
    """
    if not value:
        return []
    return [item.strip() for item in re.split(r'[;,]', value)]

def create_homemovie(
    metadata_source: Path,
    additional_media_dir: Optional[Path] = None,
    titel: Optional[str] = None,
    jahr: Optional[int] = None,
    untertyp: Optional[str] = None,
    aufnahmedatum: Optional[str] = None,
    zeitraum: Optional[str] = None,
    beschreibung: Optional[str] = None,
    notiz: Optional[str] = None,
    schluesselwoerter: Optional[str] = None,
    album: Optional[str] = None,
    videoschnitt: Optional[str] = None,
    kamerafuehrung: Optional[str] = None,
    dauer_in_sekunden: Optional[int] = None,
    studio: Optional[str] = None,
    filmfassung_name: Optional[str] = None,
    filmfassung_beschreibung: Optional[str] = None,
    no_prompt: bool = False,
):
    """
    Erstellt ein Medienset-Verzeichnis und eine Metadaten.yaml-Datei basierend auf einer Videodatei.
    Dateien werden gesucht, klassifiziert und in das Medienset-Verzeichnis verschoben.
    Es erfolgt eine Bestätigung vor dem Verschieben, es sei denn, 'no_prompt' wurde angegeben.
    Bei bestehenden Dateien wird nachgefragt, ob diese überschrieben werden sollen.
    """

    # Überprüfen, ob die Metadatenquelle existiert
    if not metadata_source.is_file():
        typer.secho(
            f"Die Metadatenquelle '{metadata_source}' existiert nicht.",
            fg=typer.colors.RED,
        )
        raise typer.Exit(code=1)

    # Metadaten aus der Metadatenquelle extrahieren (vor dem Verschieben)
    try:
        metadata = extract_metadata(metadata_source)
    except Exception as e:
        typer.secho(f"Fehler beim Extrahieren der Metadaten: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Ermitteln des Titels aus den Metadaten
    if not titel:
        name = (
            metadata.get("Title")
            or metadata.get("DisplayName")
            or metadata.get("Name")
        )
        if name:
            extracted_date, extracted_title = extract_date_and_title_from_name(name)
            if extracted_date and not aufnahmedatum:
                aufnahmedatum = extracted_date
            else:
                # Wenn kein Datum aus dem Titel extrahiert werden kann
                if not aufnahmedatum:
                    typer.secho(
                        "Warnung: Kein Aufnahmedatum aus dem Titel extrahiert. Verwende das Änderungsdatum der Datei.",
                        fg=typer.colors.YELLOW,
                    )
                    modification_time = datetime.fromtimestamp(
                        metadata_source.stat().st_mtime
                    )
                    aufnahmedatum = modification_time.strftime("%Y-%m-%d")
            titel = extracted_title
            full_title = name  # Vollständiger Titel inkl. Datum für das Matching
        else:
            typer.secho(
                "Kein Titel konnte aus den Metadaten extrahiert werden.",
                fg=typer.colors.RED,
            )
            raise typer.Exit(code=1)
    else:
        # Wenn 'titel' angegeben wurde, verwenden wir diesen sowohl als Titel als auch für das Matching
        titel = titel
        full_title = titel

    # Ermitteln des Jahres
    if not jahr:
        if aufnahmedatum:
            jahr = parse_date(aufnahmedatum).year
        else:
            date_fields = [
                "ContentCreateDate",
                "CreateDate",
                "ModifyDate",
                "MediaCreateDate",
                "MediaModifyDate",
                "CreationDate",
            ]
            for date_field in date_fields:
                date_str = metadata.get(date_field)
                if date_str:
                    date_obj = parse_date(date_str)
                    if date_obj:
                        jahr = date_obj.year
                        if not aufnahmedatum:
                            aufnahmedatum = date_obj.strftime("%Y-%m-%d")
                        break
            if not jahr:
                # Verwende das Änderungsdatum der Metadatenquelle
                modification_time = datetime.fromtimestamp(
                    metadata_source.stat().st_mtime
                )
                jahr = modification_time.year
                if not aufnahmedatum:
                    aufnahmedatum = modification_time.strftime("%Y-%m-%d")

    # Setze Standardwerte für 'typ' und 'untertyp' falls nicht angegeben
    typ = "Familienfilm"  # Immer 'Familienfilm'
    if untertyp:
        untertyp = untertyp.capitalize()
    else:
        untertyp = metadata.get('AppleProappsShareCategory', 'Ereignis').capitalize()

    # Verzeichnis 1 (Verzeichnis der Metadatenquelle)
    verzeichnis1 = metadata_source.parent

    # Funktion zum Finden von Dateien, deren Dateinamen mit dem vollständigen Titel beginnen
    def find_matching_files(directory: Path, full_title: str):
        matching_files = []
        if directory and directory.is_dir():
            for file in directory.iterdir():
                if file.is_file() and file.name.startswith(full_title):
                    matching_files.append(file)
        return matching_files

    # Dateien sammeln
    all_matching_files = []

    # Suche in Verzeichnis 1
    matching_files_verzeichnis1 = find_matching_files(verzeichnis1, full_title)
    all_matching_files.extend(matching_files_verzeichnis1)

    # Suche im zusätzlichen Verzeichnis
    if additional_media_dir:
        matching_files_additional = find_matching_files(additional_media_dir, full_title)
        all_matching_files.extend(matching_files_additional)

    # Prüfen, ob Dateien gefunden wurden
    if not all_matching_files:
        typer.secho(
            "Es wurden keine Dateien gefunden, die mit dem Titel beginnen.",
            fg=typer.colors.RED,
        )
        raise typer.Exit(code=1)

    # Initialisierung
    media_server_file = None
    internet_files = {
        "sd": None,
        "hd": None,
        "4k": None,
    }
    titelbild = None

    # Unterstützte Video- und Bildformate
    VIDEO_EXTENSIONS = [".mov", ".mp4", ".m4v"]
    IMAGE_EXTENSIONS = [".png", ".jpg", ".jpeg"]

    # Klassifizierung der Dateien
    for file in all_matching_files:
        file_ext = file.suffix.lower()

        if file_ext in VIDEO_EXTENSIONS:
            # Es ist eine Videodatei
            try:
                file_metadata = extract_metadata(file)
                image_height = int(file_metadata.get("ImageHeight", 0))
                avg_bitrate_str = file_metadata.get("AvgBitrate", "")
                # Extrahiere die numerische Bitrate in Mbps
                avg_bitrate_match = re.search(r"([\d\.]+)\s*Mbps", avg_bitrate_str)
                if avg_bitrate_match:
                    avg_bitrate = float(avg_bitrate_match.group(1))
                else:
                    avg_bitrate = 0.0
            except Exception as e:
                typer.secho(
                    f"Fehler beim Extrahieren der Metadaten von '{file}': {e}",
                    fg=typer.colors.RED,
                )
                continue

            # Klassifizierung
            if avg_bitrate > 50:
                # Medienserver-Datei
                if not media_server_file:
                    media_server_file = file
            else:
                # Internet-Dateien
                if image_height <= 540 and not internet_files["sd"]:
                    internet_files["sd"] = file
                elif image_height == 1080 and not internet_files["hd"]:
                    internet_files["hd"] = file
                elif image_height >= 2048 and not internet_files["4k"]:
                    internet_files["4k"] = file
                else:
                    # Datei passt in keine Kategorie
                    pass  # Optional: Warnung ausgeben
        elif file_ext in IMAGE_EXTENSIONS:
            # Es ist eine Bilddatei
            if not titelbild:
                if file_ext == ".png":
                    titelbild = file
                elif file_ext in [".jpg", ".jpeg"] and (not titelbild or titelbild.suffix.lower() != ".png"):
                    titelbild = file
        else:
            # Unbekannter Dateityp
            pass

    # Generiere den Verzeichnisnamen
    sanitized_title = sanitize_filename(titel)
    directory_name = f"{jahr}_{sanitized_title}"
    directory_path = verzeichnis1 / directory_name

    # Erstelle das Medienset-Verzeichnis
    if not directory_path.exists():
        try:
            directory_path.mkdir(parents=True, exist_ok=True)
            typer.secho(
                f"Verzeichnis '{directory_name}' wurde erstellt.", fg=typer.colors.GREEN
            )
        except Exception as e:
            typer.secho(
                f"Fehler beim Erstellen des Verzeichnisses: {e}", fg=typer.colors.RED
            )
            raise typer.Exit(code=1)
    else:
        typer.secho(
            f"Verzeichnis '{directory_name}' existiert bereits.",
            fg=typer.colors.YELLOW,
        )

    # Dateien zum Verschieben vorbereiten
    EXPECTED_FILENAMES = {
        "media_server": "Video-Medienserver.mov",
        "internet_4k": "Video-Internet-4K.m4v",
        "internet_hd": "Video-Internet-HD.m4v",
        "internet_sd": "Video-Internet-SD.m4v",
    }

    files_to_move = []

    if media_server_file:
        files_to_move.append(
            (media_server_file, directory_path / EXPECTED_FILENAMES["media_server"])
        )

    if internet_files["4k"]:
        files_to_move.append(
            (internet_files["4k"], directory_path / EXPECTED_FILENAMES["internet_4k"])
        )

    if internet_files["hd"]:
        files_to_move.append(
            (internet_files["hd"], directory_path / EXPECTED_FILENAMES["internet_hd"])
        )

    if internet_files["sd"]:
        files_to_move.append(
            (internet_files["sd"], directory_path / EXPECTED_FILENAMES["internet_sd"])
        )

    if titelbild:
        expected_titelbild_name = (
            "Titelbild.png"
            if titelbild.suffix.lower() == ".png"
            else "Titelbild.jpg"
        )
        files_to_move.append((titelbild, directory_path / expected_titelbild_name))

    # Bereite die Werte für 'videoschnitt' und 'kamerafuehrung' vor
    if videoschnitt:
        videoschnitt_list = split_list_field(videoschnitt)
    else:
        # Versuche, 'Producer' aus den Metadaten zu verwenden
        producer = metadata.get('Producer')
        if producer:
            videoschnitt_list = split_list_field(producer)
        else:
            videoschnitt_list = None

    if kamerafuehrung:
        kamerafuehrung_list = split_list_field(kamerafuehrung)
    else:
        # Versuche, 'Director' aus den Metadaten zu verwenden
        director = metadata.get('Director')
        if director:
            kamerafuehrung_list = split_list_field(director)
        else:
            kamerafuehrung_list = None

    # Erstelle die Metadaten.yaml vor dem Verschieben der Dateien
    try:
        # Generiere eine neue ULID
        medienset_id = generate_ulid()

        # Überprüfe erforderliche Felder
        fehlende_felder = []
        if not titel:
            fehlende_felder.append("titel")
        if not jahr:
            fehlende_felder.append("jahr")
        if untertyp == "Ereignis" and not aufnahmedatum:
            fehlende_felder.append("aufnahmedatum")
        if untertyp == "Rückblick" and not zeitraum:
            fehlende_felder.append("zeitraum")
        if fehlende_felder:
            typer.secho(f"Die folgenden erforderlichen Felder fehlen: {', '.join(fehlende_felder)}", fg=typer.colors.RED)
            raise typer.Exit(code=1)

        # Sammle die Metadaten
        metadaten = {
            "$schema": SCHEMA_URL,  # Schema-URL als Modulvariable
            "Spezifikationsversion": "1.0",
            "Id": medienset_id,
            "Titel": titel,
            "Typ": typ,  # Immer 'Familienfilm'
            "Untertyp": untertyp,
            "Jahr": str(jahr),
            "Version": 1,
            "Mediatheksdatum": datetime.now().strftime("%Y-%m-%d"),  # Hinzugefügt
        }

        if untertyp == "Ereignis":
            metadaten["Aufnahmedatum"] = aufnahmedatum
        elif untertyp == "Rückblick":
            metadaten["Zeitraum"] = zeitraum

        # Beschreibung aus Parameter oder Metadaten
        if beschreibung:
            metadaten["Beschreibung"] = beschreibung
        else:
            description = metadata.get('Description')
            if description:
                metadaten["Beschreibung"] = description

        # Notiz aus Parameter (keine Metadatenquelle)
        if notiz:
            metadaten["Notiz"] = notiz

        # Schlüsselwörter aus Parameter oder Metadaten
        if schluesselwoerter:
            metadaten["Schlüsselwörter"] = split_list_field(schluesselwoerter)
        else:
            genre = metadata.get('Genre')
            if genre:
                metadaten["Schlüsselwörter"] = split_list_field(genre)

        # Album aus Parameter oder Metadaten
        if album:
            metadaten["Album"] = album
        else:
            album_meta = metadata.get('Album')
            if album_meta:
                metadaten["Album"] = album_meta

        # Videoschnitt
        if videoschnitt_list:
            metadaten["Videoschnitt"] = videoschnitt_list

        # Kameraführung
        if kamerafuehrung_list:
            metadaten["Kameraführung"] = kamerafuehrung_list

        # Dauer in Sekunden
        if dauer_in_sekunden:
            metadaten["Dauer_in_Sekunden"] = dauer_in_sekunden
        else:
            # Versuche, die Dauer aus den Metadaten zu erhalten
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
                    metadaten["Dauer_in_Sekunden"] = dauer_in_sekunden
                except ValueError:
                    pass

        # Studio aus Parameter oder Metadaten
        if studio:
            metadaten["Studio"] = studio
        else:
            studio_meta = metadata.get('Studio')
            if studio_meta:
                metadaten["Studio"] = studio_meta

        # Filmfassung Name und Beschreibung
        if filmfassung_name:
            metadaten["Filmfassung_Name"] = filmfassung_name
        if filmfassung_beschreibung:
            metadaten["Filmfassung_Beschreibung"] = filmfassung_beschreibung

        # Erstelle das Ausgabepfad für die Metadaten.yaml
        metadata_path = directory_path / "Metadaten.yaml"

        # Schreibe die Metadaten.yaml-Datei
        with open(metadata_path, 'w', encoding='utf-8') as yaml_file:
            yaml.dump(metadaten, yaml_file, allow_unicode=True, sort_keys=False, default_flow_style=False)
        typer.secho(
            f"Metadaten.yaml wurde erstellt unter '{metadata_path.relative_to(directory_path.parent)}'.",
            fg=typer.colors.GREEN,
        )
    except Exception as e:
        typer.secho(
            f"Fehler beim Erstellen der Metadaten.yaml: {e}", fg=typer.colors.RED
        )
        raise typer.Exit(code=1)

    # Informiere den Benutzer über die Dateien, die verschoben werden
    typer.secho(
        f"Die folgenden Dateien werden in das Verzeichnis '{directory_name}' verschoben und umbenannt:",
        fg=typer.colors.YELLOW,
    )
    for src, dest in files_to_move:
        src_rel = src.name  # Nur der Dateiname
        dest_rel = dest.relative_to(directory_path.parent)
        typer.echo(f" - '{src_rel}' -> '{dest_rel}'")

    if not no_prompt:
        proceed = typer.confirm("Möchten Sie fortfahren?")
        if not proceed:
            typer.secho("Abgebrochen.", fg=typer.colors.RED)
            raise typer.Exit()
    else:
        typer.secho("Verschiebe Dateien ohne weitere Nachfrage...", fg=typer.colors.YELLOW)

    # Dateien verschieben
    for src, dest in files_to_move:
        if dest.exists():
            overwrite = True
            if not no_prompt:
                overwrite = typer.confirm(
                    f"Die Datei '{dest.name}' existiert bereits. Möchten Sie sie überschreiben?"
                )
            if not overwrite:
                typer.secho(
                    f"Überspringe das Verschieben von '{src.name}'.", fg=typer.colors.YELLOW
                )
                continue
        try:
            shutil.move(str(src), dest)
            typer.secho(
                f"Datei '{src.name}' wurde nach '{dest.relative_to(directory_path.parent)}' verschoben und umbenannt.",
                fg=typer.colors.GREEN,
            )
        except Exception as e:
            typer.secho(
                f"Fehler beim Verschieben der Datei '{src.name}': {e}", fg=typer.colors.RED
            )

    typer.secho("\nMedienset wurde erfolgreich erstellt.", fg=typer.colors.GREEN)

@app.command("create-homemovie")
def create_homemovie_cli(
    metadata_source: Path = typer.Option(
        ...,
        "--metadata-source",
        "-ms",
        help="Pfad zur Videodatei zur Extraktion von Metadaten (Referenzdatei).",
    ),
    additional_media_dir: Optional[Path] = typer.Option(
        None,
        "--additional-media-dir",
        "-amd",
        help="Zusätzliches Verzeichnis zur Suche nach Mediendateien.",
    ),
    titel: Optional[str] = typer.Option(None, help="Titel des Mediensets."),
    jahr: Optional[int] = typer.Option(None, help="Jahr des Mediensets."),
    untertyp: Optional[str] = typer.Option(
        None,
        help="Untertyp des Mediensets (Ereignis/Rückblick).",
    ),
    aufnahmedatum: Optional[str] = typer.Option(
        None, help="Aufnahmedatum (YYYY-MM-DD) für Untertyp 'Ereignis'."
    ),
    zeitraum: Optional[str] = typer.Option(
        None, help="Zeitraum für Untertyp 'Rückblick'."
    ),
    beschreibung: Optional[str] = typer.Option(
        None, help="Beschreibung des Mediensets."
    ),
    notiz: Optional[str] = typer.Option(None, help="Interne Bemerkungen zum Medienset."),
    schluesselwoerter: Optional[str] = typer.Option(
        None, help="Schlüsselwörter zur Kategorisierung, getrennt durch Komma oder Semikolon."
    ),
    album: Optional[str] = typer.Option(
        None, help="Name des Albums oder der Sammlung."
    ),
    videoschnitt: Optional[str] = typer.Option(
        None, help="Personen für den Videoschnitt, getrennt durch Komma oder Semikolon."
    ),
    kamerafuehrung: Optional[str] = typer.Option(
        None, help="Personen für die Kameraführung, getrennt durch Komma oder Semikolon."
    ),
    dauer_in_sekunden: Optional[int] = typer.Option(
        None, help="Gesamtdauer des Films in Sekunden."
    ),
    studio: Optional[str] = typer.Option(
        None, help="Studio oder Ort der Produktion."
    ),
    filmfassung_name: Optional[str] = typer.Option(
        None, help="Name der Filmfassung."
    ),
    filmfassung_beschreibung: Optional[str] = typer.Option(
        None, help="Beschreibung der Filmfassung."
    ),
    no_prompt: bool = typer.Option(
        False,
        "--no-prompt",
        help="Unterdrückt die Nachfrage beim Verschieben der Dateien."
    ),
):
    """
    Kommandozeilenfunktion für die Erstellung eines Mediensets.
    Ruft die Implementierungsfunktion auf und übergibt die Parameter.
    """
    create_homemovie(
        metadata_source=metadata_source,
        additional_media_dir=additional_media_dir,
        titel=titel,
        jahr=jahr,
        untertyp=untertyp,
        aufnahmedatum=aufnahmedatum,
        zeitraum=zeitraum,
        beschreibung=beschreibung,
        notiz=notiz,
        schluesselwoerter=schluesselwoerter,
        album=album,
        videoschnitt=videoschnitt,
        kamerafuehrung=kamerafuehrung,
        dauer_in_sekunden=dauer_in_sekunden,
        studio=studio,
        filmfassung_name=filmfassung_name,
        filmfassung_beschreibung=filmfassung_beschreibung,
        no_prompt=no_prompt,
    )