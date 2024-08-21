# Laufzeitarchitektur und Entscheidung für launchd

Dieses Kapitel beschreibt die Laufzeitarchitektur der Anwendung und erläutert, warum wir uns für `launchd` als Hauptkomponente zur Automatisierung und Steuerung entschieden haben. Es wird auch erklärt, wie `launchd` in verschiedenen Szenarien, wie dem Überwachen von Verzeichnissen und dem Anschließen von Festplatten, verwendet wird.

## Überblick über die Laufzeitarchitektur

Die Laufzeitarchitektur der Anwendung basiert auf einer Kombination aus `launchd` und `tmux`. Während `launchd` für die zeitgesteuerte Ausführung, das Triggern von Prozessen bei bestimmten Ereignissen und das Überwachen von Verzeichnissen zuständig ist, übernimmt `tmux` die Protokollierung und ermöglicht das nachträgliche Einsehen von Logs.

### Warum haben wir uns für launchd entschieden?

Zu Beginn haben wir auch das Python-Tool `watchdog` in Betracht gezogen, das Änderungen im Dateisystem überwacht und darauf basierend Ereignisse auslöst. `watchdog` funktioniert gut für viele Anwendungsfälle, stößt jedoch bei bestimmten Szenarien auf macOS auf Einschränkungen:

- **Probleme mit gemounteten Netzwerklaufwerken:** `watchdog` hat Schwierigkeiten, zuverlässig Änderungen auf gemounteten Laufwerken (z.B. NAS) zu überwachen. Insbesondere bei intermittierenden Verbindungen oder temporären Netzwerkausfällen wird die Überwachung unzuverlässig.
- **Docker-Integration:** Bei einer zukünftigen Migration der Anwendung in Docker-Container (z.B. auf einem Synology NAS) könnte es zu weiteren Komplikationen kommen, da `watchdog` innerhalb von Containern nicht ideal mit gemounteten Verzeichnissen zusammenarbeitet.

**Fazit:** Um eine zuverlässige und flexible Lösung zu implementieren, die sowohl auf lokale Verzeichnisse als auch auf gemountete Laufwerke angewendet werden kann, haben wir uns für `launchd` entschieden. `launchd` ist nativ in macOS integriert und bietet eine leistungsfähige und zuverlässige Lösung für zeit- und ereignisgesteuerte Aufgaben.

## Verwendung von launchd für verschiedene Szenarien

### 1. **Zeitgesteuerte Prozesse:**

Für Aufgaben, die regelmäßig zu festen Zeiten ausgeführt werden müssen (z.B. nächtliche Housekeeping-Aufgaben oder das Hochladen von Videos auf einen FTP-Server), verwenden wir `launchd` mit der Konfiguration von `StartCalendarInterval`. 

**Beispiel-Konfiguration:**

```xml
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.deinbenutzername.housekeeping</string>

    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/tmux</string>
        <string>new-session</string>
        <string>-d</string>
        <string>-s</string>
        <string>housekeeping</string>
        <string>python3</string>
        <string>/Pfad/zu/housekeeping.py</string>
    </array>

    <key>StartCalendarInterval</key>
    <dict>
        <key>Hour</key>
        <integer>2</integer>
        <key>Minute</key>
        <integer>0</integer>
    </dict>

    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>
```

Diese Konfiguration startet das Python-Skript jeden Tag um 2:00 Uhr morgens in einer tmux-Sitzung, wodurch die Protokollierung und Überwachung der Ausgaben sichergestellt wird.

### 2. **Überwachung von Verzeichnissen:**

Ein häufiger Anwendungsfall ist die Überwachung von Verzeichnissen, um auf Änderungen zu reagieren. Das kann nützlich sein, wenn neue Dateien in einem Verzeichnis abgelegt werden und automatisch verarbeitet werden sollen.

**Beispiel-Konfiguration zur Überwachung eines Verzeichnisses:**

```xml
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.deinbenutzername.verzeichnisueberwachung</string>

    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/tmux</string>
        <string>new-session</string>
        <string>-d</string>
        <string>-s</string>
        <string>verzeichnisueberwachung</string>
        <string>python3</string>
        <string>/Pfad/zu/deinem_verzeichnis_skript.py</string>
    </array>

    <key>WatchPaths</key>
    <array>
        <string>/Pfad/zu/dem/verzeichnis</string>
    </array>

    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>
```

- **WatchPaths:** Diese Option überwacht das angegebene Verzeichnis und startet das Skript, sobald eine Änderung im Verzeichnis erkannt wird (z.B. eine neue Datei, ein neues Verzeichnis oder eine Änderung einer Datei).

### 3. **Überwachung von angeschlossenen Festplatten:**

Wenn eine externe Festplatte angeschlossen wird, kann dies ein Trigger sein, um automatisch bestimmte Prozesse zu starten. `launchd` bietet mit der Option `StartOnMount` eine einfache Möglichkeit, Skripte auszulösen, sobald ein neues Laufwerk gemountet wird.

**Beispiel-Konfiguration zur Überwachung eines gemounteten Laufwerks:**

```xml
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.deinbenutzername.festplattenueberwachung</string>

    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/tmux</string>
        <string>new-session</string>
        <string>-d</string>
        <string>-s</string>
        <string>festplattenueberwachung</string>
        <string>python3</string>
        <string>/Pfad/zu/deinem_festplatten_skript.py</string>
    </array>

    <key>StartOnMount</key>
    <true/>
</dict>
</plist>
```

- **StartOnMount:** Diese Option löst das Skript aus, sobald eine Festplatte oder ein externes Laufwerk angeschlossen wird.

### 4. **Protokollierung und Log-Rotation:**

Die Protokollierung erfolgt über `tmux`, das die Konsolenausgaben speichert und es ermöglicht, die Logs auch nach dem Abschluss der Prozesse einzusehen. Um sicherzustellen, dass die Log-Dateien nicht zu groß werden, haben wir eine Log-Rotation über ein Shell-Skript und `launchd` implementiert.

Das Shell-Skript archiviert und komprimiert die Logs täglich, während eine neue Log-Datei für den nächsten Tag erstellt wird.

**Details zur Log-Rotation sind im Kapitel [Protokollierung und Log-Management](#Protokollierung-und-Log-Management) beschrieben.**

## Zusammenfassung

Mit der Kombination aus `launchd` und `tmux` haben wir eine flexible und zuverlässige Laufzeitarchitektur implementiert. `launchd` bietet native Unterstützung für zeit- und ereignisgesteuerte Prozesse, während `tmux` die Protokollierung und Überwachung übernimmt. Diese Architektur ist robust und deckt die folgenden Szenarien ab:

- Zeitgesteuerte Aufgaben (z.B. Housekeeping)
- Überwachung von Verzeichnissen (z.B. neue Mediendateien)
- Überwachung von gemounteten Laufwerken (z.B. externe Festplatten)

Diese Architektur stellt sicher, dass alle Prozesse automatisiert und zuverlässig ausgeführt werden, während gleichzeitig eine einfache Überwachung und Protokollierung gewährleistet ist.
