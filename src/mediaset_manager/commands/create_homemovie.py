# mediaset_manager/commands/create_homemovie.py

import typer
from pathlib import Path
from typing import Optional
from mediaset_manager.utils import sanitize_filename
from mediaset_manager.commands.create_homemovie_metadata_file import (
    create_homemovie_metadata_file,
    extract_metadata,
    parse_date,
    extract_date_and_title_from_name,
)
import shutil
from datetime import datetime

app = typer.Typer()

@app.command("create-homemovie")
def create_homemovie(
    jahr: Optional[int] = typer.Option(None, help="Jahr des Mediensets"),
    titel: Optional[str] = typer.Option(None, help="Titel des Mediensets"),
    media_server_file: Optional[Path] = typer.Option(
        None, "--media-server-file", "-m", help="Pfad zur Medienserver-Videodatei"
    ),
    internet_4k_file: Optional[Path] = typer.Option(
        None, "--internet-4k-file", "-4k", help="Pfad zur 4K-Internet-Videodatei"
    ),
    internet_hd_file: Optional[Path] = typer.Option(
        None, "--internet-hd-file", "-hd", help="Pfad zur HD-Internet-Videodatei"
    ),
    internet_sd_file: Optional[Path] = typer.Option(
        None, "--internet-sd-file", "-sd", help="Pfad zur SD-Internet-Videodatei"
    ),
    projektdatei: Optional[Path] = typer.Option(
        None, "--projekt-datei", "-p", help="Pfad zur Projektdatei"
    ),
    titelbild: Optional[Path] = typer.Option(
        None, "--titelbild", "-t", help="Pfad zum Titelbild (Titelbild.png)"
    ),
    metadata_source: Optional[Path] = typer.Option(
        None, help="Datei zur Extraktion von Metadaten"
    ),
    typ: Optional[str] = typer.Option(None, help="Typ des Mediensets"),
    untertyp: Optional[str] = typer.Option(
        None,
        help="Untertyp des Mediensets (Ereignis/Rückblick)",
        callback=lambda ctx, param, value: value.capitalize() if value else value,
    ),
    aufnahmedatum: Optional[str] = typer.Option(
        None, help="Aufnahmedatum (YYYY-MM-DD) für Untertyp 'Ereignis'"
    ),
    zeitraum: Optional[str] = typer.Option(
        None, help="Zeitraum für Untertyp 'Rückblick'"
    ),
    beschreibung: Optional[str] = typer.Option(
        None, help="Beschreibung des Mediensets"
    ),
    notiz: Optional[str] = typer.Option(None, help="Interne Bemerkungen zum Medienset"),
    schluesselwoerter: Optional[str] = typer.Option(
        None, help="Schlüsselwörter zur Kategorisierung, durch Komma getrennt"
    ),
    album: Optional[str] = typer.Option(
        None, help="Name des Albums oder der Sammlung"
    ),
    videoschnitt: Optional[str] = typer.Option(
        None, help="Personen für den Videoschnitt, durch Komma getrennt"
    ),
    kamerafuehrung: Optional[str] = typer.Option(
        None, help="Personen für die Kameraführung, durch Komma getrennt"
    ),
    dauer_in_sekunden: Optional[int] = typer.Option(
        None, help="Gesamtdauer des Films in Sekunden"
    ),
    studio: Optional[str] = typer.Option(
        None, help="Studio oder Ort der Produktion"
    ),
    filmfassung_name: Optional[str] = typer.Option(
        None, help="Name der Filmfassung"
    ),
    filmfassung_beschreibung: Optional[str] = typer.Option(
        None, help="Beschreibung der Filmfassung"
    ),
):
    """
    Erstellt ein Medienset-Verzeichnis und eine Metadaten.yaml-Datei.
    Dateien werden verschoben und umbenannt. Es erfolgt eine Bestätigung vor dem Verschieben.
    Bei bestehenden Dateien wird nachgefragt, ob diese überschrieben werden sollen.
    """
    # Überprüfe, ob mindestens eine Videodatei angegeben ist
    video_files = {
        "media_server": media_server_file,
        "internet_4k": internet_4k_file,
        "internet_hd": internet_hd_file,
        "internet_sd": internet_sd_file,
    }
    provided_videos = [f for f in video_files.values() if f is not None]

    if not provided_videos:
        typer.secho(
            "Es muss mindestens eine Videodatei angegeben werden (Medienserver oder eine Internet-Videodatei).",
            fg=typer.colors.RED,
        )
        raise typer.Exit(code=1)

    # Metadaten extrahieren vor dem Verschieben
    if metadata_source:
        try:
            metadata = extract_metadata(metadata_source)
            if not titel:
                name = (
                    metadata.get("Title")
                    or metadata.get("DisplayName")
                    or metadata.get("Name")
                )
                if name:
                    extracted_date, extracted_title = extract_date_and_title_from_name(
                        name
                    )
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
        except Exception as e:
            typer.secho(
                f"Fehler beim Extrahieren der Metadaten: {e}", fg=typer.colors.RED
            )
            raise typer.Exit(code=1)

    # Wenn titel oder jahr immer noch nicht vorhanden, Benutzer zur Eingabe auffordern
    if not titel:
        titel = typer.prompt("Bitte geben Sie den Titel des Mediensets ein")
    if not jahr:
        jahr = typer.prompt("Bitte geben Sie das Jahr des Mediensets ein", type=int)

    # Generiere den Verzeichnisnamen gemäß Spezifikation
    sanitized_title = sanitize_filename(titel)
    directory_name = f"{jahr}_{sanitized_title}"

    # Bestimme den Basispfad für das Medienset-Verzeichnis
    if metadata_source:
        base_path = metadata_source.parent
    elif media_server_file:
        base_path = media_server_file.parent
    else:
        # Verwende den Pfad der ersten verfügbaren Internet-Datei
        for file in [internet_4k_file, internet_hd_file, internet_sd_file]:
            if file:
                base_path = file.parent
                break
        else:
            base_path = Path.cwd()  # Aktuelles Verzeichnis

    directory_path = base_path / directory_name

    # Erstelle die Metadaten.yaml-Datei vor dem Verschieben der Dateien
    try:
        create_homemovie_metadata_file(
            titel=titel,
            jahr=jahr,
            typ=typ,
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
            metadata_source=metadata_source,
            output=directory_path / "Metadaten.yaml",
        )
    except Exception as e:
        typer.secho(
            f"Fehler beim Erstellen der Metadaten.yaml: {e}", fg=typer.colors.RED
        )
        raise typer.Exit(code=1)

    # Informiere den Benutzer über das Verschieben und Umbenennen der Dateien
    typer.secho(
        f"Alle angegebenen Dateien werden in das Verzeichnis '{directory_path}' verschoben und umbenannt.",
        fg=typer.colors.YELLOW,
    )
    proceed = typer.confirm("Möchtest du fortfahren?")
    if not proceed:
        typer.secho("Abgebrochen.", fg=typer.colors.RED)
        raise typer.Exit()

    # Erstelle das Verzeichnis, falls es nicht existiert
    if not directory_path.exists():
        try:
            directory_path.mkdir(parents=True, exist_ok=True)
            typer.secho(f"Verzeichnis '{directory_path}' wurde erstellt.", fg=typer.colors.GREEN)
        except Exception as e:
            typer.secho(f"Fehler beim Erstellen des Verzeichnisses: {e}", fg=typer.colors.RED)
            raise typer.Exit(code=1)
    else:
        typer.secho(f"Verzeichnis '{directory_path}' existiert bereits.", fg=typer.colors.YELLOW)

    # Erwartete Dateinamen für Videodateien
    EXPECTED_VIDEO_FILES = {
        "media_server": "Video-Medienserver.mov",
        "internet_4k": "Video-Internet-4K.m4v",
        "internet_hd": "Video-Internet-HD.m4v",
        "internet_sd": "Video-Internet-SD.m4v",
    }

    # Umbenennen und Verschieben der Videodateien
    for key, file_path in video_files.items():
        if file_path:
            if not file_path.is_file():
                typer.secho(f"Videodatei '{file_path}' existiert nicht.", fg=typer.colors.RED)
                continue
            destination = directory_path / EXPECTED_VIDEO_FILES[key]
            if destination.exists():
                overwrite = typer.confirm(
                    f"Die Datei '{destination}' existiert bereits. Möchtest du sie überschreiben?"
                )
                if not overwrite:
                    typer.secho(f"Überspringe das Verschieben von '{file_path}'.", fg=typer.colors.YELLOW)
                    continue
            try:
                shutil.move(str(file_path), destination)
                typer.secho(
                    f"Datei '{file_path}' wurde nach '{destination}' verschoben und umbenannt.",
                    fg=typer.colors.GREEN,
                )
            except Exception as e:
                typer.secho(f"Fehler beim Verschieben der Datei '{file_path}': {e}", fg=typer.colors.RED)

    # Verschiebe die Projektdatei, falls angegeben
    if projektdatei:
        if not projektdatei.is_file():
            typer.secho(f"Projektdatei '{projektdatei}' existiert nicht.", fg=typer.colors.RED)
        else:
            destination = directory_path / projektdatei.name
            if destination.exists():
                overwrite = typer.confirm(
                    f"Die Datei '{destination}' existiert bereits. Möchtest du sie überschreiben?"
                )
                if not overwrite:
                    typer.secho(f"Überspringe das Verschieben von '{projektdatei}'.", fg=typer.colors.YELLOW)
                else:
                    try:
                        shutil.move(str(projektdatei), destination)
                        typer.secho(
                            f"Projektdatei '{projektdatei}' wurde nach '{destination}' verschoben.",
                            fg=typer.colors.GREEN,
                        )
                    except Exception as e:
                        typer.secho(f"Fehler beim Verschieben der Projektdatei '{projektdatei}': {e}", fg=typer.colors.RED)
            else:
                try:
                    shutil.move(str(projektdatei), destination)
                    typer.secho(
                        f"Projektdatei '{projektdatei}' wurde nach '{destination}' verschoben.",
                        fg=typer.colors.GREEN,
                    )
                except Exception as e:
                    typer.secho(f"Fehler beim Verschieben der Projektdatei '{projektdatei}': {e}", fg=typer.colors.RED)

    # Verschiebe das Titelbild, falls angegeben, und benenne es zu 'Titelbild.png' um
    if titelbild:
        if not titelbild.is_file():
            typer.secho(f"Titelbild-Datei '{titelbild}' existiert nicht.", fg=typer.colors.RED)
        else:
            destination = directory_path / "Titelbild.png"
            if destination.exists():
                overwrite = typer.confirm(
                    f"Die Datei '{destination}' existiert bereits. Möchtest du sie überschreiben?"
                )
                if not overwrite:
                    typer.secho(f"Überspringe das Verschieben von '{titelbild}'.", fg=typer.colors.YELLOW)
                else:
                    try:
                        shutil.move(str(titelbild), destination)
                        typer.secho(
                            f"Titelbild '{titelbild}' wurde nach '{destination}' verschoben und umbenannt.",
                            fg=typer.colors.GREEN,
                        )
                    except Exception as e:
                        typer.secho(f"Fehler beim Verschieben des Titelbildes '{titelbild}': {e}", fg=typer.colors.RED)
            else:
                try:
                    shutil.move(str(titelbild), destination)
                    typer.secho(
                        f"Titelbild '{titelbild}' wurde nach '{destination}' verschoben und umbenannt.",
                        fg=typer.colors.GREEN,
                    )
                except Exception as e:
                    typer.secho(f"Fehler beim Verschieben des Titelbildes '{titelbild}': {e}", fg=typer.colors.RED)

    typer.secho("\nMedienset wurde erfolgreich erstellt.", fg=typer.colors.GREEN)