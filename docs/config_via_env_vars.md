# Konzept: Konfiguration der CLI-App über Docker mit Umgebungsvariablen und Typer CLI

Dieses Konzept beschreibt, wie du deine Python-CLI-Anwendungen, die auf der Typer-Bibliothek basieren, in Docker betreiben kannst. Es werden zwei Ansätze vorgestellt: **Mehrere Services** für jede CLI-Anwendung oder ein **Gesamt-Service**, der alle CLI-Befehle in einer übergeordneten CLI zusammenführt.

## 1. Einführung

In vielen Anwendungsfällen, vor allem bei der Arbeit mit Docker, ist es sinnvoll, Konfigurationswerte wie Verzeichnispfade über Umgebungsvariablen zu steuern und die CLI-Befehle flexibel in einer Docker-Umgebung nutzbar zu machen. Hier werden zwei Varianten beschrieben, wie dies erreicht werden kann.

- **Variante 1:** Jeder CLI-Befehl wird als separater Docker-Service definiert.
- **Variante 2:** Alle CLI-Befehle werden zu einem übergeordneten Gesamt-CLI zusammengeführt.

## 2. Ziel des Konzepts

- Die CLI-Anwendung sollte flexibel über Docker nutzbar sein.
- Umgebungsvariablen werden über eine `.env`-Datei definiert.
- Befehle werden entweder als einzelne Docker-Services oder als Gesamt-Service über einen Haupt-CLI zugänglich gemacht.

## 3. Docker Compose Setup

### 3.1 Erstellung der `.env`-Datei

Die `.env`-Datei enthält die Konfigurationswerte für die Anwendung. In diesem Beispiel definieren wir Pfade für das Quell- und Zielverzeichnis der Movie-Integration sowie des Original-Media-Integrators.

```env
MOVIE_SOURCE_DIR=/volume/movies-integration
MOVIE_TARGET_DIR=/volume/movies-archive
ORIGINAL_MEDIA_SOURCE_DIR=/volume/original-media
ORIGINAL_MEDIA_TARGET_DIR=/volume/original-archive
```

Diese Datei sollte sich im Hauptverzeichnis des Projekts befinden und wird sowohl von Docker Compose als auch von der Python-Anwendung verwendet.

### 3.2 Variante 1: Mehrere Services

In dieser Variante wird für jedes CLI-Package ein separater Docker-Service erstellt, der auf den jeweiligen CLI-Befehl zeigt. Jeder Service hat seinen eigenen Entry-Point.

```yaml
version: '3'
services:
  movie-integrator:
    build: .
    container_name: movie_integrator_cli
    volumes:
      - ./src:/app/src  # Dein Anwendungscode
      - ${MOVIE_SOURCE_DIR}:/source  # Quellverzeichnis aus der .env-Datei
      - ${MOVIE_TARGET_DIR}:/target  # Zielverzeichnis aus der .env-Datei
    env_file:
      - .env
    entrypoint: ["movie-integrator"]  # Setzt den Entry-Point auf den Typer-CLI-Befehl
    stdin_open: true
    tty: true

  original-media-integrator:
    build: .
    container_name: original_media_integrator_cli
    volumes:
      - ./src:/app/src  # Dein Anwendungscode
      - ${ORIGINAL_MEDIA_SOURCE_DIR}:/original_source  # Quellverzeichnis
      - ${ORIGINAL_MEDIA_TARGET_DIR}:/original_target  # Zielverzeichnis
    env_file:
      - .env
    entrypoint: ["original-media-integrator"]  # Setzt den Entry-Point auf den Typer-CLI-Befehl
    stdin_open: true
    tty: true
```

#### Aufruf:
- **Movie Integrator:**
  ```bash
  docker-compose run movie-integrator /source /target
  ```

- **Original Media Integrator:**
  ```bash
  docker-compose run original-media-integrator --source-dir /original_source --destination-dir /original_target
  ```

### 3.3 Variante 2: Gesamt-Service

In dieser Variante erstellst du einen **übergeordneten CLI**, der alle Sub-Commands (z.B. `movie-integrator` und `original-media-integrator`) als **Sub-Befehle** integriert. Dadurch gibt es nur einen einzigen Service.

#### Beispiel: Gesamt-CLI "kurmann-videoschnitt"

In deinem Python-Projekt kannst du eine Datei `kurmann_videoschnitt.py` erstellen, die alle CLI-Befehle zusammenführt:

```python
import typer
from movie_integrator.commands.integrate_to_library import integrate_to_library_command
from original_media_integrator.app import import_media

app = typer.Typer()

# Integriere die einzelnen CLI-Commands
app.command("movie-integrator")(integrate_to_library_command)
app.command("original-media-integrator")(import_media)

if __name__ == "__main__":
    app()
```

### Anpassung der Docker-Compose-Konfiguration:

```yaml
version: '3'
services:
  kurmann-videoschnitt:
    build: .
    container_name: kurmann_videoschnitt_cli
    volumes:
      - ./src:/app/src  # Dein Anwendungscode
      - ${MOVIE_SOURCE_DIR}:/source  # Quellverzeichnis aus der .env-Datei für movie-integrator
      - ${MOVIE_TARGET_DIR}:/target  # Zielverzeichnis aus der .env-Datei für movie-integrator
      - ${ORIGINAL_MEDIA_SOURCE_DIR}:/original_source  # Quellverzeichnis für original-media-integrator
      - ${ORIGINAL_MEDIA_TARGET_DIR}:/original_target  # Zielverzeichnis für original-media-integrator
    env_file:
      - .env
    entrypoint: ["kurmann-videoschnitt"]  # Setzt den Entry-Point auf den Gesamt-CLI-Befehl
    stdin_open: true
    tty: true
```

#### Aufruf:
- **Movie Integrator Command:**
  ```bash
  docker-compose run kurmann-videoschnitt movie-integrator /source /target
  ```

- **Original Media Integrator Command:**
  ```bash
  docker-compose run kurmann-videoschnitt original-media-integrator --source-dir /original_source --destination-dir /original_target
  ```

## 4. Entry-Point und CLI in Docker

### Wie funktioniert der Entry-Point?

Der Entry-Point in Docker definiert den Befehl, der automatisch ausgeführt wird, sobald der Container gestartet wird. Wenn du den Entry-Point auf deine Haupt-CLI-Anwendung setzt, brauchst du keine vollständigen Python-Skripte anzugeben, sondern kannst den Container wie eine normale CLI-Anwendung verwenden.

Beispiel:
- **Entry-Point für Gesamt-CLI:**
  ```yaml
  entrypoint: ["kurmann-videoschnitt"]
  ```

Wenn du den Container dann ausführst, wird automatisch der `kurmann-videoschnitt`-Befehl ausgeführt, und du kannst weitere Befehle und Argumente direkt anhängen.

Beispiel-Aufruf:
```bash
docker-compose run kurmann-videoschnitt original-media-integrator --source-dir /source --destination-dir /target
```

## 5. Vor- und Nachteile der Varianten

### Variante 1: Mehrere Services
- **Vorteile:**
  - Klare Trennung der einzelnen CLI-Anwendungen.
  - Jeder Service hat seinen eigenen Entry-Point und kann unabhängig verwendet werden.
- **Nachteile:**
  - Mehrere Services müssen gepflegt werden.
  - Bei vielen CLI-Befehlen kann die Docker-Compose-Datei schnell komplex werden.

### Variante 2: Gesamt-Service
- **Vorteile:**
  - Alle CLI-Befehle sind in einem einzigen Service integriert, was die Verwaltung vereinfacht.
  - Nur ein Entry-Point für alle Befehle, wodurch die Aufrufe konsistenter werden.
- **Nachteile:**
  - Größere Komplexität beim Erstellen und Warten der Gesamt-CLI.
  - Bei sehr vielen Sub-Commands kann es unübersichtlich werden.

## 6. Fazit

Beide Varianten haben ihre Vorteile und sind abhängig von der gewünschten Flexibilität und der Anzahl der CLI-Befehle. Variante 1 (mehrere Services) ist ideal, wenn du eine klare Trennung zwischen verschiedenen CLI-Befehlen behalten möchtest. Variante 2 (Gesamt-Service) bietet eine einfachere Verwaltung und Konsistenz, indem alle Befehle in einem Gesamt-CLI zusammengeführt werden.
