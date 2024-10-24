# mediaset_manager/commands/integrate_mediaset.py

import typer
from pathlib import Path
from typing import Optional
import shutil
from datetime import datetime
import yaml

from mediaset_manager.utils import sanitize_filename
# Entfernen Sie den Import von validate_mediaset, da die Validierung nicht mehr durchgeführt wird
# from mediaset_manager.commands.validate_mediaset import validate_mediaset
import sys

app = typer.Typer(help="Befehl zur Integration eines Mediensets in die Mediathek.")

def read_metadata(metadata_path: Path) -> dict:
    """
    Liest die Metadaten.yaml und gibt sie als Dictionary zurück.
    """
    with open(metadata_path, 'r', encoding='utf-8') as f:
        return yaml.safe_load(f)

def write_metadata(metadata_path: Path, metadata: dict):
    """
    Schreibt das Metadaten-Dictionary in die Metadaten.yaml.
    """
    with open(metadata_path, 'w', encoding='utf-8') as f:
        yaml.dump(metadata, f, allow_unicode=True, sort_keys=False, default_flow_style=False)

def get_existing_ulid(ziel_medisenset_dir: Path) -> Optional[str]:
    """
    Liest die ULID aus der bestehenden Metadaten.yaml des Mediensets in der Mediathek.
    """
    bestehende_metadata_path = ziel_medisenset_dir / "Metadaten.yaml"
    if not bestehende_metadata_path.is_file():
        return None
    try:
        bestehende_metadata = read_metadata(bestehende_metadata_path)
        return bestehende_metadata.get("Id")
    except Exception:
        return None

def set_ulid_in_metadata(metadata: dict, ulid: str):
    """
    Setzt die ULID in den Metadaten.
    """
    metadata["Id"] = ulid

def set_mediatheksdatum(metadata: dict, date_str: str):
    """
    Setzt das Mediatheksdatum in den Metadaten.
    """
    metadata["Mediatheksdatum"] = date_str

def determine_versionierung(ziel_jahr_dir: Path, aktuelle_date: datetime, option: Optional[str]) -> str:
    """
    Bestimmt, ob eine Überschreibung oder eine neue Version erstellt werden soll.
    """
    vorherige_versionen_dir = ziel_jahr_dir / "Vorherige_Versionen"
    vorherige_versionen_dir.mkdir(parents=True, exist_ok=True)  # Sicherstellen, dass das Verzeichnis existiert

    bestehende_versions = [d for d in vorherige_versionen_dir.iterdir() if d.is_dir() and d.name.startswith("Version_")]

    tage_seit_integration = 0

    if bestehende_versions:
        # Bestimme das Datum der letzten Integration
        letzte_version_dir = max(bestehende_versions, key=lambda d: d.name)
        bestehende_metadata_path = letzte_version_dir / "Metadaten.yaml"
        if bestehende_metadata_path.is_file():
            try:
                bestehende_metadata = read_metadata(bestehende_metadata_path)
                mediatheksdatum_str = bestehende_metadata.get("Mediatheksdatum")
                if mediatheksdatum_str:
                    mediatheksdatum = datetime.strptime(mediatheksdatum_str, "%Y-%m-%d")
                    tage_seit_integration = (aktuelle_date - mediatheksdatum).days
            except Exception:
                tage_seit_integration = 0

    if option == "overwrite":
        return "overwrite"
    elif option == "new":
        return "new"
    else:
        if tage_seit_integration > 40:
            return "new"
        else:
            return "overwrite"

def create_new_version(ziel_jahr_dir: Path, ziel_medisenset_dir: Path, version_num: int):
    """
    Erstellt eine neue Version des bestehenden Mediensets.
    Verschiebt das bestehende Medienset in das Vorherige_Versionen-Verzeichnis.
    """
    vorherige_versionen_dir = ziel_jahr_dir / "Vorherige_Versionen"
    vorherige_versionen_dir.mkdir(parents=True, exist_ok=True)  # Sicherstellen, dass das Verzeichnis existiert

    neue_version_dir = vorherige_versionen_dir / f"Version_{version_num}"
    shutil.move(str(ziel_medisenset_dir), str(neue_version_dir))
    return neue_version_dir

def integrate_medienset_logic(
    medienset_dir: Path,
    mediathek_dir: Path,
    version_option: Optional[str] = None,
    no_prompt: bool = False,
    callback=None  # Optionaler Callback
) -> bool:
    """
    Kernlogik zur Integration eines Mediensets in die Mediathek.
    Gibt True bei Erfolg und False bei Fehler zurück.
    """
    typer.secho(f"Integriere Medienset '{medienset_dir}' in die Mediathek...", fg=typer.colors.BLUE)

    # Schritt 1: Lesen der Metadaten
    metadata_path = medienset_dir / "Metadaten.yaml"
    try:
        metadata = read_metadata(metadata_path)
    except Exception as e:
        typer.secho(f"Fehler beim Lesen der Metadaten.yaml: {e}", fg=typer.colors.RED)
        if callback:
            callback(False)
        return False

    jahr = metadata.get("Jahr")
    titel = metadata.get("Titel")

    if not jahr or not titel:
        typer.secho("Metadaten fehlen für 'Jahr' oder 'Titel'.", fg=typer.colors.RED)
        if callback:
            callback(False)
        return False

    # Schritt 2: Bestimmen des Zieljahres-Verzeichnisses
    ziel_jahr_dir = mediathek_dir / str(jahr)
    ziel_jahr_dir.mkdir(parents=True, exist_ok=True)

    # Bestimmen des Ziel-Medienset-Verzeichnisses
    sanitized_title = sanitize_filename(titel)
    ziel_medisenset_dir = ziel_jahr_dir / f"{jahr}_{sanitized_title}"

    # Schritt 3: Überprüfen, ob das Medienset bereits existiert
    if ziel_medisenset_dir.exists():
        typer.secho(f"Medienset '{ziel_medisenset_dir}' existiert bereits in der Mediathek.", fg=typer.colors.YELLOW)

        # Lesen der bestehenden Metadaten
        bestehende_metadata_path = ziel_medisenset_dir / "Metadaten.yaml"
        if not bestehende_metadata_path.is_file():
            typer.secho(f"Bestehendes Medienset '{ziel_medisenset_dir}' hat keine Metadaten.yaml. Kann nicht überschrieben werden.", fg=typer.colors.RED)
            if callback:
                callback(False)
            return False

        try:
            bestehende_metadata = read_metadata(bestehende_metadata_path)
            bestehende_ulid = bestehende_metadata.get("Id")
            bestehende_mediatheksdatum_str = bestehende_metadata.get("Mediatheksdatum")
            bestehende_version = bestehende_metadata.get("Version", 1)  # Standard auf 1, wenn nicht vorhanden
        except Exception as e:
            typer.secho(f"Fehler beim Lesen der bestehenden Metadaten.yaml: {e}", fg=typer.colors.RED)
            if callback:
                callback(False)
            return False

        if not bestehende_ulid or not bestehende_mediatheksdatum_str:
            typer.secho(f"Bestehendes Medienset '{ziel_medisenset_dir}' hat unvollständige Metadaten.", fg=typer.colors.RED)
            if callback:
                callback(False)
            return False

        try:
            bestehende_mediatheksdatum = datetime.strptime(bestehende_mediatheksdatum_str, "%Y-%m-%d")
        except ValueError:
            typer.secho(f"Mediatheksdatum '{bestehende_mediatheksdatum_str}' ist ungültig.", fg=typer.colors.RED)
            if callback:
                callback(False)
            return False

        aktuelle_date = datetime.now()
        tage_seit_integration = (aktuelle_date - bestehende_mediatheksdatum).days

        # Schritt 4: Bestimmen der Versionierungsstrategie
        versionierung = determine_versionierung(ziel_jahr_dir, aktuelle_date, version_option)
        typer.secho(f"Versionierungsentscheidung: {versionierung}", fg=typer.colors.BLUE)

        if versionierung == "overwrite":
            # Schritt 5a: Überschreiben des bestehenden Mediensets
            if not bestehende_ulid:
                typer.secho(f"Bestehendes Medienset '{ziel_medisenset_dir}' hat keine ULID. Kann nicht überschrieben werden.", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False

            # ULID übernehmen
            set_ulid_in_metadata(metadata, bestehende_ulid)

            # Mediatheksdatum aktualisieren
            mediatheksdatum_str = aktuelle_date.strftime("%Y-%m-%d")
            set_mediatheksdatum(metadata, mediatheksdatum_str)

            # Version des neuen Mediensets erhöhen
            neue_version_num = bestehende_version + 1
            metadata["Version"] = neue_version_num

            write_metadata(metadata_path, metadata)

            if not no_prompt:
                proceed = typer.confirm(f"Möchten Sie das bestehende Medienset '{ziel_medisenset_dir}' überschreiben?")
                if not proceed:
                    typer.secho("Abgebrochen.", fg=typer.colors.RED)
                    if callback:
                        callback(False)
                    return False
            else:
                typer.secho("Überschreibe Medienset ohne Nachfrage...", fg=typer.colors.YELLOW)

            # Dateien kopieren und überschreiben
            try:
                for item in medienset_dir.iterdir():
                    if item.name == "Metadaten.yaml":
                        # Metadaten.yaml separat behandeln
                        ziel_metadata_path = ziel_medisenset_dir / "Metadaten.yaml"
                        write_metadata(ziel_metadata_path, metadata)
                        continue
                    ziel_item = ziel_medisenset_dir / item.name
                    if item.is_file():
                        shutil.copy2(item, ziel_item)
                        typer.secho(f"Datei '{item.name}' wurde kopiert/überschrieben.", fg=typer.colors.GREEN)
                    elif item.is_dir():
                        if ziel_item.exists():
                            shutil.rmtree(ziel_item)
                        shutil.copytree(item, ziel_item)
                        typer.secho(f"Verzeichnis '{item.name}' wurde kopiert/überschrieben.", fg=typer.colors.GREEN)
            except Exception as e:
                typer.secho(f"Fehler beim Kopieren der Dateien: {e}", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False

            # Nach erfolgreichem Kopieren das Quellverzeichnis entfernen
            try:
                shutil.rmtree(medienset_dir)
                typer.secho(f"Quellverzeichnis '{medienset_dir}' wurde entfernt.", fg=typer.colors.GREEN)
            except Exception as e:
                typer.secho(f"Fehler beim Entfernen des Quellverzeichnisses: {e}", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False

        elif versionierung == "new":
            # Schritt 5b: Erstellen einer neuen Version

            # Schritt 5b.1: Archivieren des bestehenden Mediensets
            vorherige_versionen_dir = ziel_jahr_dir / "Vorherige_Versionen"
            vorherige_versionen_dir.mkdir(parents=True, exist_ok=True)

            # Sicherstellen, dass die bestehende Version korrekt benannt ist
            archive_version_num = bestehende_version
            neue_version_dir = vorherige_versionen_dir / f"Version_{archive_version_num}"

            # Überprüfen, ob die Archivversion bereits existiert
            if neue_version_dir.exists():
                typer.secho(f"Archivversion '{neue_version_dir}' existiert bereits. Wählen Sie eine andere Versionierungsstrategie.", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False

            try:
                shutil.move(str(ziel_medisenset_dir), str(neue_version_dir))
                typer.secho(f"Bestehendes Medienset wurde nach '{neue_version_dir}' verschoben.", fg=typer.colors.GREEN)
            except shutil.Error as e:
                typer.secho(f"Fehler beim Verschieben des bestehenden Mediensets: {e}", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False

            # Schritt 5b.2: Aktualisieren der archivierten Metadaten (Version hinzufügen, falls fehlt)
            try:
                archived_metadata_path = neue_version_dir / "Metadaten.yaml"
                archived_metadata = read_metadata(archived_metadata_path)
                if "Version" not in archived_metadata:
                    archived_metadata["Version"] = archive_version_num
                    write_metadata(archived_metadata_path, archived_metadata)
                    typer.secho(f"Version {archive_version_num} zur archivierten Metadaten.yaml hinzugefügt.", fg=typer.colors.GREEN)
            except Exception as e:
                typer.secho(f"Fehler beim Aktualisieren der archivierten Metadaten.yaml: {e}", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False

            # Schritt 5b.3: Aktualisieren der Metadaten des neuen Mediensets
            metadata["Version"] = archive_version_num + 1  # Neue Version
            mediatheksdatum_str = aktuelle_date.strftime("%Y-%m-%d")
            set_mediatheksdatum(metadata, mediatheksdatum_str)
            write_metadata(metadata_path, metadata)

            if not no_prompt:
                proceed = typer.confirm(f"Möchten Sie das neue Medienset '{ziel_medisenset_dir}' in die Mediathek integrieren?")
                if not proceed:
                    typer.secho("Abgebrochen.", fg=typer.colors.RED)
                    if callback:
                        callback(False)
                    return False
            else:
                typer.secho("Integriere neues Medienset ohne Nachfrage...", fg=typer.colors.YELLOW)

            # Schritt 5b.4: Verschieben des neuen Mediensets in die Mediathek
            try:
                shutil.move(str(medienset_dir), str(ziel_medisenset_dir))
                typer.secho(f"Medienset wurde nach '{ziel_medisenset_dir}' verschoben.", fg=typer.colors.GREEN)
            except shutil.Error as e:
                typer.secho(f"Fehler beim Verschieben des neuen Mediensets: {e}", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False

        else:
            typer.secho(f"Ungültige Versionierungsoption '{version_option}'. Verwenden Sie 'overwrite' oder 'new'.", fg=typer.colors.RED)
            if callback:
                callback(False)
            return False

    else:
        # Schritt 6: Integration eines neuen Mediensets ohne bestehende Einträge
        # Mediatheksdatum setzen
        mediatheksdatum_str = datetime.now().strftime("%Y-%m-%d")
        set_mediatheksdatum(metadata, mediatheksdatum_str)
        write_metadata(metadata_path, metadata)

        if not no_prompt:
            proceed = typer.confirm(f"Möchten Sie das Medienset '{ziel_medisenset_dir}' in die Mediathek integrieren?")
            if not proceed:
                typer.secho("Abgebrochen.", fg=typer.colors.RED)
                if callback:
                    callback(False)
                return False
        else:
            typer.secho("Integriere Medienset ohne Nachfrage...", fg=typer.colors.YELLOW)

        # Verschieben des Mediensets in die Mediathek
        try:
            shutil.move(str(medienset_dir), str(ziel_medisenset_dir))
            typer.secho(f"Medienset wurde nach '{ziel_medisenset_dir}' verschoben.", fg=typer.colors.GREEN)
        except shutil.Error as e:
            typer.secho(f"Fehler beim Verschieben des Mediensets: {e}", fg=typer.colors.RED)
            if callback:
                callback(False)
            return False

    # Erfolgreiche Integration
    typer.secho("Medienset-Integration erfolgreich abgeschlossen.", fg=typer.colors.GREEN)
    if callback:
        callback(True)
    return True

@app.command("integrate-mediaset")
def integrate_mediaset_command(
    medienset_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        readable=True,
        help="Das Medienset-Verzeichnis, das in die Mediathek integriert werden soll."
    ),
    mediathek_dir: Path = typer.Argument(
        ...,
        exists=True,
        file_okay=False,
        dir_okay=True,
        writable=True,
        readable=True,
        help="Das Hauptverzeichnis der Mediathek, in das das Medienset integriert werden soll."
    ),
    version_option: Optional[str] = typer.Option(
        None,
        "--version",
        "-v",
        help="Versionierungsoption: 'overwrite' oder 'new'."
    ),
    no_prompt: bool = typer.Option(
        False,
        "--no-prompt",
        help="Unterdrückt die Nachfrage bei der Integration."
    )
):
    """
    Integriert ein einzelnes Medienset in die Mediathek. Verschiebt es in das passende Jahresverzeichnis.
    """
    success = integrate_medienset_logic(
        medienset_dir=medienset_dir,
        mediathek_dir=mediathek_dir,
        version_option=version_option,
        no_prompt=no_prompt
    )
    if success:
        raise typer.Exit(code=0)
    else:
        raise typer.Exit(code=1)

if __name__ == "__main__":
    app()