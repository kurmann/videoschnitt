# Kommunikation zwischen Python-Packages in einer modularen Architektur

In diesem Kapitel wird erklärt, wie du eine effiziente Kommunikation zwischen Python-Packages realisierst, die lokal installiert und über PIP verwaltet werden. Der Fokus liegt auf direkter Aufruf- und Rückmeldefunktionalität innerhalb einer modularen Architektur. Es wird Methode 1 (Callback-Funktion) eingesetzt, die in Python sehr einfach zu implementieren ist und sowohl Flexibilität als auch Skalierbarkeit bietet.

## Projektübersicht

In unserem Beispiel haben wir verschiedene Python-Packages, die unabhängig voneinander arbeiten, aber dennoch miteinander kommunizieren müssen. Jedes dieser Packages ist über PIP installiert und bietet CLI-Befehle zum direkten Aufruf. Gleichzeitig gibt es die Anforderung, dass bestimmte Module nach Abschluss einer Aufgabe Rückmeldungen an andere Module senden.

### Ziele:
- **Direkte Aufrufe von Packages:** Die Pakete sollen über die Kommandozeile aufrufbar bleiben, beispielsweise `import-new-media`, `integrate-new-media`.
- **Rückkanal zur Kommunikation:** Wenn ein Package eine Aufgabe erledigt, soll es optional eine Rückmeldung an das aufrufende Package senden können.

## Implementierung der Kommunikation mit Callback-Funktionen

### Schritt 1: Direktes Aufrufen von Packages

Stelle sicher, dass die Packages über PIP installiert und über CLI aufrufbar sind. Die Konfiguration erfolgt in der `pyproject.toml`-Datei, in der du die Einträge für die CLI-Befehle definierst:

```toml
[project]
name = "kurmann_videoschnitt"
version = "0.43.0"
description = "Video editing automation tools for Kurmann Videoschnitt"
dependencies = ["click"]

[project.scripts]
import-new-media = "new_media_importer.import_new_media:main"
integrate-new-media = "original_media_integrator.integrate_new_media:main"
compress-prores = "apple_compressor_manager.compress_prores:main"
```

Mit dieser Konfiguration kannst du die Packages einfach mit Befehlen wie `import-new-media` ausführen.

### Schritt 2: Optionaler Rückkanal durch Callback-Funktion

Um eine Rückmeldung zu ermöglichen, kannst du eine Callback-Funktion als optionales Argument an die aufgerufenen Funktionen übergeben. Diese Funktion wird nur aufgerufen, wenn eine Rückmeldung erforderlich ist.

#### Beispiel: Hauptmodul (Neumedien-Import)

```python
from apple_compressor_manager.compress_prores import compress_files
from original_media_integrator.integrate_new_media import organize_media_files

def on_file_ready(file_path):
    print(f"Die Datei wurde erfolgreich verarbeitet: {file_path}")

def main():
    # Parameter und Konfigurationen laden...
    
    if compress_flag:
        compress_files(source_directory, destination_directory, callback=on_file_ready)

    organize_media_files(source_directory, destination_directory)

if __name__ == "__main__":
    main()
```

#### Beispiel: Modul Apple Compressor Manager

```python
def compress_files(source_directory, destination_directory, callback=None):
    # Simulierte Verarbeitung von Dateien
    for file in get_files_to_compress(source_directory):
        # Hier die eigentliche Kompression durchführen
        print(f"Komprimiere Datei: {file}")
        
        # Wenn eine Callback-Funktion übergeben wurde, rufe sie auf
        if callback:
            callback(file)
```

In diesem Beispiel sendet das Modul `compress_files` eine Rückmeldung an das aufrufende Modul, sobald eine Datei verarbeitet wurde. Falls keine Rückmeldung erforderlich ist, bleibt die Callback-Funktion optional.

### Vorteile dieser Lösung
- **Flexibilität:** Jedes Package kann unabhängig arbeiten, aber bei Bedarf dennoch Informationen an andere Module zurückgeben.
- **Leichte Erweiterbarkeit:** Du kannst später auf eine andere Methode (wie eine Web-API) umsteigen, ohne die Kernlogik grundlegend ändern zu müssen.
- **Unabhängigkeit der Module:** Die Module bleiben über CLI aufrufbar und sind nicht auf die Rückkanalfunktionalität angewiesen.

## Zusammenfassung

Diese Architektur ermöglicht dir die flexible Nutzung deiner Python-Packages über die Kommandozeile, kombiniert mit der Möglichkeit, Rückmeldungen zwischen den Modulen zu ermöglichen. Das bietet sowohl Entwicklern als auch Benutzern eine hohe Flexibilität und lässt sich leicht in größere Projekte integrieren.
