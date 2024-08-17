import os
import shutil

def is_file_in_use(filepath):
    """Überprüft, ob eine Datei in Verwendung ist."""
    try:
        os.rename(filepath, filepath)  # Versuch, die Datei umzubenennen, um sicherzustellen, dass sie nicht in Verwendung ist.
        return False
    except OSError:
        return True

def move_file_with_postfix(source_file, destination_dir):
    """Verschiebt eine Datei und fügt einen Postfix hinzu, wenn eine Datei mit demselben Namen existiert."""
    filename = os.path.basename(source_file)
    destination_path = os.path.join(destination_dir, filename)
    
    if os.path.exists(destination_path):
        name, ext = os.path.splitext(filename)
        counter = 1
        while os.path.exists(destination_path):
            destination_path = os.path.join(destination_dir, f"{name}_{counter}{ext}")
            counter += 1

    try:
        os.makedirs(destination_dir, exist_ok=True)  # Stellt sicher, dass das Zielverzeichnis existiert
        shutil.move(source_file, destination_path)
        print(f"Verschoben: {source_file} -> {destination_path}")
    except Exception as e:
        print(f"Fehler beim Verschieben der Datei: {e}")
        return None

    return destination_path

def move_and_rename_to_target(source_file, destination_dir, new_filename):
    """Verschiebt eine Datei in das Zielverzeichnis und benennt sie um."""
    destination_path = os.path.join(destination_dir, new_filename)
    file_size = os.path.getsize(source_file)

    try:
        os.makedirs(destination_dir, exist_ok=True)  # Stellt sicher, dass das Zielverzeichnis existiert
        print(f"Verschiebe die Datei {source_file} ({file_size / (1024 * 1024):.2f} MB) nach {destination_path}")
        shutil.move(source_file, destination_path)
        print(f"Verschoben und umbenannt: {source_file} -> {destination_path}")

        if os.path.exists(destination_path):
            print(f"Bestätigt: Datei erfolgreich verschoben nach {destination_path}")
        else:
            print(f"Warnung: Datei wurde nicht korrekt verschoben nach {destination_path}")
            return None

    except Exception as e:
        print(f"Fehler beim Verschieben der Datei: {e}")
        return None

    return destination_path

def delete_file(filepath):
    """Löscht die angegebene Datei."""
    try:
        os.remove(filepath)
        print(f"Gelöscht: {filepath}")
    except Exception as e:
        print(f"Fehler beim Löschen der Datei {filepath}: {e}")

def get_directory_size(directory):
    """Berechnet die Gesamtgröße eines Verzeichnisses (rekursiv) in Bytes."""
    total_size = 0
    for dirpath, dirnames, filenames in os.walk(directory):
        for filename in filenames:
            filepath = os.path.join(dirpath, filename)
            total_size += os.path.getsize(filepath)
    return total_size

def log_directory_sizes(source_dir, comp_dir):
    """Loggt die Gesamtgröße der ProRes-Quelldateien und des Komprimierungsverzeichnisses."""
    total_source_size = sum(
        os.path.getsize(os.path.join(source_dir, filename)) 
        for filename in os.listdir(source_dir) 
        if filename.lower().endswith('.mov') and 'prores' in filename.lower()
    )
    total_source_size_gb = total_source_size / (1024 * 1024 * 1024)
    
    current_compression_size = get_directory_size(comp_dir)
    current_compression_size_gb = current_compression_size / (1024 * 1024 * 1024)

    print(f"Größe aller ProRes-Quelldateien: {total_source_size_gb:.2f} GB")
    print(f"Größe des Komprimierungsverzeichnisses: {current_compression_size_gb:.2f} GB")

def can_move_to_compression(comp_dir, source_file_size, max_gb):
    """Überprüft, ob eine Datei ins Komprimierungsverzeichnis verschoben werden kann, ohne das Limit zu überschreiten."""
    max_compression_size = max_gb * 1024 * 1024 * 1024  # Umrechnung von GB in Bytes
    current_size = get_directory_size(comp_dir)
    remaining_space = max_compression_size - current_size

    if remaining_space <= 0:
        print(f"Das Komprimierungsverzeichnis hat bereits das Limit von {max_gb} GB erreicht.")
        return False

    if source_file_size > remaining_space:
        print(f"Überspringe Datei. Das Verschieben von {source_file_size / (1024 * 1024 * 1024):.2f} GB würde das verbleibende Limit überschreiten ({remaining_space / (1024 * 1024 * 1024):.2f} GB verfügbar).")
        return False

    return True