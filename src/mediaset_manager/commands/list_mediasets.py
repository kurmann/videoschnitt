# src/mediaset_manager/commands/list_mediasets.py

import os
import re
import json
from typing import Dict, Optional
import typer
import logging
from mediaset_manager.models import Medienset
from metadata_manager.commands.get_resolution import get_resolution_category
from metadata_manager.commands.get_title import get_title
from metadata_manager.commands.get_video_codec import get_video_codec

logger = logging.getLogger(__name__)

def list_mediasets_command(
    directory: str = typer.Argument(..., help="Hauptverzeichnis mit den Mediendateien"),
    additional_dir: Optional[str] = typer.Option(None, "--additional-dir", "-a", help="Optionales zusätzliches Verzeichnis mit Mediendateien"),
    json_output: bool = typer.Option(False, "--json", help="Gibt die Ausgabe als JSON zurück")
):
    """
    Listet alle Mediensets im angegebenen Verzeichnis auf und klassifiziert die Dateien.
    Optional durchsucht ein zusätzliches Verzeichnis nach Mediendateien.
    """
    try:
        mediensets = get_mediasets(directory, additional_dir)
        
        if not mediensets:
            typer.secho("Keine abgeschlossenen Mediensets gefunden.", fg=typer.colors.YELLOW)
            raise typer.Exit()
        
        if json_output:
            # Rückgabe als JSON
            output = {name: medienset_to_dict(set) for name, set in mediensets.items()}
            typer.echo(json.dumps(output, indent=4, ensure_ascii=False))
        else:
            # Ansprechende Konsolenausgabe mit besserer Ausrichtung
            for name, set in mediensets.items():
                typer.secho(f"Medienset: {name}", fg=typer.colors.BLUE, bold=True)
                typer.echo(f"  {'Masterdatei:':<21} {set.masterdatei or 'Nicht vorhanden'}")
                typer.echo(f"  {'Medienserver-Datei:':<21} {set.medienserver_datei or 'Nicht vorhanden'}")
                typer.echo(f"  {'Internet-Datei 4K:':<21} {set.internet_datei_4k or 'Nicht vorhanden'}")
                typer.echo(f"  {'Internet-Datei HD:':<21} {set.internet_datei_hd or 'Nicht vorhanden'}")
                typer.echo(f"  {'Titelbild:':<21} {set.titelbild or 'Nicht vorhanden'}")
                typer.echo("")
        
        # Rückgabe für die Nutzung in anderen Modulen
        return {name: set for name, set in mediensets.items()}
    
    except ValueError as ve:
        typer.secho(str(ve), fg=typer.colors.RED)
        raise typer.Exit(code=1)
    except typer.Exit:
        # Erlaube typer.Exit ohne weitere Aktion
        raise
    except Exception as e:
        logger.error(f"Unerwarteter Fehler: {e}")
        typer.secho("Ein unerwarteter Fehler ist aufgetreten.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

def medienset_to_dict(medienset: Medienset) -> Dict:
    return {
        "name": medienset.name,
        "masterdatei": medienset.masterdatei,
        "medienserver_datei": medienset.medienserver_datei,
        "internet_datei_4k": medienset.internet_datei_4k,
        "internet_datei_hd": medienset.internet_datei_hd,
        "titelbild": medienset.titelbild,
    }

def get_mediasets(directory: str, additional_dir: Optional[str] = None) -> Dict[str, Medienset]:
    """
    Scannt das angegebene Hauptverzeichnis und ein optionales zusätzliches Verzeichnis,
    gruppiert die Mediendateien in Mediensets und klassifiziert sie.

    Args:
        directory (str): Der Pfad zum Hauptverzeichnis mit den Mediendateien.
        additional_dir (Optional[str]): Ein optionaler Pfad zu einem zusätzlichen Verzeichnis mit Mediendateien.

    Returns:
        Dict[str, Medienset]: Ein Dictionary mit Medienset-Namen als Schlüsseln und Medienset-Objekten als Werten.
    """
    try:
        main_files = os.listdir(directory)
        logger.debug(f"Hauptverzeichnis '{directory}' enthält: {main_files}")
    except Exception as e:
        logger.error(f"Fehler beim Scannen des Hauptverzeichnisses '{directory}': {e}")
        typer.secho(f"Fehler beim Scannen des Hauptverzeichnisses '{directory}': {e}", fg=typer.colors.RED)
        raise typer.Exit(code=1)
    
    additional_files = []
    if additional_dir:
        try:
            additional_files = os.listdir(additional_dir)
            logger.debug(f"Zusätzliches Verzeichnis '{additional_dir}' enthält: {additional_files}")
        except Exception as e:
            logger.error(f"Fehler beim Scannen des zusätzlichen Verzeichnisses '{additional_dir}': {e}")
            typer.secho(f"Fehler beim Scannen des zusätzlichen Verzeichnisses '{additional_dir}': {e}", fg=typer.colors.RED)
            raise typer.Exit(code=1)
    
    # Kombiniere Dateien aus beiden Verzeichnissen
    all_files = main_files + additional_files
    logger.debug(f"Alle Dateien kombiniert: {all_files}")
    
    # Finde alle PNG-Dateien ohne zusätzliche Suffixe
    png_files = [f for f in all_files if f.lower().endswith('.png') and not re.search(r'\.\w+\.\w+$', f)]
    logger.debug(f"Gefundene PNG-Dateien: {png_files}")
    
    mediensets: Dict[str, Medienset] = {}
    
    for png in png_files:
        medienset_name = os.path.splitext(png)[0]
        logger.debug(f"Verarbeite Medienset: '{medienset_name}'")
        medienset = Medienset(name=medienset_name, titelbild=png)
        
        # Suche nach zugehörigen Dateien in beiden Verzeichnissen
        # Anpassung des Musters, um Dateien zu finden, die mit Medienset-Namen gefolgt von '-' oder ' ' beginnen
        pattern = re.escape(medienset_name) + r'([\- ].+|\..+)$'
        related_files = [f for f in all_files if re.match(pattern, f) and f != png]
        logger.debug(f"Zugehörige Dateien für Medienset '{medienset_name}': {related_files}")
        
        # Prüfe auf .sb Dateien
        if any(f.endswith('.sb') for f in related_files):
            logger.info(f"Medienset '{medienset_name}' enthält .sb-Dateien und wird ignoriert.")
            continue  # Ignoriere unvollständige Mediensets
        
        internet_datei_4k = None
        internet_datei_hd = None
        medienserver_files = []
        
        for file in related_files:
            # Bestimme das Verzeichnis der aktuellen Datei
            if file in main_files:
                file_dir = directory
            else:
                file_dir = additional_dir
            
            file_path = os.path.join(file_dir, file)
            logger.debug(f"Verarbeite Datei: '{file_path}'")
            
            if file.lower().endswith('.mov'):
                # Bestimme, ob es sich um Master- oder Medienserver-Datei handelt
                video_codec = get_video_codec(file_path)
                logger.debug(f"Videocodec für '{file_path}': {video_codec}")
                if video_codec and 'prores' in video_codec.lower():
                    if medienset.masterdatei is not None:
                        logger.error(f"Mehr als eine Masterdatei in Medienset '{medienset_name}' gefunden.")
                        typer.secho(f"Mehr als eine Masterdatei in Medienset '{medienset_name}' gefunden.", fg=typer.colors.RED)
                        raise ValueError(f"Mehr als eine Masterdatei in Medienset '{medienset_name}' gefunden.")
                    medienset.masterdatei = file
                    logger.debug(f"Masterdatei für '{medienset_name}' gesetzt auf: {file}")
                else:
                    medienserver_files.append(file)
                    logger.debug(f"Medienserver-Datei für '{medienset_name}' hinzugefügt: {file}")
            elif file.lower().endswith('.m4v'):
                resolution_info = get_resolution_category(file_path)
                if resolution_info:
                    _, category = resolution_info
                    logger.debug(f"Auflösungskategorie für '{file_path}': {category}")
                    if '4K' in category:
                        if internet_datei_4k is not None:
                            logger.error(f"Mehrere 4K-Internet-Dateien in Medienset '{medienset_name}' gefunden.")
                            typer.secho(f"Mehrere 4K-Internet-Dateien in Medienset '{medienset_name}' gefunden.", fg=typer.colors.RED)
                            raise ValueError(f"Mehrere 4K-Internet-Dateien in Medienset '{medienset_name}' gefunden.")
                        internet_datei_4k = file
                        logger.debug(f"Internet-Datei 4K für '{medienset_name}' gesetzt auf: {file}")
                    else:
                        if internet_datei_hd is not None:
                            logger.error(f"Mehrere HD-Internet-Dateien in Medienset '{medienset_name}' gefunden.")
                            typer.secho(f"Mehrere HD-Internet-Dateien in Medienset '{medienset_name}' gefunden.", fg=typer.colors.RED)
                            raise ValueError(f"Mehrere HD-Internet-Dateien in Medienset '{medienset_name}' gefunden.")
                        internet_datei_hd = file
                        logger.debug(f"Internet-Datei HD für '{medienset_name}' gesetzt auf: {file}")
            elif file.lower().endswith('.zip'):
                # Optional: Behandle ZIP-Dateien, falls gewünscht
                logger.debug(f"ZIP-Datei gefunden und ignoriert: {file}")
                pass
            # Weitere Dateitypen können hier hinzugefügt werden
        
        # Prüfe auf Medienserver-Dateien
        if len(medienserver_files) > 1:
            logger.error(f"Mehr als eine Medienserver-Datei in Medienset '{medienset_name}' gefunden.")
            typer.secho(f"Mehr als eine Medienserver-Datei in Medienset '{medienset_name}' gefunden.", fg=typer.colors.RED)
            raise ValueError(f"Mehr als eine Medienserver-Datei in Medienset '{medienset_name}' gefunden.")
        elif len(medienserver_files) == 1:
            medienset.medienserver_datei = medienserver_files[0]
            logger.debug(f"Medienserver-Datei für '{medienset_name}' gesetzt auf: {medienserver_files[0]}")
        
        medienset.internet_datei_4k = internet_datei_4k
        medienset.internet_datei_hd = internet_datei_hd
        
        logger.debug(f"Finales Medienset '{medienset_name}': {medienset}")
        
        mediensets[medienset_name] = medienset
    
    return mediensets