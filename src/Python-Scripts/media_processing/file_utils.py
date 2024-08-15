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

    try:
        os.makedirs(destination_dir, exist_ok=True)  # Stellt sicher, dass das Zielverzeichnis existiert
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

def delete_file(file_path):
    """Löscht die angegebene Datei."""
    try:
        if os.path.exists(file_path):
            os.remove(file_path)
            print(f"Datei gelöscht: {file_path}")
        else:
            print(f"Datei nicht gefunden: {file_path}")
    except Exception as e:
        print(f"Fehler beim Löschen der Datei: {e}")