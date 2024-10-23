# mediaset_manager/commands/create_homemovie_metadata_file.py

import typer
import yaml
from datetime import datetime
from pathlib import Path
from typing import Optional
from mediaset_manager.utils import generate_ulid
import logging

app = typer.Typer()

@app.command("create-homemovie-metadata-file")
def create_homemovie_metadata_file(
    titel: str = typer.Option(..., help="Titel des Mediensets"),
    jahr: int = typer.Option(..., help="Jahr des Mediensets"),
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
    filmfassung_beschreibung: Optional[str] = typer.Option(None, help="Beschreibung der Filmfassung"),
    output: Optional[Path] = typer.Option(None, "--output", "-o", help="Ausgabepfad inklusive Dateiname (z.B., /path/to/Metadaten.yaml)")
):
    """
    Erstellt die Metadaten.yaml-Datei im angegebenen Verzeichnis oder im aktuellen Verzeichnis.
    """
    # Bestimme den Ausgabepfad
    if output:
        metadata_path = output
    else:
        metadata_path = Path("Metadaten.yaml")
    
    # Generiere eine neue ULID
    medienset_id = generate_ulid()
    
    metadaten = {
        "Spezifikationsversion": "1.0",
        "Id": medienset_id,
        "Titel": titel,
        "Typ": "Familienfilm",
        "Untertyp": untertyp,
        "Jahr": str(jahr),
        "Mediatheksdatum": datetime.now().strftime("%Y-%m-%d"),
        "Version": 1
    }
    
    if untertyp == "Ereignis":
        if not aufnahmedatum:
            typer.secho("Für Untertyp 'Ereignis' ist das Aufnahmedatum obligatorisch.", fg=typer.colors.RED)
            raise typer.Exit(code=1)
        metadaten["Aufnahmedatum"] = aufnahmedatum
    elif untertyp == "Rückblick":
        if not zeitraum:
            typer.secho("Für Untertyp 'Rückblick' ist das Zeitraum-Feld obligatorisch.", fg=typer.colors.RED)
            raise typer.Exit(code=1)
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
        metadaten["Videoschnitt"] = [vs.strip() for vs in videoschnitt.split(",")]
    if kamerafuehrung:
        metadaten["Kameraführung"] = [kf.strip() for kf in kamerafuehrung.split(",")]
    if dauer_in_sekunden:
        metadaten["Dauer_in_Sekunden"] = dauer_in_sekunden
    if studio:
        metadaten["Studio"] = studio
    if filmfassung_name:
        metadaten["Filmfassung_Name"] = filmfassung_name
    if filmfassung_beschreibung:
        metadaten["Filmfassung_Beschreibung"] = filmfassung_beschreibung
    
    # Schreibe die Metadaten.yaml-Datei
    try:
        with open(metadata_path, 'w', encoding='utf-8') as yaml_file:
            yaml.dump(metadaten, yaml_file, allow_unicode=True, sort_keys=False, default_flow_style=False)
        typer.secho(f"Metadaten.yaml wurde erstellt unter '{metadata_path}'.", fg=typer.colors.GREEN)
    except Exception as e:
        typer.secho(f"Fehler beim Erstellen der Metadaten.yaml: {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)