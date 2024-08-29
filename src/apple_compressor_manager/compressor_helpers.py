import os
import subprocess
import time
from datetime import timedelta
from apple_compressor_manager.compressor_utils import are_sb_files_present
from apple_compressor_manager.video_utils import get_video_codec

def send_macos_notification(title, message):
    """Sendet eine macOS-Benachrichtigung."""
    subprocess.run([
        "osascript", "-e", f'display notification "{message}" with title "{title}"'
    ])

def process_batch(files, compressor_profile_path, check_interval, max_checks):
    """Verarbeitet einen Batch von Dateien und prüft periodisch, welche fertig komprimiert sind."""
    # Starte die Kompression für den gesamten Batch
    for input_file, output_file in files:
        # Dynamischer Job-Titel
        job_title = f"Kompression '{os.path.basename(input_file)}' zu HEVC-A"
        
        command = [
            "/Applications/Compressor.app/Contents/MacOS/Compressor",
            "-batchname", job_title,
            "-jobpath", input_file,
            "-locationpath", output_file,
            "-settingpath", compressor_profile_path
        ]
        try:
            subprocess.run(command, check=False)
            print(f"Kompressionsauftrag erstellt für: {input_file} (Job-Titel: {job_title})")
        except subprocess.CalledProcessError as e:
            print(f"Fehler bei der Komprimierung von {input_file}: {e}")

    # Warte und prüfe periodisch den Status der Kompression
    check_count = 0
    total_wait_time = check_interval * max_checks  # Gesamtwartezeit in Sekunden
    print(f"Das Skript wird insgesamt bis zu {timedelta(seconds=total_wait_time)} warten, um die Kompression zu überprüfen.")

    while files and check_count < max_checks:
        time.sleep(check_interval)
        check_count += 1
        print(f"Überprüfung {check_count}/{max_checks} nach {timedelta(seconds=check_count * check_interval)}...")

        for input_file, output_file in files[:]:  # Verwende eine Kopie der Liste, um sicher zu iterieren
            print(f"Prüfe Status für: {output_file}")

            # Prüfen, ob die Datei vorhanden ist (hat die Kompression begonnen?)
            if not os.path.exists(output_file):
                print(f"Komprimierung für: {output_file} noch nicht gestartet")
                continue

            # Prüfen, ob temporäre .sb-Dateien vorhanden sind (läuft die Kompression noch?)
            if are_sb_files_present(output_file):
                print(f"Komprimierung für: {output_file} läuft noch")
                continue

            # Prüfen, ob die komprimierte Datei den Codec "hevc" hat
            codec = get_video_codec(output_file)
            if codec != "hevc":
                print(f"Fehlerhafter Codec für: {output_file}. Erwartet: 'hevc', erhalten: '{codec}'")
                continue

            print(f"Komprimierung abgeschlossen: {output_file}")
            try:
                os.remove(input_file)
                print(f"Originaldatei gelöscht: {input_file}")
                files.remove((input_file, output_file))  # Entferne die Datei aus der Liste
                check_count = 0  # Timer zurücksetzen, da eine Datei erfolgreich abgeschlossen wurde
            except Exception as e:
                print(f"Fehler beim Löschen der Datei: {e}")

    if files:
        print(f"Maximale Überprüfungsanzahl erreicht. {len(files)} Dateien wurden nicht erfolgreich verarbeitet.")
        print(f"Das Skript hat insgesamt {timedelta(seconds=total_wait_time)} gewartet.")

def final_cleanup(input_directory, output_directory):
    """Prüft alle komprimierten Dateien im Verzeichnis und löscht ggf. Originaldateien."""
    for root, _, files in os.walk(input_directory):
        for file in files:
            if not file.lower().endswith(".mov"):
                continue

            input_file = os.path.join(root, file)
            output_file = os.path.join(output_directory, f"{os.path.splitext(file)[0]}-HEVC-A.mov")

            if os.path.exists(output_file) and not are_sb_files_present(output_file):
                codec = get_video_codec(output_file)
                if codec == "hevc":
                    try:
                        os.remove(input_file)
                        print(f"Abschließende Überprüfung: Originaldatei gelöscht: {input_file}")
                    except Exception as e:
                        print(f"Fehler beim Löschen der Datei während der abschließenden Überprüfung: {e}")