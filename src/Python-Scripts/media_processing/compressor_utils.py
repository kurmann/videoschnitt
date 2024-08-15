import subprocess
import time

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