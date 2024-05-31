# Kurmann Videoschnitt

## Überblick

Kurmann Videoschnitt ist eine leistungsstarke Anwendung, die sich auf die Automatisierung und Verwaltung von Videoschnittprozessen konzentriert. Diese Anwendung zielt darauf ab, die Effizienz und Produktivität zu steigern, indem verschiedene Aufgaben im Videoschnitt automatisiert und optimiert werden.

## Hauptfunktionen

Die folgenden Hauptfunktionen gelten als Ziele der Anwendung und sind derzeit in Entwicklung. Weitere Funktionen können hinzugefügt oder entfernt werden:

- **Verwaltung Originalmedien**: Organisation und Katalogisierung von Originalvideodateien.
- **Verwaltung Heimserver-Mediathek**: Verwaltung und Bereitstellung von Videos auf einem Heimserver.
- **Komprimierung**: Automatische Komprimierung von Videos zur Platzersparnis.
- **Metadaten-Synchronisation**: Synchronisation und Aktualisierung von Metadaten in den Videodateien.
- **Bereinigungsarbeiten**: Automatische Entfernung von unerwünschten Szenen und Dateibereinigung.

## Architektur

Videoschnitt verwendet die Kurmann.Videoschnitt.Engine als Kernmodul und erweitert diese durch spezialisierte Automatisierungsdienste. Diese Dienste sind als separate Module implementiert und kommunizieren über die zentrale Engine.

## Docker Setup

Das Docker-Image ist auf Docker Hub verfügbar. Um das Image zu erstellen und auszuführen, verwenden Sie die folgenden Befehle:

```shell
docker pull kurmann/videoschnitt
docker run -d kurmann/videoschnitt
```

Um das Image lokal zu erstellen und auszuführen, verwenden Sie diese Befehle:

```shell
docker build -t kurmann/videoschnitt .
docker run -d kurmann/videoschnitt
```

### Docker Compose

Ein Docker Compose-File wird angeboten, um die Anwendung noch einfacher zu starten. Speichern Sie das folgende Compose-File als `docker-compose.yml`:

```yaml
services:
  videoschnitt:
    image: kurmann/videoschnitt:latest
    container_name: videoschnitt
    volumes:
      - logs:/app/logs
    environment:
      - ASPNETCORE_ENVIRONMENT=Production

volumes:
  logs:
```

Um die Anwendung mit Docker Compose zu starten, verwenden Sie den folgenden Befehl:

```shell
docker-compose up -d
```

### Überwachung

Die Überwachung der Anwendung erfolgt vorzugsweise über Docker-Logs:

```shell
docker logs -f videoschnitt
```

### Konfiguration des Repositorys: Verwendung von Volumes und Bind Mounts auf Docker-Ebene und Host-System

#### Überblick

Kurmann Videoschnitt nutzt Docker, um eine konsistente und isolierte Umgebung für die Ausführung der Anwendung bereitzustellen. In diesem Abschnitt wird erläutert, wie Sie Volumes und Bind Mounts konfigurieren können, um lokale Verzeichnisse auf Ihrem Host-System, wie z.B. einem Synology NAS, in den Docker-Container einzubinden. Diese Mechanismen ermöglichen es, externe Verzeichnisse in den Container zu mappen, sodass Ihre .NET-Anwendung darauf zugreifen und diese überwachen kann.

#### Unterschiede zwischen Volumes und Bind Mounts

- **Volumes**:
  - Werden von Docker verwaltet.
  - Sind unabhängig vom Host-Dateisystem.
  - Bieten Vorteile bei der Portabilität und Verwaltung durch Docker.
  
- **Bind Mounts**:
  - Binden spezifische Verzeichnisse des Host-Dateisystems in den Container ein.
  - Sind direkt an das Host-Dateisystem gebunden.
  - Bieten mehr Flexibilität bei der Nutzung bestehender Host-Verzeichnisse.

#### Konfiguration mit Docker Compose

Die folgende `docker-compose.yml` Datei zeigt, wie Volumes und Bind Mounts verwendet werden können, um Verzeichnisse vom Host-System in den Docker-Container zu integrieren.

```yaml
version: '3.8'

services:
  videoschnitt:
    image: kurmann/videoschnitt:latest
    container_name: videoschnitt
    volumes:
      - /path/to/media/library:/app/media         # Bind Mount
      - videoschnitt-config:/app/config           # Docker Volume
    environment:
      - LocalMediaLibrary__LibraryPath=/app/media
```

#### Docker Run-Befehl Beispiel

Alternativ können Volume-Mounts und Bind Mounts direkt beim Starten des Containers mit dem Docker Run-Befehl festgelegt werden:

```bash
docker run -d \
  -v /path/to/media/library:/app/media \
  -v videoschnitt-config:/app/config \
  -e LocalMediaLibrary__LibraryPath=/app/media \
  --name videoschnitt \
  kurmann/videoschnitt:latest
```

### Konfigurationsmechanismus

#### Umgebungsvariablen

Die .NET-Anwendung im Container kann über Umgebungsvariablen konfiguriert werden. Diese Variablen können im Docker Compose-File oder direkt im Docker Run-Befehl gesetzt werden. Beispiel:

```yaml
environment:
  - LocalMediaLibrary__LibraryPath=/app/media
```

```bash
-e LocalMediaLibrary__LibraryPath=/app/media
```

Diese Variablen werden von der Anwendung ausgelesen, um Pfade und andere Konfigurationen zu definieren.

#### Unterstützte Konfigurationswerte

Aktuell wird nur der `Library Path` als Konfigurationswert unterstützt. Die Liste der unterstützten Konfigurationswerte wird noch erweitert.

### Pfadzuweisungen

#### Pfad innerhalb des Containers

Bei der Zuweisung eines Verzeichnisses, das von der Anwendung überwacht werden soll, muss der Pfad angegeben werden, wie er innerhalb des Containers sichtbar ist. Beispiel:

- Lokaler Pfad auf dem Host: `/path/to/media/library`
- Gemounteter Pfad im Container: `/app/media`

Die Umgebungsvariable würde dann auf den Pfad innerhalb des Containers gesetzt werden:

```yaml
environment:
  - LocalMediaLibrary__LibraryPath=/app/media
```

### Implementierung in der .NET-Anwendung

Die `Kurmann.Videoschnitt.Engine` und andere direkt oder indirekt integrierte Module müssen zwingend im Benutzerverzeichnis arbeiten. Dies hat folgende Gründe und Vorteile:

1. **Sicherheitsgründe**: Das Docker-Image läuft als Nicht-Root-Benutzer, um Sicherheitsrisiken zu minimieren. Der Zugriff auf Systemverzeichnisse ist daher eingeschränkt.
2. **Verzeichnisverwaltung**: Die Anwendung kann flexibel im Benutzerverzeichnis arbeiten und benötigte Unterverzeichnisse selbst erstellen und verwalten.
3. **Einfache Konfiguration**: Durch das direkte Mapping der Host-Verzeichnisse auf das Benutzerverzeichnis im Container wird die Konfiguration vereinfacht und ist klar nachvollziehbar.

Hier ist ein Beispiel, wie Ihre .NET-Anwendung den `LibraryPath` aus der Umgebungsvariablen lesen und verwenden kann:

```csharp
string libraryPath = Environment.GetEnvironmentVariable("LocalMediaLibrary__LibraryPath");

if (!string.IsNullOrEmpty(libraryPath))
{
    string fullLibraryPath = Path.Combine(libraryPath, "myFile.txt");
    File.WriteAllLines(fullLibraryPath, myText);
}
else
{
    // Fallback, falls die Umgebungsvariable nicht gesetzt ist
    string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "library", "myFile.txt");
    File.WriteAllLines(defaultPath, myText);
}
```

### Zusammenfassung

- **Volumes** und **Bind Mounts** ermöglichen es, Verzeichnisse vom Host-System in den Docker-Container zu integrieren.
- **Volumes** werden von Docker verwaltet und bieten Portabilität.
- **Bind Mounts** binden spezifische Verzeichnisse des Host-Dateisystems in den Container ein und bieten Flexibilität.
- Umgebungsvariablen können im Docker Compose-File oder im Docker Run-Befehl gesetzt werden, um Pfade und andere Einstellungen festzulegen.
- Pfade müssen immer so angegeben werden, wie sie innerhalb des Containers sichtbar sind.
- **Sicherheitsgründe**: Das Docker-Image läuft als Nicht-Root-Benutzer, um Sicherheitsrisiken zu minimieren. Der Zugriff auf Systemverzeichnisse ist daher eingeschränkt.
- **Verzeichnisverwaltung**: Die Anwendung kann flexibel im Benutzerverzeichnis arbeiten und benötigte Unterverzeichnisse selbst erstellen und verwalten.
- **Einfache Konfiguration**: Durch das direkte Mapping der Host-Verzeichnisse auf das Benutzerverzeichnis im Container wird die Konfiguration vereinfacht und ist klar nachvollziehbar.

Durch die Nutzung dieser Konfigurationsmechanismen wird sichergestellt, dass die Anwendung auf die notwendigen Verzeichnisse zugreifen und korrekt funktionieren kann, unabhängig davon, ob sie in einem Docker-Container oder auf einem Host-System wie einem Synology NAS läuft.

## Installation

Um die Anwendung lokal zu installieren und zu starten, folgen Sie diesen Schritten:

1. Repository klonen:
    ```shell
    git clone https://github.com/kurmann/videoschnitt.git
    ```
2. Abhängigkeiten installieren:
    ```shell
    dotnet restore
    ```
3. Anwendung starten:
    ```shell
    dotnet run
    ```

## Namensgebung

Das Repository wurde als “videoschnitt” unter dem GitHub-Account “kurmann” erstellt. Diese Namenskonvention sorgt dafür, dass der Name des Docker-Images direkt aus dem Repository-Namen abgeleitet werden kann, was die Verwaltung und Nutzung des Images erleichtert. Innerhalb der Anwendung und bei der Erstellung von NuGet-Paketen wird die Punktnotation verwendet, wie es bei .NET-Projekten üblich ist, um eine klare Struktur und Namenskonvention zu gewährleisten.

## Mitwirken

1. **Issue einreichen**: Wenn ein Fehler gefunden wird oder eine Funktion angefordert werden soll, kann ein Issue im GitHub-Repository eröffnet werden.
2. **Pull Requests**: Bei Vorschlägen für direkte Änderungen oder Ergänzungen, kann ein Pull Request mit einer klaren Beschreibung der Änderungen eingereicht werden.

## Lizenz

Dieses Projekt ist unter der Apache-2.0-Lizenz lizenziert. Weitere Details sind in der Datei [LICENSE](LICENSE) im GitHub-Repository zu finden.

## Kontakt

Falls Fragen bestehen oder Unterstützung benötigt wird, kann ein Issue im GitHub-Repository eröffnet werden.
