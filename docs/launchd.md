# launchd Konfigurationen

## Run when a device has been mounted: `StartOnMount`
Ein Job mit diesem Schlüssel, der auf `true` gesetzt ist, wird gestartet, sobald ein Gerät gemountet wird, d.h. wenn eine CD/DVD eingelegt, eine externe Festplatte angeschlossen oder ein virtuelles Dateisystem gemountet wird.

```xml
<key>StartOnMount</key>
<true/>
```
**Hinweis**: `launchd` meldet nicht, welches Gerät gemountet wurde.

## Run when a path has been modified: `WatchPaths`
Dieser Schlüssel nimmt eine Liste von Strings an, wobei jeder String einen Pfad zu einer Datei oder einem Verzeichnis darstellt.

```xml
<key>WatchPaths</key>
<array>
  <string>/path/to/directory_or_file</string>
</array>
```

- Wenn der Pfad zu einer Datei zeigt, startet das Erstellen, Entfernen und Schreiben in diese Datei den Job.
- Wenn der Pfad zu einem Verzeichnis zeigt, startet das Erstellen und Entfernen dieses Verzeichnisses sowie das Erstellen, Entfernen und Schreiben von Dateien in diesem Verzeichnis den Job.
- Aktionen, die in Unterverzeichnissen dieses Verzeichnisses ausgeführt werden, werden nicht erkannt.

**Hinweis**: `launchd` meldet nicht, welcher Pfad geändert wurde.

## Run when files are available for processing: `QueueDirectories`
Dieser Schlüssel nimmt eine Liste von Strings an, wobei jeder String ein Verzeichnis darstellt.

```xml
<key>QueueDirectories</key>
<array>
  <string>/path/to/directory</string>
</array>
```

- Sobald eines der angegebenen Verzeichnisse nicht leer ist, wird der Job gestartet.
- Es liegt in der Verantwortung des Jobs, jede verarbeitete Datei zu entfernen, andernfalls wird der Job nach Ablauf von `ThrottleInterval` Sekunden erneut gestartet.

**Hinweis**: `launchd` meldet nicht, in welchem Verzeichnis neue Dateien gefunden wurden oder welche Namen diese haben.
