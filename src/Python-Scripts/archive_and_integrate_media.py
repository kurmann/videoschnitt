import os
import subprocess
import shutil
import sys
import time
from datetime import datetime

def is_file_in_use(filepath):
    """Überprüft, ob eine Datei in Verwendung ist."""
    try:
        os.rename(filepath, filepath)  # Versuch, die Datei umzubenennen, um sicherzustellen, dass sie nicht in Verwendung ist.
        return False
    except OSError:
        return True

def get_video_codec(filepath):
    """Ermittelt den Codec einer Videodatei."""
    cmd = ['ffprobe', '-v', 'error', '-select_streams', 'v:0', '-show_entries', 'stream=codec_name', '-of', 'default=noprint_wrappers=1:nokey=1', filepath]
    try:
        result = subprocess.check_output(cmd, text=True)
        return result.strip()
    except subprocess.CalledProcessError:
        return None

def get_creation_datetime(filepath):
    """Extrahiert das Aufnahmedatum aus den Metadaten der Videodatei."""
    cmd = ['ffprobe', '-v', 'error', '-show_entries', 'format_tags=creation_time', '-of', 'default=noprint_wrappers=1:nokey=1', filepath]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
    creation_time_str = result.stdout.strip()
    
    if creation_time_str:
        creation_time = datetime.fromisoformat(creation_time_str.replace('Z', '+00:00'))
        return creation_time
    else:
        print(f"Warnung: Konnte kein Aufnahmedatum für {filepath} extrahieren. Verwende das aktuelle Datum.")
        return datetime.now()

def is_compressor_running():
    """Überprüft, ob Apple Compressor läuft."""
    try:
        output = subprocess.check_output(['pgrep', '-x', 'Compressor'], text=True)
        return bool(output.strip())
    except subprocess.CalledProcessError:
        return False

def start_compressor():
    """Startet Apple Compressor, falls es nicht läuft."""
    if not is_compressor_running():
        print("Apple Compressor wird gestartet...")
        subprocess.Popen(['open', '-a', 'Compressor'])
        time.sleep(3)  # 3 Sekunden warten, damit Compressor vollständig startet

def move_file_with_postfix(source_file, destination_dir):
    """Verschiebt eine Datei ins Komprimierungsverzeichnis und fügt einen Postfix hinzu, wenn eine Datei mit demselben Namen existiert."""
    filename = os.path.basename(source_file)
    destination_path = os.path.join(destination_dir, filename)
    
    # Füge einen Postfix hinzu, falls eine Datei mit demselben Namen bereits existiert
    if os.path.exists(destination_path):
        name, ext = os.path.splitext(filename)
        counter = 1
        while os.path.exists(destination_path):
            destination_path = os.path.join(destination_dir, f"{name}_{counter}{ext}")
            counter += 1

    try:
        shutil.move(source_file, destination_path)
        print(f"Verschoben: {source_file} -> {destination_path}")
    except Exception as e:
        print(f"Fehler beim Verschieben der Datei: {e}")
        return None

    return destination_path

def move_and_rename_to_target(source_file, original_media_dir, codec_type):
    """Verschiebt und benennt eine Datei ins Zielverzeichnis um, mit einer Unterverzeichnisstruktur basierend auf dem Aufnahmedatum."""
    creation_time = get_creation_datetime(source_file)
    date_path = creation_time.strftime('%Y/%Y-%m/%Y-%m-%d')
    new_filename = creation_time.strftime(f'%Y-%m-%d_%H%M%S-{codec_type}.mov')
    destination_dir = os.path.join(original_media_dir, date_path, codec_type)

    # Erstelle das Zielverzeichnis, falls es nicht existiert
    os.makedirs(destination_dir, exist_ok=True)

    destination_path = os.path.join(destination_dir, new_filename)

    try:
        shutil.move(source_file, destination_path)
        print(f"Verschoben und umbenannt: {source_file} -> {destination_path}")

        # Existenz der Datei am Zielort prüfen
        if os.path.exists(destination_path):
            print(f"Bestätigt: Datei erfolgreich verschoben nach {destination_path}")
        else:
            print(f"Warnung: Datei wurde nicht korrekt verschoben nach {destination_path}")
            return None

    except Exception as e:
        print(f"Fehler beim Verschieben der Datei: {e}")
        return None

    return destination_path

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
        move_file_with_postfix(source_file, comp_dir)
    else:
        # Integriere andere QuickTime-Dateien direkt in das Originalmedien-Verzeichnis
        print(f"Integriere Datei direkt in das Verzeichnis {original_media_dir}")
        new_file_path = move_and_rename_to_target(source_file, original_media_dir, codec.upper())

        if new_file_path and codec == 'hevc':
            # Lösche die entsprechende ProRes-Datei, wenn HEVC-A integriert wurde
            creation_time = get_creation_datetime(new_file_path)
            find_and_delete_prores(original_media_dir, creation_time)

def find_and_delete_prores(original_dir, creation_time):
    """Findet und löscht die entsprechende ProRes-Datei basierend auf dem Aufnahmedatum."""
    base_filename = creation_time.strftime('%Y-%m-%d_%H%M%S')
    prores_filename = base_filename + ".mov"
    
    for root, _, files in os.walk(original_dir):
        if prores_filename in files:
            prores_path = os.path.join(root, prores_filename)
            print(f"ProRes-Datei gefunden: {prores_path}")
            os.remove(prores_path)
            print(f"ProRes-Datei gelöscht: {prores_path}")
            return True

    print("Keine passende ProRes-Datei gefunden.")
    return False

def process_completed_hevca_files(comp_dir, original_media_dir):
    """Überprüft das Komprimierungsverzeichnis auf fertige HEVC-A-Dateien und verschiebt sie."""
    for filename in os.listdir(comp_dir):
        if filename.startswith('.') or filename.endswith('.sb'):
            continue  # Ignoriere versteckte Dateien und Zwischenartefakte

        if filename.lower().endswith('-hevc-a.mov'):
            file_path = os.path.join(comp_dir, filename)

            if is_file_in_use(file_path):
                print(f"HEVC-A-Datei {filename} wird noch verwendet. Überspringe.")
                continue

            # Verschiebe die fertige HEVC-A-Datei ins Originalmedien-Verzeichnis
            print(f"Fertige HEVC-A-Datei gefunden: {filename}. Verschiebe sie ins Originalmedien-Verzeichnis.")
            move_and_rename_to_target(file_path, original_media_dir, 'HEVC-A')

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("Usage: python3 archive_and_integrate_media.py /path/to/source_directory /path/to/compression_directory /path/to/original_media_directory")
        sys.exit(1)

    source_dir = sys.argv[1]
    comp_dir = sys.argv[2]
    original_media_dir = sys.argv[3]

    if not os.path.isdir(source_dir):
        print(f"Error: {source_dir} is not a valid directory")
        sys.exit(1)

    if not os.path.isdir(comp_dir):
        print(f"Error: {comp_dir} is not a valid directory")
        sys.exit(1)

    if not os.path.isdir(original_media_dir):
        print(f"Error: {original_media_dir} is not a valid directory")
        sys.exit(1)

    compressor_started = False

    # Zuerst prüfen, ob fertige HEVC-A-Dateien im Komprimierungsverzeichnis vorhanden sind
    process_completed_hevca_files(comp_dir, original_media_dir)

    # Verarbeite die Dateien im Quellverzeichnis
    for filename in os.listdir(source_dir):
        if filename.startswith('.') or not filename.lower().endswith('.mov'):
            continue  # Versteckte Dateien oder Dateien, die nicht MOV sind, überspringen

        file_path = os.path.join(source_dir, filename)

        if is_file_in_use(file_path):
            print(f"Datei {filename} wird noch verwendet. Überspringe.")
            continue
        process_file(file_path, comp_dir, original_media_dir, compressor_started)