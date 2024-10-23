# mediaset_manager/commands/create_homemovie.py

import typer
from pathlib import Path
from typing import Optional
from mediaset_manager.utils import sanitize_filename
from mediaset_manager.commands.create_homemovie_metadata_file import create_homemovie_metadata_file
import shutil

app = typer.Typer()

@app.command("create-homemovie")
def create_homemovie(
    jahr: int = typer.Option(..., prompt=True, help="Jahr des Mediensets"),
    titel: str = typer.Option(..., prompt=True, help="Titel des Mediensets"),
    media_server_file: Optional[Path] = typer.Option(None, "--media-server-file", "-m", help="Pfad zur Medienserver-Videodatei"),
    internet_4k_file: Optional[Path] = typer.Option(None, "--internet-4k-file", "-4k", help="Pfad zur 4K-Internet-Videodatei"),
    internet_hd_file: Optional[Path] = typer.Option(None, "--internet-hd-file", "-hd", help="Pfad zur HD-Internet-Videodatei"),
    internet_sd_file: Optional[Path] = typer.Option(None, "--internet-sd-file", "-sd", help="Pfad zur SD-Internet-Videodatei"),
    projektdatei: Optional[Path] = typer.Option(None, "--projekt-datei", "-p", help="Pfad zur Projektdatei"),
    titelbild: Optional[Path] = typer.Option(None, "--titelbild", "-t", help="Pfad zum Titelbild (Titelbild.png)"),
    untertyp: str = typer.Option(..., help="Untertyp des Mediensets (Ereignis/Rückblick)",
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
    filmfassung_beschreibung: Optional[str] = typer.Option(None, help="Beschreibung der Filmfassung")
):
    """
    Erstellt ein Medienset-Verzeichnis und eine Metadaten.yaml-Datei.
    Dateien werden verschoben und umbenannt. Es erfolgt eine Bestätigung vor dem Verschieben.
    Bei bestehenden Dateien wird nachgefragt, ob diese überschrieben werden sollen.
    """
    # Überprüfe, ob mindestens eine Videodatei angegeben ist
    video_files = {
        'media_server': media_server_file,
        'internet_4k': internet_4k_file,
        'internet_hd': internet_hd_file,
        'internet_sd': internet_sd_file
    }
    provided_videos = [f for f in video_files.values() if f is not None]

    if not provided_videos:
        typer.secho("Es muss mindestens eine Videodatei angegeben werden (Medienserver oder eine Internet-Videodatei).", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Generiere den Verzeichnisnamen gemäß Spezifikation
    sanitized_title = sanitize_filename(titel)
    directory_name = f"{jahr}_{sanitized_title}"
    directory_path = Path(directory_name)

    # Informiere den Benutzer über das Verschieben und Umbenennen der Dateien
    typer.secho(f"Alle angegebenen Dateien werden in das Verzeichnis '{directory_name}' verschoben und umbenannt.", fg=typer.colors.YELLOW)
    proceed = typer.confirm("Möchtest du fortfahren?")
    if not proceed:
        typer.secho("Abgebrochen.", fg=typer.colors.RED)
        raise typer.Exit()

    # Erstelle das Verzeichnis, falls es nicht existiert
    if not directory_path.exists():
        try:
            directory_path.mkdir(parents=True, exist_ok=True)
            typer.secho(f"Verzeichnis '{directory_name}' wurde erstellt.", fg=typer.colors.GREEN)
        except Exception as e:
            typer.secho(f"Fehler beim Erstellen des Verzeichnisses: {e}", fg=typer.colors.RED)
            raise typer.Exit(code=1)
    else:
        typer.secho(f"Verzeichnis '{directory_name}' existiert bereits.", fg=typer.colors.YELLOW)

    # Erwartete Dateinamen für Videodateien
    EXPECTED_VIDEO_FILES = {
        'media_server': 'Video-Medienserver.mov',
        'internet_4k': 'Video-Internet-4K.m4v',
        'internet_hd': 'Video-Internet-HD.m4v',
        'internet_sd': 'Video-Internet-SD.m4v'
    }

    # Umbenennen und Verschieben der Videodateien
    for key, file_path in video_files.items():
        if file_path:
            if not file_path.is_file():
                typer.secho(f"Videodatei '{file_path}' existiert nicht.", fg=typer.colors.RED)
                continue
            destination = directory_path / EXPECTED_VIDEO_FILES[key]
            if destination.exists():
                overwrite = typer.confirm(f"Die Datei '{destination}' existiert bereits. Möchtest du sie überschreiben?")
                if not overwrite:
                    typer.secho(f"Überspringe das Verschieben von '{file_path}'.", fg=typer.colors.YELLOW)
                    continue
            try:
                shutil.move(str(file_path), destination)
                typer.secho(f"Datei '{file_path}' wurde nach '{destination}' verschoben und umbenannt.", fg=typer.colors.GREEN)
            except Exception as e:
                typer.secho(f"Fehler beim Verschieben der Datei '{file_path}': {e}", fg=typer.colors.RED)

    # Verschiebe die Projektdatei, falls angegeben
    if projektdatei:
        if not projektdatei.is_file():
            typer.secho(f"Projektdatei '{projektdatei}' existiert nicht.", fg=typer.colors.RED)
        else:
            destination = directory_path / projektdatei.name
            if destination.exists():
                overwrite = typer.confirm(f"Die Datei '{destination}' existiert bereits. Möchtest du sie überschreiben?")
                if not overwrite:
                    typer.secho(f"Überspringe das Verschieben von '{projektdatei}'.", fg=typer.colors.YELLOW)
                else:
                    try:
                        shutil.move(str(projektdatei), destination)
                        typer.secho(f"Projektdatei '{projektdatei}' wurde nach '{destination}' verschoben.", fg=typer.colors.GREEN)
                    except Exception as e:
                        typer.secho(f"Fehler beim Verschieben der Projektdatei '{projektdatei}': {e}", fg=typer.colors.RED)
            else:
                try:
                    shutil.move(str(projektdatei), destination)
                    typer.secho(f"Projektdatei '{projektdatei}' wurde nach '{destination}' verschoben.", fg=typer.colors.GREEN)
                except Exception as e:
                    typer.secho(f"Fehler beim Verschieben der Projektdatei '{projektdatei}': {e}", fg=typer.colors.RED)

    # Verschiebe das Titelbild, falls angegeben, und benenne es zu 'Titelbild.png' um
    if titelbild:
        if not titelbild.is_file():
            typer.secho(f"Titelbild-Datei '{titelbild}' existiert nicht.", fg=typer.colors.RED)
        else:
            destination = directory_path / "Titelbild.png"
            if destination.exists():
                overwrite = typer.confirm(f"Die Datei '{destination}' existiert bereits. Möchtest du sie überschreiben?")
                if not overwrite:
                    typer.secho(f"Überspringe das Verschieben von '{titelbild}'.", fg=typer.colors.YELLOW)
                else:
                    try:
                        shutil.move(str(titelbild), destination)
                        typer.secho(f"Titelbild '{titelbild}' wurde nach '{destination}' verschoben und umbenannt.", fg=typer.colors.GREEN)
                    except Exception as e:
                        typer.secho(f"Fehler beim Verschieben des Titelbildes '{titelbild}': {e}", fg=typer.colors.RED)
            else:
                try:
                    shutil.move(str(titelbild), destination)
                    typer.secho(f"Titelbild '{titelbild}' wurde nach '{destination}' verschoben und umbenannt.", fg=typer.colors.GREEN)
                except Exception as e:
                    typer.secho(f"Fehler beim Verschieben des Titelbildes '{titelbild}': {e}", fg=typer.colors.RED)

    # Erstelle die Metadaten.yaml-Datei
    try:
        create_homemovie_metadata_file(
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
            output=directory_path / "Metadaten.yaml"
        )
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der Metadaten.yaml: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    typer.secho("\nMedienset wurde erfolgreich erstellt.", fg=typer.colors.GREEN)