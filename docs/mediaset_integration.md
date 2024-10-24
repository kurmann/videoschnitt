# Spezifikation für die Medienset-Integration

## Zielsetzung

Das Ziel dieses Befehls ist es, ein Medienset-Verzeichnis zu validieren und in die zentrale Mediathek zu integrieren, gemäß der Kurmann-Medienset-Spezifikation (`kurmann_mediaset.md`). Der Prozess aktualisiert das Mediatheksdatum in der `Metadaten.yaml` auf das aktuelle Datum und gewährleistet die Konsistenz, indem die bestehende ULID übernommen und eine geeignete Versionierungsstrategie angewendet wird.

## Schlüsselpunkte

1. **Eingabeverzeichnisse**
   - **Medienset-Verzeichnis**: Das Verzeichnis, das das zu integrierende Medienset enthält.
   - **Mediathek-Verzeichnis**: Das Hauptverzeichnis der Mediathek, organisiert nach Jahresordnern.

2. **Validierung**
   - Der Befehl `mediaset-manager validate-mediaset` wird verwendet, um die Struktur und Metadaten des Mediensets gemäß der Spezifikation (`kurmann_mediaset.md`) zu überprüfen.

3. **Bestimmung des Zieljahres**
   - Das Jahr wird aus der `Metadaten.yaml` des Mediensets extrahiert.
   - Das Medienset wird in das entsprechende Jahresverzeichnis der Mediathek verschoben, z.B. `/Mediathek/2024/`.

4. **Versionierungslogik**
   - **Automatische Entscheidung**:
     - Wenn das bestehende Medienset vor mehr als 40 Tagen integriert wurde, wird eine neue Version erstellt.
     - Andernfalls wird das bestehende Medienset überschrieben.
   - **Manuelle Optionen**:
     - `--version overwrite`: Überschreibt das bestehende Medienset unabhängig vom Integrationsdatum.
     - `--version new`: Erstellt immer eine neue Version unabhängig vom Integrationsdatum.

5. **Integration des Mediensets**
   - **Überschreiben**:
     - Die bestehende ULID aus der Mediathek wird übernommen.
     - Das Mediatheksdatum wird auf das aktuelle Datum aktualisiert.
     - Vorhandene Dateien werden durch die neuen Dateien ersetzt, während nicht enthaltene Dateien bestehen bleiben.
   - **Neue Version erstellen**:
     - Das bestehende Medienset wird in ein Versionsverzeichnis (`Vorherige_Versionen/Version_X`) verschoben.
     - Das neue Medienset wird in das Jahresverzeichnis der Mediathek verschoben.
     - Eine neue ULID wird generiert, und das Mediatheksdatum wird aktualisiert.

6. **Benutzerinteraktion**
   - **Bestätigung**:
     - Standardmäßig wird der Benutzer einmalig um Bestätigung gebeten, bevor Dateien verschoben werden.
     - Die Bestätigungsabfrage kann mit der Option `--no-prompt` unterdrückt werden, um den Prozess zu automatisieren.

7. **Rückgabewerte**
   - **Erfolg**: Rückgabe eines Statuscodes 0 und einer Erfolgsmeldung.
   - **Fehler**: Rückgabe eines nicht-null Statuscodes und einer detaillierten Fehlermeldung.

## Algorithmusschritte

1. **Validierung des Mediensets**
   - Der Befehl `mediaset-manager validate-mediaset` wird auf das Medienset-Verzeichnis angewendet.
   - Wenn die Validierung fehlschlägt, wird der Prozess mit einer Fehlermeldung abgebrochen.

2. **Lesen der Metadaten**
   - Das Jahr (`Jahr`) und der Titel (`Titel`) werden aus der `Metadaten.yaml` extrahiert.

3. **Bestimmen des Ziel-Jahresverzeichnisses**
   - Das Jahresverzeichnis in der Mediathek wird erstellt, falls es noch nicht existiert, z.B. `/Mediathek/2024/`.

4. **Überprüfung auf bestehende Mediensets**
   - Es wird überprüft, ob ein Medienset mit dem gleichen Titel bereits im Ziel-Jahresverzeichnis vorhanden ist.

5. **Entscheidung über Versionierung**
   - **Manuelle Optionen**:
     - `overwrite`: Setzt die Versionierungsentscheidung auf Überschreiben.
     - `new`: Setzt die Versionierungsentscheidung auf Neue Version.
   - **Automatische Entscheidung**:
     - Das Mediatheksdatum des bestehenden Mediensets wird überprüft.
     - Liegt das Integrationsdatum vor mehr als 40 Tagen, wird eine neue Version erstellt; andernfalls wird überschrieben.

6. **Integration des Mediensets**
   - **Überschreiben**:
     - Die bestehende ULID wird übernommen.
     - Das Mediatheksdatum in der `Metadaten.yaml` wird aktualisiert.
     - Die neuen Dateien werden in das bestehende Verzeichnis kopiert, wobei vorhandene Dateien überschrieben werden und nicht enthaltene Dateien bestehen bleiben.
   - **Neue Version erstellen**:
     - Das bestehende Medienset wird in das Versionsverzeichnis (`Vorherige_Versionen/Version_X`) verschoben.
     - Das neue Medienset wird in das Jahresverzeichnis verschoben.
     - Eine neue ULID wird generiert, und das Mediatheksdatum wird aktualisiert.

7. **Benutzerbestätigung**
   - Falls nicht durch `--no-prompt` unterdrückt, wird der Benutzer zur Bestätigung aufgefordert.
   - Wird die Bestätigung abgelehnt, wird der Prozess abgebrochen.

8. **Rückgabe des Status**
   - Bei erfolgreicher Integration wird ein Erfolgscode und eine entsprechende Meldung zurückgegeben.
   - Bei Fehlern wird ein Fehlercode und eine detaillierte Fehlermeldung zurückgegeben.

## Beispielanwendung des Algorithmus

Angenommen, wir haben das folgende Medienset-Verzeichnis:

```
/Users/patrickkurmann/Movies/Compressed Media/2024_Leah_will_Krokodil_zeigen_Test/
├── Video-Internet-4K.m4v
├── Video-Internet-HD.m4v
├── Video-Internet-SD.m4v
├── Video-Medienserver.mov
├── Titelbild.png
└── Metadaten.yaml
```

**Schritte:**

1. **Validierung**
   - Führe `mediaset-manager validate-mediaset '/Users/patrickkurmann/Movies/Compressed Media/2024_Leah_will_Krokodil_zeigen_Test'` aus.
   - Bei erfolgreicher Validierung wird fortgefahren.

2. **Lesen der Metadaten**
   - Extrahiere Jahr: `2024` und Titel: `Leah will Krokodil zeigen (Test)`.

3. **Bestimmen des Ziel-Jahresverzeichnisses**
   - Erstelle `/Mediathek/2024/` falls nicht vorhanden.

4. **Überprüfung auf bestehende Mediensets**
   - Prüfe, ob `/Mediathek/2024/2024_Leah_will_Krokodil_zeigen_Test` existiert.

5. **Entscheidung über Versionierung**
   - **Beispiel 1**: Keine Option angegeben und das bestehende Medienset wurde vor weniger als 40 Tagen integriert → Überschreiben.
   - **Beispiel 2**: Keine Option angegeben und das bestehende Medienset wurde vor über 40 Tagen integriert → Neue Version erstellen.
   - **Beispiel 3**: Option `--version overwrite` angegeben → Überschreiben.
   - **Beispiel 4**: Option `--version new` angegeben → Neue Version erstellen.

6. **Integration**
   - **Überschreiben**:
     - Übernehme die ULID aus `/Mediathek/2024/2024_Leah_will_Krokodil_zeigen_Test/Metadaten.yaml`.
     - Aktualisiere das Mediatheksdatum.
     - Kopiere die neuen Dateien und überschreibe vorhandene Dateien.
   - **Neue Version erstellen**:
     - Verschiebe das bestehende Verzeichnis nach `/Mediathek/2024/Vorherige_Versionen/Version_1/`.
     - Verschiebe das neue Medienset nach `/Mediathek/2024/2024_Leah_will_Krokodil_zeigen_Test/`.
     - Generiere eine neue ULID und aktualisiere das Mediatheksdatum.

7. **Benutzerbestätigung**
   - Falls nicht durch `--no-prompt` unterdrückt, fordere den Benutzer zur Bestätigung auf.

8. **Rückgabe des Status**
   - **Bei Erfolg**: Statuscode 0 und Erfolgsmeldung.
   - **Bei Fehler**: Statuscode 1 und detaillierte Fehlermeldung.

## Vorteile dieses Ansatzes

- **Konsistenz**: Stellt sicher, dass alle Mediensets gemäß den festgelegten Standards validiert und korrekt integriert werden.
- **Flexibilität**: Manuelle Steuerung der Versionierungsstrategie durch Befehlsoptionen.
- **Automatisierung**: Möglichkeit zur automatisierten Integration durch Unterdrückung von Bestätigungsabfragen.
- **Datenintegrität**: Die Beibehaltung der ULID und konsistente Metadatenaktualisierung gewährleisten die Integrität der Mediathek.
- **Skalierbarkeit**: Durch die Integration eines Mediensets pro Aufruf kann die Verwaltung kontrolliert und skaliert werden.

## Implementierungsüberlegungen

- **Fehlerbehandlung**: Robuste Fehlerbehandlung für Probleme wie Dateizugriffsfehler, ungültige Metadaten und Versionskonflikte.
- **Benutzerfreundlichkeit**: Klare und verständliche Fehlermeldungen sowie die Option zur Unterdrückung von Bestätigungsabfragen verbessern die Benutzererfahrung.
- **Performance**: Effiziente Verarbeitung auch großer Mediensets durch optimierte Dateiverwaltung und Vermeidung unnötiger Operationen.
- **Sicherheit**: Nur autorisierte Prozesse dürfen Zugriff auf die Mediathek erhalten und Änderungen vornehmen.

## Schlussfolgerung

Der Befehl `mediaset-manager integrate-mediaset` bietet eine zuverlässige, konsistente und flexible Methode zur Integration von Mediensets in die zentrale Mediathek. Die Kombination von Validierung, automatischer Versionierung und nutzerfreundlicher Interaktion stellt sicher, dass die Mediathek stets aktuell und gut strukturiert bleibt. Diese Spezifikation gewährleistet, dass der gesamte Integrationsprozess den Anforderungen der Kurmann-Medienset-Spezifikation entspricht und eine optimale Verwaltung der Mediendateien ermöglicht.

## Zusammenfassung

- **Ziel**: Effiziente und konsistente Integration von Mediensets in die Mediathek.
- **Validierung**: Überprüfung der Medienset-Struktur und Metadaten gemäß `kurmann_mediaset.md`.
- **Versionierung**: Automatische oder manuelle Entscheidung, abhängig vom Integrationsdatum oder den angegebenen Optionen.
- **Rückgabewerte**: Klare Erfolgsmeldungen oder detaillierte Fehlermeldungen zur Unterstützung des weiteren Prozesses.
- **Sicherheit und Integrität**: Sicherstellung der Datenkonsistenz durch Beibehaltung der ULID und aktualisierte Metadaten.
