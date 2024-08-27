# Architektur und Struktur der Packages

Dieses Dokument beschreibt, wie du eine modulare Struktur für deine Python-Packages aufbaust, die sowohl eigenständig nutzbar als auch in einer übergeordneten CLI integriert werden können. Der Fokus liegt darauf, wie du mehrere unabhängige Packages und Sub-Packages organisierst und eine flexible CLI mit Click implementierst.

## Organisieren von Packages und Sub-Packages

### Grundlegende Prinzipien der Package-Struktur

In größeren Projekten ist es sinnvoll, die Funktionalitäten in mehrere Packages und Sub-Packages zu unterteilen. Diese Struktur ermöglicht es, unabhängige Module zu entwickeln, die sowohl einzeln als auch in Kombination verwendet werden können.

#### Vorteile der Modularisierung

- **Wiederverwendbarkeit:** Module können in verschiedenen Projekten wiederverwendet werden.
- **Unabhängigkeit:** Änderungen in einem Package beeinflussen nicht notwendigerweise andere Teile des Projekts.
- **Flexibilität:** Verschiedene Module können eigenständig getestet und entwickelt werden.
- **Einfache Erweiterung:** Neue Funktionalitäten lassen sich leicht hinzufügen, ohne die bestehende Struktur zu stören.

### Beispiel für eine Verzeichnisstruktur mit Sub-Packages

Angenommen, du hast ein Package, das die Medienintegration als Hauptaufgabe hat. Innerhalb dieses Packages gibt es mehrere Sub-Packages, die spezialisierte Aufgaben übernehmen:

```plaintext
kurmann_videoschnitt/
├── src/
│   ├── integrate_new_media/
│   │   ├── __init__.py
│   │   ├── neumedien_import/
│   │   │   ├── __init__.py
│   │   │   ├── cli.py
│   │   │   └── importer.py
│   │   ├── apple_compressor_manager/
│   │   │   ├── __init__.py
│   │   │   ├── cli.py
│   │   │   └── compressor.py
│   │   ├── prores_cleanup/
│   │   │   ├── __init__.py
│   │   │   ├── cli.py
│   │   │   └── cleaner.py
│   │   ├── original_media_integrator/
│   │   │   ├── __init__.py
│   │   │   ├── cli.py
│   │   │   └── integrator.py
│   │   └── cli.py  # Zentrale CLI, die die Sub-Packages orchestriert
│   └── main.py
├── docs/
│   ├── manual/
│   ├── reference/
│   └── mkdocs.yml
└── pyproject.toml
```

#### Erläuterung der Struktur

- **Hauptpackage:** `integrate_new_media` gruppiert verwandte Sub-Packages.
- **Sub-Packages:** Jedes Sub-Package (`neumedien_import`, `apple_compressor_manager`, `prores_cleanup`, `original_media_integrator`) ist eigenständig und verfügt über seine eigene CLI, Logik und Tests.
- **Zentrale CLI:** Die Datei `cli.py` im `integrate_new_media`-Package bietet eine zentrale CLI, die die Sub-Packages orchestriert.

## CLI-Design und die Verwendung von Click

### Aufbau einer flexiblen CLI mit Click

[Click](https://click.palletsprojects.com/) ist ein beliebtes Python-Framework, um benutzerfreundliche Kommandozeilen-Interfaces (CLI) zu erstellen. Es bietet viele eingebaute Features wie Eingabevalidierung, Hilfe-Generierung und einfache Subcommand-Strukturen.

#### Struktur einer CLI mit Click

Click erlaubt es, sowohl eigenständige CLIs pro Package zu erstellen als auch eine zentrale CLI, die alle Sub-Packages orchestriert.

**Beispiel einer CLI-Struktur:**

```python
# src/integrate_new_media/cli.py

import click
from integrate_new_media.neumedien_import.cli import cli as neumedien_import_cli
from integrate_new_media.apple_compressor_manager.cli import cli as compressor_cli
from integrate_new_media.prores_cleanup.cli import cli as cleanup_cli
from integrate_new_media.original_media_integrator.cli import cli as integrator_cli

@click.group()
def cli():
    """Zentrale CLI für die Medienintegration."""
    pass

# Subcommands aus den Sub-Packages hinzufügen
cli.add_command(neumedien_import_cli, name="import-new-media")
cli.add_command(compressor_cli, name="compress-media")
cli.add_command(cleanup_cli, name="cleanup-media")
cli.add_command(integrator_cli, name="integrate-original-media")

if __name__ == "__main__":
    cli()
```

In diesem Beispiel wird eine zentrale CLI erstellt, die die Subcommands der einzelnen Sub-Packages zusammenführt. Jeder Befehl wie `import-new-media`, `compress-media` etc. kann dann unabhängig ausgeführt werden.

#### CLI in den Sub-Packages

Die Sub-Packages haben ihre eigene CLI, die eigenständig funktioniert:

```python
# src/integrate_new_media/neumedien_import/cli.py

import click
from .importer import import_media

@click.command()
@click.argument('source_directory', type=click.Path(exists=True))
def cli(source_directory):
    """Importiert neue Medien aus dem Quellverzeichnis."""
    import_media(source_directory)
```

### Validierung und CLI-Design

Click bietet eingebaute Validierungen für Pfade, Auswahlmöglichkeiten und vieles mehr. Es ist wichtig, die Validierung so zu strukturieren, dass grundlegende Überprüfungen in Click stattfinden, während geschäftslogik-spezifische Validierungen in den Methoden selbst durchgeführt werden.

#### Validierungen in Click

Click eignet sich für:

- **Pfadüberprüfungen:** Überprüfen, ob ein Verzeichnis oder eine Datei existiert.
- **Typüberprüfungen:** Sicherstellen, dass Eingaben dem erwarteten Datentyp entsprechen.
- **Wahlmöglichkeiten:** Validieren, ob ein Wert in einer vorgegebenen Liste von Optionen liegt.

```python
@click.command()
@click.argument('directory', type=click.Path(exists=True, file_okay=False))
@click.option('--level', type=click.Choice(['low', 'medium', 'high']), default='medium')
def cli(directory, level):
    """Ein Beispielbefehl mit Validierungen."""
    # Logik folgt
```

#### Validierungen in den Methoden

Geschäftslogik-spezifische Validierungen sollten in der Methode selbst durchgeführt werden:

```python
def validate_and_process(directory):
    if not os.listdir(directory):
        raise ValueError("Das Verzeichnis ist leer.")
    # Weiterverarbeitung
```
