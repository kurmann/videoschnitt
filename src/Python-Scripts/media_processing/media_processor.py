import os
from file_utils import is_file_in_use, move_and_rename_to_target, delete_file
from video_utils import get_video_codec
from date_utils import get_creation_datetime
from compressor_utils import start_compressor

def are_sb_files_present(hevc_a_path):
    """
    Überprüft, ob temporäre .sb-Dateien vorhanden sind, die auf eine laufende Komprimierung hinweisen.
    Diese Prüfung erfolgt auf das Vorkommen von ".sb-" im Dateinamen.
    """
    directory = os.path.dirname(hevc_a_path)
    base_name = os.path.basename(hevc_a_path).replace('.mov', '')

    # Suche nach Dateien im gleichen Verzeichnis, die ".sb-" im Namen enthalten
    for filename in os.listdir(directory):
        if base_name in filename and ".sb-" in filename:
            print(f"Temporäre Datei gefunden: {filename}. Warte auf Abschluss der Komprimierung.")
            return True

    return False

def process_file(source_file, comp_dir, original_media_dir, compressor_started=False):
    """Verarbeitet eine Datei, verschiebt sie ins Komprimierungsverzeichnis oder integriert sie direkt."""
    codec = get_video_codec(source_file)
    print(f"Verarbeitung der Datei {source_file} mit Codec {codec}")

    if codec == 'prores':
        # Stelle sicher, dass Apple Compressor läuft
        if not compressor_started:
            start_compressor()
            compressor_started = True

        # Verschiebe ProRes-Datei ins Komprimierungsverzeichnis
        print(f"Verschiebe ProRes-Datei in das Komprimierungsverzeichnis {comp_dir}")
        move_and_rename_to_target(source_file, comp_dir, os.path.basename(source_file))
    else:
        # Integriere andere QuickTime-Dateien direkt in das Originalmedien-Verzeichnis
        creation_time = get_creation_datetime(source_file)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
        # Spezielle Behandlung für "ProRes"
        if codec.lower() == "prores":
            codec = "ProRes"
        else:
            codec = codec.upper()
        new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-{codec}.mov')
        destination_dir = os.path.join(original_media_dir, date_path)
        
        move_and_rename_to_target(source_file, destination_dir, new_filename)

def process_completed_hevca_and_delete_prores(comp_output_dir, comp_dir, original_media_dir):
    """
    Prüft, ob fertige HEVC-A-Dateien im Komprimierungs-Ausgabeverzeichnis vorhanden sind, löscht die passende ProRes-Datei
    und integriert die HEVC-A-Dateien in das Originalmedien-Verzeichnis.
    """
    for filename in os.listdir(comp_output_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov') or '-hevc-a' not in filename.lower():
            continue  # Versteckte Dateien oder nicht HEVC-A-Dateien überspringen

        hevc_a_path = os.path.join(comp_output_dir, filename)

        # Prüfen, ob temporäre .sb-Dateien vorhanden sind, die auf eine laufende Komprimierung hinweisen
        if are_sb_files_present(hevc_a_path):
            continue  # Überspringe, wenn .sb-Dateien gefunden wurden

        # Prüfen, ob die Datei vollständig ist und nicht leer
        if os.path.getsize(hevc_a_path) == 0:
            print(f"Überspringe leere Datei: {hevc_a_path}")
            continue

        # Ermittele den Basenamen der ProRes-Datei
        base_name = filename.replace('-HEVC-A', '').replace('-hevc-a', '').replace('.mov', '')
        prores_path = os.path.join(comp_dir, f"{base_name}.mov")

        if os.path.exists(prores_path):
            print(f"Entsprechende ProRes-Datei gefunden: {prores_path}. Lösche die Datei.")
            delete_file(prores_path)

        # Verschiebe die HEVC-A-Datei in das Originalmedien-Verzeichnis
        creation_time = get_creation_datetime(hevc_a_path)
        date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
        new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-HEVC-A.mov')
        destination_dir = os.path.join(original_media_dir, date_path)
        
        move_and_rename_to_target(hevc_a_path, destination_dir, new_filename)

def process_media_files(source_dir, original_media_dir):
    """Verschiebt QuickTime- und Bilddateien direkt ins Zielverzeichnis und berücksichtigt Unterverzeichnisse."""
    for root, _, files in os.walk(source_dir):
        for filename in files:
            if filename.startswith('.') or not (filename.lower().endswith(('.mov', '.mp4', '.jpg', '.jpeg', '.png', '.heif', '.heic', '.dng'))):
                continue  # Versteckte Dateien oder nicht unterstützte Formate überspringen

            file_path = os.path.join(root, filename)

            if is_file_in_use(file_path):
                print(f"Datei {filename} wird noch verwendet. Überspringe.")
                continue

            # Ermittele das Erstellungsdatum der Datei
            creation_time = get_creation_datetime(file_path)
            date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')

            # Bestimme das Dateiformat (Unterschied zwischen Video- und Bilddateien)
            if filename.lower().endswith(('.mov', '.mp4')):
                codec = get_video_codec(file_path)
                # Spezielle Behandlung für "ProRes"
                if codec.lower() == "prores":
                    codec = "ProRes"
                else:
                    codec = codec.upper()
                new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-{codec}.mov')
            else:
                # Für Bilddateien gibt es kein Codec-Suffix, nur das Datumsmuster
                extension = os.path.splitext(filename)[1].lower()  # Behalte die Original-Erweiterung bei
                new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S{extension}')

            # Bestimme das relative Verzeichnis gegenüber dem Quellverzeichnis
            relative_dir = os.path.relpath(root, source_dir)
            destination_dir = os.path.join(original_media_dir, date_path, relative_dir)

            # Verschiebe und benenne die Datei um
            move_and_rename_to_target(file_path, destination_dir, new_filename)

    # Entferne leere Verzeichnisse nach der Verarbeitung
    remove_empty_directories(source_dir)

def remove_empty_directories(root_dir):
    """Durchsucht rekursiv das Quellverzeichnis und entfernt alle leeren Unterverzeichnisse."""
    for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
        # Überspringe das Quellverzeichnis selbst
        if dirpath == root_dir:
            continue

        # Überprüfe, ob das Verzeichnis leer ist (keine nicht-versteckten Dateien)
        if not any(fname for fname in filenames if not fname.startswith('.')) and not dirnames:
            try:
                os.rmdir(dirpath)
                print(f"Leeres Verzeichnis entfernt: {dirpath}")
            except Exception as e:
                print(f"Fehler beim Entfernen des Verzeichnisses {dirpath}: {e}")