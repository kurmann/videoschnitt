# Anleitung zur Konfiguration und Test der Log-Rotation auf macOS

Diese Anleitung beschreibt, wie du die Log-Rotation für spezifische Log-Dateien auf macOS einrichtest und testest, ohne zusätzliche Software zu installieren.

## Schritt 1: Erstellen der Konfigurationsdatei

1. Öffne das Terminal.
2. Erstelle eine neue Konfigurationsdatei im Verzeichnis `/etc/newsyslog.d`:
   ```sh
   sudo nano /etc/newsyslog.d/kurmann-videoschnitt.conf
   ```

3. Füge die folgenden Zeilen in die Datei ein und ersetze `yourmacosuser` durch deinen tatsächlichen macOS-Benutzernamen:
   ```plaintext
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.FinalCutPro.log          640  7     *    $D0   J
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.FinalCutPro.error.log    640  7     *    $D0   J
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.InfuseMediaLibrary.log   640  7     *    $D0   J
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.InfuseMediaLibrary.error.log 640  7 * $D0 J
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MediaSetIndex.log        640  7     *    $D0   J
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MediaSetIndex.error.log  640  7     *    $D0   J
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MetadataXml.log          640  7     *    $D0   J
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MetadataXml.error.log    640  7     *    $D0   J
   ```

4. Speichere die Datei und schliesse den Editor:
   - Drücke `Ctrl + O`, um die Datei zu speichern.
   - Drücke `Ctrl + X`, um den Editor zu schliessen.

## Schritt 2: Testen der Konfiguration

1. Um die Konfiguration zu testen, führe den folgenden Befehl im Terminal aus:
   ```sh
   sudo newsyslog -nv
   ```

   Dieser Befehl zeigt an, welche Log-Dateien rotiert werden würden, ohne tatsächlich Änderungen vorzunehmen. Ein typisches Ergebnis könnte so aussehen:
   ```plaintext
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.FinalCutPro.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.FinalCutPro.error.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.InfuseMediaLibrary.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.InfuseMediaLibrary.error.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MediaSetIndex.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MediaSetIndex.error.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MetadataXml.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   /Users/yourmacosuser/Library/Logs/kurmann-videoschnitt.MetadataXml.error.log <7J>: --> will trim at Sun Jul 21 00:00:00 2024
   ```

2. Um die Log-Rotation tatsächlich durchzuführen, entferne die `-n` Option und führe `newsyslog` aus:
   ```sh
   sudo newsyslog
   ```

## Erläuterung der Konfigurationsfelder

- **Pfad zur Log-Datei:** Der vollständige Pfad zur Log-Datei.
- **Modus:** Die Zugriffsrechte der neuen Log-Datei (z.B. 640).
- **Zahl der Backups:** Wie viele alte Log-Dateien aufbewahrt werden sollen (z.B. 7).
- **Größe oder Zeitplan:** Wann die Log-Dateien rotiert werden sollen (z.B. `*` für keine Größenbeschränkung, `$D0` für tägliche Rotation).
- **Optionen:** Zusätzliche Optionen wie `J` für Komprimierung der Log-Dateien.

## Hinweis zum Bearbeiten bestehender Konfigurationen

Wenn du bestehende Konfigurationsdateien bearbeitest und Zeilen löschen musst, kannst du in `nano` die Tastenkombination `Ctrl + K` verwenden, um die aktuelle Zeile zu löschen. Dies kann hilfreich sein, um alte oder fehlerhafte Konfigurationen zu entfernen.

Diese Schritte sollten dir helfen, die Log-Rotation für deine spezifischen Log-Dateien unter macOS einzurichten und zu testen. Falls du noch weitere Anpassungen benötigst oder Fragen hast, stehe ich dir gerne zur Verfügung!
