# Kurmann Videoschnitt

## Überblick

**Kurmann Videoschnitt** ist eine leistungsstarke CLI-Anwendung für macOS, die sich auf die Automatisierung und Verwaltung von Videoschnittprozessen konzentriert. Sie zielt darauf ab, die Effizienz und Produktivität zu steigern, indem verschiedene Aufgaben im Videoschnitt automatisiert und optimiert werden.

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
2. **Python 3.8 oder neuer**: Stelle sicher, dass Python auf deinem System installiert ist. Du kannst die Installation überprüfen mit:

   ```bash
   python3 --version
   ```

### Installation mit `pip` und `venv`

1. **Repository klonen**:

   ```bash
   git clone https://github.com/kurmann/videoschnitt.git
   ```

2. **In das Projektverzeichnis wechseln**:

   ```bash
   cd videoschnitt
   ```

3. **Virtuelle Umgebung erstellen**:

   ```bash
   python3 -m venv venv
   ```

4. **Virtuelle Umgebung aktivieren**:

   ```bash
   source venv/bin/activate
   ```

5. **Abhängigkeiten installieren**:

   ```bash
   pip install -r requirements.txt
   ```

6. **Anwendung installieren**:

   ```bash
   pip install .
   ```

7. **Anwendung ausführen**:

   ```bash
   kurmann-videoschnitt --help
   ```

## CLI

Die Grundlage für die CLI ist das [CLI-Design](/docs/CLI-Design.md).

### CLI-Dokumentation

- [Kurmann Videoschnitt CLI](/docs/cli/kurmann_videoschnitt.md)

## Mitwirken

1. **Issue einreichen**: Bei Fehlern oder Funktionswünschen können GitHub-Issues eröffnet werden.
2. **Pull Requests**: Vorschläge für Änderungen oder Verbesserungen können per Pull Request eingereicht werden.

## Lizenz

Dieses Projekt steht unter der Apache-2.0-Lizenz. Details sind in der Datei [LICENSE](LICENSE) im Repository zu finden.

## Kontakt

Für Fragen oder Unterstützung kann ein Issue im GitHub-Repository erstellt werden.