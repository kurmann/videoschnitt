# Protokollierung und Log-Management

Dieses Kapitel erklärt, wie die Protokollierung und das Log-Management in dieser Anwendung funktioniert. Es beschreibt, wie Logs von den verschiedenen Prozessen nachträglich eingesehen werden können, wie `launchd` konfiguriert ist, um die Python-Skripte über `tmux` zu starten, und wie die Log-Rotation implementiert wird.

## Übersicht

Die Anwendung nutzt `tmux` zur Protokollierung und Aggregation von Log-Ausgaben. Dabei können die Konsolenausgaben aller Python-Skripte direkt in einer `tmux`-Sitzung gespeichert und später eingesehen werden. Es wird mit einfachen `printf`-Statements gearbeitet, sodass keine spezielle Logging-Infrastruktur benötigt wird.

## Protokollierung mit tmux

Die Python-Skripte werden über `tmux` gestartet, was bedeutet, dass alle Konsolenausgaben in der jeweiligen `tmux`-Sitzung gespeichert werden. Auch wenn ein Skript beendet ist, bleiben die Logs in `tmux` verfügbar und können nachträglich eingesehen werden.

### Nachträgliches Einsehen der Logs

Um die Logs eines bestimmten Prozesses nachträglich einzusehen, gehe wie folgt vor:

1. Liste alle laufenden oder beendeten `tmux`-Sitzungen auf:

    ```bash
    tmux ls
    ```

2. Verbinde dich mit der Sitzung, um die Logs einzusehen:

    ```bash
    tmux attach -t <session_name>
    ```

3. Scrolle durch die Sitzung, um ältere Ausgaben anzuzeigen:

    - Drücke `Ctrl + b`, gefolgt von `[` und nutze die Pfeiltasten zum Scrollen.
    - Drücke `q`, um den Scroll-Modus zu verlassen.

Auch wenn das Skript bereits abgeschlossen ist, bleiben die Ausgaben in `tmux` verfügbar, bis die Sitzung manuell geschlossen wird.

### Starten der Python-Skripte über launchd und tmux

Um sicherzustellen, dass die Python-Skripte in einer `tmux`-Sitzung laufen und ihre Ausgaben gespeichert werden, startet `launchd` die Skripte wie folgt:

```xml
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.deinbenutzername.kurmannprocess</string>

    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/bin/tmux</string>
        <string>new-session</string>
        <string>-d</string>
        <string>-s</string>
        <string>kurmannprocess</string>
        <string>python3</string>
        <string>/Pfad/zu/deinem_script.py</string>
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

- `tmux new-session -d -s <session_name>` erstellt eine neue `tmux`-Sitzung im Hintergrund, in der das Python-Skript ausgeführt wird.
- Die Python-Skripte verwenden `printf` oder `print`, um Ausgaben direkt ins Terminal zu schreiben, die von `tmux` aufgezeichnet werden.

### Verwendung von printf statt Logging-Modul

In dieser Anwendung arbeiten alle Python-Skripte nur mit einfachen `print`- oder `printf`-Befehlen für die Konsolenausgabe. Es ist keine spezielle Logging-Bibliothek erforderlich. Alle Ausgaben, die über `print` oder `printf` generiert werden, werden direkt in der `tmux`-Sitzung protokolliert und können nachträglich eingesehen werden.

### Log-Rotation und Archivierung mit launchd

Um sicherzustellen, dass die Log-Dateien nicht zu groß werden, wird eine Log-Rotation über ein Skript und `launchd` durchgeführt. Die Logs werden täglich archiviert und komprimiert. Hierzu wird das folgende Setup verwendet:

#### Shell-Skript zur Log-Rotation

```bash
#!/bin/bash

LOG_DIR=~/Library/Logs
LOG_FILE=$LOG_DIR/kurmann-videoschnitt.log
ARCHIVE_DIR=$LOG_DIR/archive

# Erstelle das Archiv-Verzeichnis, falls es nicht existiert
mkdir -p $ARCHIVE_DIR

# Verschiebe die aktuelle Log-Datei und komprimiere sie
if [ -f $LOG_FILE ]; then
    TIMESTAMP=$(date +"%Y-%m-%d")
    mv $LOG_FILE $ARCHIVE_DIR/kurmann-videoschnitt-$TIMESTAMP.log
    gzip $ARCHIVE_DIR/kurmann-videoschnitt-$TIMESTAMP.log
fi

# Erstelle eine neue leere Log-Datei
touch $LOG_FILE
```

Dieses Skript wird täglich um Mitternacht ausgeführt und stellt sicher, dass die Log-Dateien archiviert und komprimiert werden.

#### Konfiguration von launchd für die Log-Rotation

Das obenstehende Shell-Skript wird über `launchd` gesteuert:

```xml
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.deinbenutzername.kurmannvideoschnitt.logrotate</string>

    <key>ProgramArguments</key>
    <array>
        <string>/Pfad/zu/deinem_logrotate_script.sh</string>
    </array>

    <key>StartCalendarInterval</key>
    <dict>
        <key>Hour</key>
        <integer>0</integer>
        <key>Minute</key>
        <integer>0</integer>
    </dict>

    <key>RunAtLoad</key>
    <true/>
</dict>
</plist>
```

Speichere die `.plist`-Datei unter `~/Library/LaunchAgents/com.deinbenutzername.kurmannvideoschnitt.logrotate.plist` und lade sie mit:

```bash
launchctl load ~/Library/LaunchAgents/com.deinbenutzername.kurmannvideoschnitt.logrotate.plist
```

Jetzt wird die Log-Rotation täglich um Mitternacht durchgeführt.

### Weitere Aspekte des Log-Managements

- **tmux als Aggregator:** Tmux speichert die Ausgaben aller Skripte in den jeweiligen Sitzungen und ermöglicht dir, die Logs jederzeit nachträglich einzusehen.
- **Sitzung wieder verbinden:** Selbst wenn die Sitzung abgetrennt wurde, kannst du dich wieder verbinden, um alte Ausgaben zu überprüfen.
- **Flexibilität und Einfachheit:** Mit tmux kannst du die Protokollierung flexibel gestalten, ohne eine komplexe Logging-Infrastruktur einrichten zu müssen.

Dieses Setup bietet dir eine robuste und einfache Lösung für die Protokollierung, die sowohl bei automatisierten Prozessen als auch bei manuell gestarteten Skripten zuverlässig funktioniert.
