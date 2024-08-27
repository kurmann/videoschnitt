# Ausbau mit einer WebAPI und Verwendung von Callbacks in Python

Dieses Dokument beschreibt, wie du dein bestehendes Projekt um eine WebAPI erweitern kannst, die dieselben Python-Module nutzt, die auch über die CLI aufgerufen werden. Darüber hinaus wird das Konzept von Callback-Funktionen erklärt und wie sie sinnvoll in einem modularen Python-Projekt eingesetzt werden können.

## Ausbau mit einer WebAPI

### Überblick

In einem modularen System wie deinem Projekt kann es sinnvoll sein, neben der CLI auch eine WebAPI bereitzustellen. Diese ermöglicht es, die vorhandene Logik über HTTP(S)-Anfragen zu nutzen, sei es zur Automatisierung, für externe Systeme oder zur Integration in andere Dienste.

Eine WebAPI kann dieselben Module und Funktionen aufrufen wie die CLI, was bedeutet, dass du die Logik nicht doppelt implementieren musst. Typische Python-Frameworks, die sich für eine WebAPI eignen, sind **FastAPI** und **Flask**.

### Implementierung mit FastAPI

FastAPI ist ein schnelles und leichtgewichtiges Framework, das sich besonders gut für Projekte eignet, die bereits stark auf Python-Typisierung setzen. Es bietet außerdem automatische Generierung von OpenAPI-Dokumentation und eine benutzerfreundliche Oberfläche zur API-Interaktion.

#### Beispielstruktur

Angenommen, deine aktuelle Projektstruktur sieht so aus:

```plaintext
kurmann_videoschnitt/
├── src/
│   ├── integrate_new_media/
│   ├── api/
│   │   └── app.py  # API-Einstiegspunkt
│   └── integrate_new_media/cli.py  # CLI-Einstiegspunkt
└── pyproject.toml
```

In diesem Fall könntest du eine FastAPI-basierte API im `api/`-Verzeichnis implementieren.

#### Beispiel für eine API-Route

```python
# src/api/app.py

from fastapi import FastAPI
from integrate_new_media.apple_compressor_manager.compressor import compress_files

app = FastAPI()

@app.post("/compress/")
def compress_media(source_directory: str, destination_directory: str):
    processed_files = compress_files(source_directory, destination_directory)
    return {"processed_files": processed_files}
```

Diese API bietet eine POST-Route `/compress/`, die zwei Parameter (`source_directory`, `destination_directory`) erwartet und dann die Komprimierungslogik ausführt.

#### Vorteile dieser Architektur

- **Wiederverwendbarkeit der Logik:** Die gleiche Logik, die in der CLI verwendet wird, kann auch in der API genutzt werden.
- **Flexibilität:** Du kannst sowohl eine Kommandozeilen-Oberfläche als auch eine HTTP-Schnittstelle anbieten.
- **Typisierte Parameter und Rückgabewerte:** FastAPI unterstützt die native Python-Typisierung, was zu saubererem Code und besserer Dokumentation führt.

### Zusammenführung von CLI und API

Wenn du sowohl eine CLI als auch eine API anbietest, ist es sinnvoll, dieselbe Logik in deinen Modulen zu verwenden. Die CLI dient dabei für lokale und automatisierte Aufgaben, während die API für externe Systeme oder komplexe Integrationen genutzt werden kann.

## Verwendung von Callback-Funktionen

### Was sind Callbacks?

Ein Callback ist eine Funktion, die als Argument an eine andere Funktion übergeben wird und innerhalb dieser Funktion aufgerufen wird, sobald eine bestimmte Aktion abgeschlossen ist. Callbacks sind besonders dann nützlich, wenn du erweiterbare oder asynchrone Prozesse integrieren möchtest.

### Beispiel für die Verwendung eines Callbacks

```python
def process_data(data, callback=None):
    result = data * 2
    if callback:
        callback(result)

def print_callback(result):
    print(f"Das Ergebnis ist: {result}")

process_data(5, callback=print_callback)
```

In diesem Beispiel wird die Funktion `print_callback` aufgerufen, sobald die Datenverarbeitung abgeschlossen ist.

### Callbacks in einem modularen System

In einem modularen Projekt wie deinem können Callbacks dazu genutzt werden, zusätzliche Logik flexibel anzuhängen, ohne dass die Kernfunktionalität geändert werden muss. Beispiele sind Benachrichtigungen, Logging oder das Auslösen von Folgeaktionen.

### Einschränkungen bei der CLI-Nutzung

Während Callbacks beim direkten Modulaufruf äußerst sinnvoll sind, haben sie bei der Nutzung über die CLI keinen direkten Mehrwert. Das liegt daran, dass eine CLI in der Regel keine Funktionen übergibt, sondern nur Text- und Dateieingaben verarbeitet. Der Callback-Parameter wird daher in der CLI weder angezeigt noch genutzt.

**Beispiel für eine CLI-Integration ohne Callback:**

```python
import click
from my_module import process_data

@click.command()
@click.argument('data', type=int)
def cli(data):
    """Verarbeitet Daten und gibt das Ergebnis aus."""
    process_data(data)

if __name__ == '__main__':
    cli()
```

### Fazit zur Verwendung von Callbacks

- **Direkter Modulaufruf:** Callbacks sind nützlich für flexible und erweiterbare Logik.
- **CLI-Aufruf:** Callbacks sind hier nicht sinnvoll, da die CLI nicht mit dynamischen Funktionen arbeitet. Stattdessen solltest du hier auf Optionen und Flags setzen, um verschiedene Ausgaben zu steuern.
