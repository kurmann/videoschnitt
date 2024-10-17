# Guideline zur Gestaltung von Python-Packages und CLI-Tools mit Typer

Diese Guideline beschreibt, wie Python-Packages und CLI-Tools strukturiert werden sollten, um eine saubere und modularisierte Entwicklung sicherzustellen. Sie richtet sich insbesondere an Entwickler, die Typer verwenden, um einfache und leistungsfähige Command-Line-Interfaces (CLI) zu erstellen. Die hier beschriebenen Vorgehensweisen können auch von KI-Systemen wie ChatGPT beachtet und umgesetzt werden.

## 1. Strukturierung von Python-Packages

Ein gut strukturiertes Python-Projekt hilft dabei, die Übersicht zu behalten und zukünftige Erweiterungen oder Änderungen zu erleichtern. Es ist wichtig, eine klare Trennung von Verantwortlichkeiten in den Modulen vorzunehmen und eine konsistente Verzeichnisstruktur zu etablieren.

### Verzeichnisstruktur:

```
project_root/
│
├── src/
│   ├── package_name/
│   │   ├── init.py
│   │   ├── commands/
│   │   │   ├── command1.py
│   │   │   ├── command2.py
│   │   │   └── …
│   │   ├── utils/
│   │   │   ├── helper1.py
│   │   │   └── helper2.py
│   │   └── app.py  # Haupt-CLI-Datei
│   └── another_package/
│       ├── init.py
│       └── …
├── README.md
└── pyproject.toml / setup.py  # Für Package-Verwaltung
```

In dieser Struktur werden alle Commands in einem eigenen `commands/`-Verzeichnis gehalten, während Hilfsfunktionen oder Utility-Klassen in einem `utils/`-Verzeichnis organisiert werden. Die `app.py` agiert als Hauptdatei, welche die CLI-Kommandos registriert.

## 2. Erstellen von Commands

Jeder Command sollte in einer eigenen Datei unter `commands/` gespeichert werden. Dies fördert die Modularität und Übersichtlichkeit.

### Beispiel für einen Command (`get_resolution.py`):

```python
import typer
import subprocess

def get_resolution_command(filepath: str):
    """
    Gibt die Auflösung einer Videodatei zurück.
    """
    resolution = get_video_resolution(filepath)
    typer.secho(f"Auflösung: {resolution}", fg=typer.colors.GREEN)

def get_video_resolution(filepath: str) -> str:
    # Logik zur Ermittlung der Auflösung
    pass

if __name__ == "__main__":
    typer.run(get_resolution_command)
```

In diesem Beispiel ist der Command get_resolution_command so strukturiert, dass er die Auflösung einer Videodatei ermittelt und an die Konsole ausgibt.

## 3. Integrieren der Commands in die app.py

Die app.py Datei dient als zentrales Bindeglied zwischen den einzelnen Commands. Alle Commands werden in der app.py registriert und dort als Teil der CLI verwendet.

Beispiel (app.py):

```python
import typer
from package_name.commands.get_resolution import get_resolution_command
from package_name.commands.get_title import get_title_command

app = typer.Typer()

# Registriere die Commands
app.command("get-resolution")(get_resolution_command)
app.command("get-title")(get_title_command)

if __name__ == "__main__":
    app()
```

Dies sorgt dafür, dass beim Aufruf der Anwendung alle registrierten Commands zur Verfügung stehen.

## 4. Verwendung von Typer

Typer wird als zentrales Werkzeug für die CLI-Entwicklung verwendet. Es ermöglicht eine einfache und klare Verwaltung von Befehlen, Argumenten und Optionen.

Best Practices für Typer:

- Kurz- und Langversionen von Argumenten: Jedes Argument hat eine lange Version (z.B. --output-file) und eine kurze Version (z.B. -o), um die Bedienung zu erleichtern.

    ```python
    @app.command()
    def example_command(option: str = typer.Option(..., "--option", "-o")):
        typer.echo(f"Option: {option}")
    ```

- Argumente und Optionen: Argumente sind zwingend erforderlich, während Optionen optional sind und über Typing und Default-Werte definiert werden.

    ```python
    @app.command()
    def convert_image(image_path: str, output_file: str = typer.Option("output.jpg")):
        # Logik zur Konvertierung des Bildes
        pass
    ```

- Docstrings: Jede CLI-Methode sollte eine kurze, prägnante erste Zeile im Docstring enthalten, gefolgt von einer detaillierteren Beschreibung. Dies hilft Typer bei der automatischen Generierung der Hilfe (--help).

    ```python
    def get_resolution_command(filepath: str):
        """
        Ermittelt die Auflösung einer Videodatei.

        Dieser Command verwendet ffprobe oder exiftool, um die Auflösung
        der angegebenen Videodatei zu extrahieren.
        """
        pass
    ```

- Fehlerbehandlung und Bestätigungen: Für Aktionen, die potenziell kritisch sind (z.B. Überschreiben von Dateien), werden Bestätigungen eingebaut, um den Benutzer zu fragen, ob er fortfahren möchte.

    ```python
    if os.path.exists(output_file):
        overwrite = typer.confirm(f"{output_file} existiert bereits. Überschreiben?")
        if not overwrite:
            raise typer.Exit()
    ```

## 5. Imports

Eine wichtige Best Practice ist es, alle Imports über den vollständigen Packagenamen durchzuführen. Dadurch wird sichergestellt, dass alle Abhängigkeiten konsistent gefunden werden und der Code modular bleibt.

Beispiel:

```python
# Falsch:
from utils import helper_function

# Richtig:
from package_name.utils.helper_function import helper_function
```

Dieser Ansatz fördert die Wartbarkeit und verhindert Namenskollisionen.

## 6. Rückfragen und Bestätigungen

Beim Erstellen von CLI-Tools ist es wichtig, Rückfragen und Bestätigungen einzubauen, um die Benutzerfreundlichkeit zu erhöhen. Dies verhindert ungewollte Aktionen und sorgt für mehr Kontrolle.

Beispiel:

```python
if os.path.exists(output_file):
    overwrite = typer.confirm(f"{output_file} existiert bereits. Möchten Sie es überschreiben?")
    if not overwrite:
        typer.secho("Aktion abgebrochen.", fg=typer.colors.RED)
        raise typer.Exit()
```

In diesem Beispiel wird der Benutzer gefragt, ob er eine existierende Datei überschreiben möchte, bevor die Aktion ausgeführt wird.
