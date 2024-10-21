# CLI-Design Konzept für Typer: Verben-orientierte Struktur mit Packages und Subcommands

Dieses Konzept beschreibt die Strukturierung einer Typer-basierten CLI-Anwendung, die sich nach Verben orientiert und modular über mehrere Packages hinweg organisiert ist. Ziel ist es, klare und intuitive Befehle wie `kurmann-videoschnitt list mediasets` oder `mediaset-manager list mediasets` zu ermöglichen, die sowohl über das übergeordnete CLI als auch direkt über Sub-Packages aufrufbar sind.

## 1. Grundprinzipien des CLI-Designs mit Typer und Verben

Typer ermöglicht die Erstellung flexibler Kommandozeilenanwendungen durch die Nutzung von **Typer-Instanzen** für verschiedene Befehlsgruppen. Eine verben-orientierte Struktur sorgt für intuitive und leicht verständliche Befehle, die Aktionen wie `list`, `create` oder `delete` widerspiegeln.

## 2. Modularität und Verben-orientierte Struktur

Die CLI wird in mehrere **Packages** unterteilt, wobei jedes Package spezifische Funktionalitäten abdeckt (z.B. Medienverwaltung, Metadaten-Erstellung). Innerhalb jedes Packages werden **Commands** nach Verben organisiert, um eine klare Trennung und einfache Erweiterbarkeit zu gewährleisten.

## 3. Übergeordnetes CLI (z.B. "kurmann-videoschnitt")

Das Haupt-CLI dient als **Entry-Point** für die gesamte Anwendung und registriert die Sub-Packages. Dadurch können Befehle sowohl über das Haupt-CLI als auch direkt über die Sub-Packages aufgerufen werden.

## 4. Design des CLI

### 4.1. Dateistruktur

Die CLI-Anwendung ist in einer modularen Struktur organisiert, die mehrere Packages und deren Befehle umfasst:

```
├── pyproject.toml
├── README.md
└── src
    ├── kurmann_videoschnitt
    │   └── app.py
    ├── mediaset_manager
    │   ├── app.py
    │   └── commands
    │       ├── list.py
    │       └── create.py
    └── other_package
        └── app.py
```

### 4.2. Typer in den einzelnen Packages

Jedes Package hat eine eigene `app.py`, die eine **Typer-Instanz** enthält und die spezifischen **Commands** registriert.

#### Beispiel: `mediaset_manager/app.py`

```python
import typer
from mediaset_manager.commands.list import list_mediasets_command, list_grouped_media_command
from mediaset_manager.commands.create import create_metadata_file_command

app = typer.Typer()

# Registriere die Commands mit verb-orientierten Namen
app.command("list-mediasets")(list_mediasets_command)
app.command("list-grouped-media")(list_grouped_media_command)
app.command("create-metadata-file")(create_metadata_file_command)

if __name__ == "__main__":
    app()
```

### 4.3. Commands als Module

Die spezifischen Commands werden in separaten Modulen innerhalb eines `commands`-Verzeichnisses organisiert.

#### Beispiel: `mediaset_manager/commands/list.py`

```python
import typer

def list_mediasets_command():
    """
    Listet alle Mediensets auf.
    """
    typer.echo("Mediensets auflisten...")

def list_grouped_media_command():
    """
    Listet gruppierte Mediendateien auf.
    """
    typer.echo("Gruppierte Mediendateien auflisten...")
```

#### Beispiel: `mediaset_manager/commands/create.py`

```python
import typer

def create_metadata_file_command():
    """
    Erstellt eine Metadata-Datei für ein Medienset.
    """
    typer.echo("Metadata-Datei erstellen...")
```

### 4.4. Übergeordnetes CLI für mehrere Packages

Im Hauptpackage (`kurmann_videoschnitt`) wird eine zentrale `app.py` erstellt, die die **Typer-Instanzen** der Sub-Packages registriert. Dadurch können die Befehle sowohl über das Haupt-CLI als auch direkt über die Sub-Packages aufgerufen werden.

#### Beispiel: `kurmann_videoschnitt/app.py`

```python
import typer
from mediaset_manager.app import app as mediaset_manager_app
from other_package.app import app as other_app

app = typer.Typer()

# Registriere die Sub-Package Typer-Instanzen als Subcommands
app.add_typer(mediaset_manager_app, name="mediaset-manager")
app.add_typer(other_app, name="other-package")

if __name__ == "__main__":
    app()
```

### 4.5. Direkte Nutzung von Verben in der Haupt-CLI

Um die Befehle kürzer und direkter nutzbar zu machen, können die **Commands** der Sub-Packages auch direkt in der Haupt-CLI registriert werden. Dadurch können Befehle wie `kurmann-videoschnitt list mediasets` direkt ausgeführt werden, ohne den Sub-Package-Namen explizit anzugeben.

#### Beispiel: Direkte Command-Registrierung in `kurmann_videoschnitt/app.py`

```python
import typer
from mediaset_manager.commands.list import list_mediasets_command, list_grouped_media_command
from mediaset_manager.commands.create import create_metadata_file_command

app = typer.Typer()

# Registriere die Commands direkt mit verb-orientierten Namen
app.command("list-mediasets")(list_mediasets_command)
app.command("list-grouped-media")(list_grouped_media_command)
app.command("create-metadata-file")(create_metadata_file_command)

if __name__ == "__main__":
    app()
```

Damit können Befehle wie `kurmann-videoschnitt list-mediasets` und `kurmann-videoschnitt create-metadata-file` direkt ausgeführt werden.

## 5. Registrierung in `pyproject.toml`

Um sowohl das übergeordnete CLI als auch die Sub-Packages eigenständig aufrufbar zu machen, werden die entsprechenden Entry-Points in der `pyproject.toml` definiert.

#### Beispiel: `pyproject.toml`

```toml
[project.scripts]
kurmann-videoschnitt = "kurmann_videoschnitt.app:app"
mediaset-manager = "mediaset_manager.app:app"
other-package = "other_package.app:app"
```

Damit können sowohl das übergeordnete CLI (`kurmann-videoschnitt`) als auch die Sub-Packages (`mediaset-manager`, `other-package`) eigenständig verwendet werden.

## 6. Beispielhafte Befehle

- **Übergeordnetes CLI:**
  - `kurmann-videoschnitt list-mediasets`: Listet alle Mediensets über das Haupt-CLI auf.
  - `kurmann-videoschnitt create-metadata-file`: Erstellt eine Metadaten-Datei für ein Medienset.
  
- **Sub-Package CLI:**
  - `mediaset-manager list-mediasets`: Listet alle Mediensets über das `mediaset-manager`-Sub-Package auf.
  - `mediaset-manager create-metadata-file`: Erstellt eine Metadaten-Datei im `mediaset-manager`.

## 7. CLI-Design Entscheidungen

### 7.1. Verben-orientierte Struktur

- **Vorteil:** Intuitive und leicht verständliche Befehle, die Aktionen klar widerspiegeln.
- **Nachteil:** Bei einer großen Anzahl von Commands kann die Struktur komplex werden.

### 7.2. Modulare Struktur für Verben

- **Vorteil:** Saubere Trennung der Befehle und einfache Wartbarkeit durch Module und Sub-Packages.
- **Nachteil:** Erfordert sorgfältige Planung, um Kollisionen und Redundanzen zu vermeiden.

### 7.3. Kombination aus beiden Ansätzen

- **Vorteil:** Flexibilität, indem einige wichtige Commands direkt in der Haupt-CLI registriert werden, während andere über Sub-Packages verwaltet werden.
- **Nachteil:** Kann die Struktur komplizierter machen, wenn nicht klar zwischen direkt registrierten und sub-package Commands unterschieden wird.

## 8. Zusammenfassung

- **Verben-orientierte Befehle:** Die CLI ist nach Verben wie `list`, `create` organisiert, was die Benutzerfreundlichkeit erhöht.
- **Modularität:** Jede Funktionalität ist in einem eigenen Package mit spezifischen Commands organisiert.
- **Flexibilität:** Befehle können sowohl über das übergeordnete CLI als auch direkt über die Sub-Packages aufgerufen werden.
- **Eigenständige Aufrufbarkeit:** Sub-Packages bleiben eigenständig aufrufbar durch die Registrierung in der `pyproject.toml`.
