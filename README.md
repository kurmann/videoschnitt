# Kurmann Videoschnitt

## Überblick

**Kurmann Videoschnitt** ist eine leistungsstarke CLI-Anwendung für MacOS, die sich auf die Automatisierung und Verwaltung von Videoschnittprozessen konzentriert. Sie zielt darauf ab, die Effizienz und Produktivität zu steigern, indem verschiedene Aufgaben im Videoschnitt automatisiert und optimiert werden.

## Hauptfunktionen

Die Anwendung bietet eine Reihe von Funktionen, die speziell für den Videoschnitt und die Medienverwaltung entwickelt wurden:

- **Verwaltung Originalmedien**: Organisiert und katalogisiert Originalvideos.
- **Automatische Komprimierung**: Reduziert die Dateigröße von Videos und spart Speicherplatz, ohne signifikanten Qualitätsverlust.
- **Metadaten-Synchronisation**: Aktualisiert und pflegt Videometadaten.
- **Bereinigungsfunktionen**: Automatische Entfernung doppelter oder unnötiger Dateien.
- **Medienbereitstellung**: Unterstützt verschiedene Bereitstellungsvarianten, wie lokale Medienbibliotheken und Cloud-Streaming.

## Installation

### Voraussetzungen

1. **macOS**: Die Anwendung ist für macOS optimiert.
2. **Python 3.12 oder neuer**: Stelle sicher, dass Python auf deinem System installiert ist.
3. **Poetry**: Die Abhängigkeitsverwaltung erfolgt über Poetry. Installiere es, falls noch nicht geschehen:

   ```bash
   curl -sSL https://install.python-poetry.org | python3 -
   ```

### Installation mit Poetry

1. Repository klonen:

   ```bash
   git clone https://github.com/kurmann/videoschnitt.git
   ```

2. In das Projektverzeichnis wechseln:

   ```bash
   cd videoschnitt
   ```

3. Abhängigkeiten installieren:

   ```bash
   poetry install
   ```

4. Virtuelle Umgebung aktivieren:

   ```bash
   poetry shell
   ```

5. Anwendung ausführen:

   ```bash
   kurmann-videoschnitt --help
   ```

## CLI-Dokumentation
### Haupt-CLI

```
Usage: kurmann-videoschnitt [OPTIONS] COMMAND [ARGS]...

  Kurmann Videoschnitt - Zentrale CLI

Options:
  --help  Show this message and exit.

Commands:
  compressor  Apple Compressor Manager CLI
  emby        FileManager CLI für Emby Integrator
  integrator  Original Media Integrator CLI

```
### Apple Compressor Manager CLI

```
Usage: kurmann-videoschnitt compressor [OPTIONS] COMMAND [ARGS]...

  Apple Compressor Manager CLI

Options:
  --help  Show this message and exit.

Commands:
  cleanup-prores         Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant.
  compress-prores-file   Komprimiert eine einzelne ProRes-Datei.
  compress-prores-files  Komprimiert ProRes-Dateien in einem Verzeichnis.

```
### Original Media Integrator CLI

```
Usage: kurmann-videoschnitt integrator [OPTIONS] COMMAND [ARGS]...

  Original Media Integrator CLI

Options:
  --help  Show this message and exit.

Commands:
  import-media    Importiert neue Medien, führt die Kompression durch und...
  organize-media  Organisiert Medien nach Datum.

```
### Emby Integrator CLI

```
Usage: kurmann-videoschnitt emby [OPTIONS] COMMAND [ARGS]...

  FileManager CLI für Emby Integrator

Options:
  --help  Show this message and exit.

Commands:
  compress-masterfile         Komprimiere eine Master-Datei.
  convert-image-to-adobe-rgb  Konvertiere ein Bild in das Adobe...
  get-images-for-artwork      Rufe geeignete Bilder für Artwork aus einem...
  get-mediaserver-files       Rufe die Mediaserver-Dateien aus einem...

```

### Apple Compressor Manager CLI

```bash
kurmann-videoschnitt compressor [OPTIONS] COMMAND [ARGS]...
```

**Befehle:**
- `cleanup-prores` - Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant.
- `compress-prores-file` - Komprimiert eine einzelne ProRes-Datei.
- `compress-prores-files` - Komprimiert ProRes-Dateien in einem Verzeichnis.

### Original Media Integrator CLI

```bash
kurmann-videoschnitt integrator [OPTIONS] COMMAND [ARGS]...
```

**Befehle:**
- `import-media` - Importiert neue Medien und führt Kompression durch.
- `organize-media` - Organisiert Medien nach Datum.

### Emby Integrator CLI

```bash
kurmann-videoschnitt emby [OPTIONS] COMMAND [ARGS]...
```

**Befehle:**
- `compress-masterfile` - Komprimiert eine Master-Datei.
- `convert-image-to-adobe-rgb` - Konvertiert ein Bild in Adobe RGB.
- `get-images-for-artwork` - Extrahiert Bilder zur Verwendung als Artwork.
- `get-mediaserver-files` - Ruft die Dateien vom Mediaserver ab.

## Einsatzzwecke und Bereitstellungsvarianten

Die Anwendung unterstützt verschiedene Einsatzzwecke für lokale und cloudbasierte Medienbereitstellung:

1. **Lokaler Medienserver**: Kompatibel mit Infuse Media Player.
2. **Cloud-Streaming**: Nutzung von Azure Blob Storage mit ULID-basierten Verzeichnisnamen.

## Mitwirken

1. **Issue einreichen**: Bei Fehlern oder Funktionswünschen können GitHub-Issues eröffnet werden.
2. **Pull Requests**: Vorschläge für Änderungen oder Verbesserungen können per Pull Request eingereicht werden.

## Lizenz

Dieses Projekt steht unter der Apache-2.0-Lizenz. Details sind in der Datei [LICENSE](LICENSE) im Repository zu finden.

## Kontakt

Für Fragen oder Unterstützung kann ein Issue im GitHub-Repository erstellt werden.