# Videoschnitt-Kraftwerk

## Überblick

Das Kurmann Videoschnitt-Kraftwerk ist eine leistungsstarke Anwendung, die auf der Kurmann.Videoschnitt.Engine aufbaut und sich auf die Automatisierung und Verwaltung von Videoschnittprozessen konzentriert. Diese Anwendung zielt darauf ab, die Effizienz und Produktivität zu steigern, indem verschiedene Aufgaben im Videoschnitt automatisiert und optimiert werden.

## Hauptfunktionen

Die folgenden Hauptfunktionen gelten als Ziele der Anwendung und sind derzeit in Entwicklung. Weitere Funktionen können hinzugefügt oder entfernt werden:

- **Verwaltung Originalmedien**: Organisation und Katalogisierung von Originalvideodateien.
- **Verwaltung Heimserver-Mediathek**: Verwaltung und Bereitstellung von Videos auf einem Heimserver.
- **Komprimierung**: Automatische Komprimierung von Videos zur Platzersparnis.
- **Metadaten-Synchronisation**: Synchronisation und Aktualisierung von Metadaten in den Videodateien.
- **Bereinigungsarbeiten**: Automatische Entfernung von unerwünschten Szenen und Dateibereinigung.

## Architektur

Videoschnitt-Kraftwerk verwendet die Kurmann.Videoschnitt.Engine als Kernmodul und erweitert diese durch spezialisierte Automatisierungsdienste. Diese Dienste sind als separate Module implementiert und kommunizieren über die zentrale Engine.

## Docker Setup

Das Docker-Image ist auf Docker Hub verfügbar. Um das Image zu erstellen und auszuführen, verwenden Sie die folgenden Befehle:

```shell
docker pull kurmann/videoschnitt-kraftwerk
docker run -d -p 8080:80 kurmann/videoschnitt-kraftwerk
```

Um das Image lokal zu erstellen und auszuführen, verwenden Sie diese Befehle:

```shell
docker build -t kurmann/videoschnitt-kraftwerk .
docker run -d -p 8080:80 kurmann/videoschnitt-kraftwerk
```

### Docker Compose

Ein Docker Compose-File wird angeboten, um die Anwendung noch einfacher zu starten. Speichern Sie das folgende Compose-File als `docker-compose.yml`:

```yaml
version: '3.8'

services:
  videoschnitt-kraftwerk:
    image: kurmann/videoschnitt-kraftwerk:latest
    container_name: videoschnitt-kraftwerk
    ports:
      - "8080:80"
    volumes:
      - /path/to/media/library:/app/media
      - /path/to/config:/app/config
    environment:
      - LocalMediaLibrary__LibraryPath=/app/media
      - LocalMediaLibrary__CacheSize=1024
```

Um die Anwendung mit Docker Compose zu starten, verwenden Sie den folgenden Befehl:

```shell
docker-compose up -d
```

## Konfiguration

Die Konfiguration kann über Umgebungsvariablen angepasst werden:

```dockerfile
ENV LocalMediaLibrary__LibraryPath="/path/to/media/library"
```

Weitere Konfigurationsoptionen können in der `appsettings.json` Datei oder über Umgebungsvariablen festgelegt werden.

## Installation

Um die Anwendung lokal zu installieren und zu starten, folgen Sie diesen Schritten:

1. Repository klonen:
    ```shell
    git clone https://github.com/kurmann/videoschnitt-kraftwerk.git
    ```
2. Abhängigkeiten installieren:
    ```shell
    dotnet restore
    ```
3. Anwendung starten:
    ```shell
    dotnet run
    ```

## Nutzung

Kurmann.Videoschnitt.Kraftwerk kann durch REST-APIs oder direkt über die Benutzeroberfläche genutzt werden. Die API-Dokumentation ist unter `/swagger` nach dem Start der Anwendung zu finden.

## Namensgebung

Das Repository wurde als "videoschnitt-kraftwerk" unter dem GitHub-Account "kurmann" erstellt. Diese Namenskonvention sorgt dafür, dass der Name des Docker-Images direkt aus dem Repository-Namen abgeleitet werden kann, was die Verwaltung und Nutzung des Images erleichtert. Innerhalb der Anwendung und bei der Erstellung von NuGet-Paketen wird die Punktnotation verwendet, wie es bei .NET-Projekten üblich ist, um eine klare Struktur und Namenskonvention zu gewährleisten.

## Mitwirken

1. **Issue einreichen**: Wenn ein Fehler gefunden wird oder eine Funktion angefordert werden soll, kann ein Issue im GitHub-Repository eröffnet werden.
2. **Pull Requests**: Bei Vorschlägen für direkte Änderungen oder Ergänzungen, kann ein Pull Request mit einer klaren Beschreibung der Änderungen eingereicht werden.

## Lizenz

Dieses Projekt ist unter der Apache-2.0-Lizenz lizenziert. Weitere Details sind in der Datei [LICENSE](LICENSE) im GitHub-Repository zu finden.

## Kontakt

Falls Fragen bestehen oder Unterstützung benötigt wird, kann ein Issue im GitHub-Repository eröffnet werden.
