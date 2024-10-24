
# Spezifikation für einen Algorithmus zur automatischen Identifikation von Mediensets

## Zielsetzung

Entwicklung eines Algorithmus, der in einem gegebenen Hauptverzeichnis nach Mediendateien sucht und basierend auf den Metadaten potenzielle Mediensets identifiziert. Der Algorithmus liefert eine Liste von Dateien, die als Metadatenquellen für die jeweiligen Gruppen (potenzielle Mediensets) dienen. Diese Dateien können dann einzeln an den bestehenden Medienset-Erstellungsalgorithmus übergeben werden. ProRes-Masterdateien werden angemessen behandelt, und der Benutzer wird einmalig zur Bestätigung des Verschiebens von Dateien aufgefordert, mit der Möglichkeit, diese Abfrage zu unterdrücken.

## Schlüsselpunkte

1. **Suchverzeichnis**:
   - Der Algorithmus durchsucht ein Hauptverzeichnis.
   - Eine Suche in einem zusätzlichen Verzeichnis ist nicht erforderlich, da der bestehende Medienset-Erstellungsalgorithmus (basierend auf der Metadatenquelle) bereits die Suche in einem zusätzlichen Verzeichnis unterstützt.

2. **Identifikation potenzieller Mediensets**:
   - Dateien werden anhand von Metadaten, nicht aufgrund von Dateinamenpräfixen oder -suffixen, gruppiert.
   - Das Titel-Feld in den Metadaten wird als primäres Kriterium für die Gruppierung verwendet.
   - Der Algorithmus extrahiert den Titel jeder Datei mithilfe von EXIFTool.

3. **Auswahl der Metadatenquellendateien**:
   - Für jede Gruppe wählt der Algorithmus eine Metadatenquelle aus, die zur Erstellung des Mediensets verwendet wird.
   - **Priorität bei der Auswahl der Metadatenquelle**:
     - Erste Priorität: `.mov`-Dateien (QuickTime), da sie in der Regel reichhaltigere Metadaten enthalten.
     - Falls keine `.mov`-Dateien verfügbar sind, wird die größte `.mp4`- oder `.m4v`-Datei (nach Dateigröße) der Gruppe ausgewählt.
   - ProRes-Masterdateien werden nicht als Metadatenquellen ausgewählt.

4. **Identifikation von ProRes-Masterdateien**:
   - ProRes-Masterdateien werden über das Metadatenfeld `Compressor Name` identifiziert, das den String `Apple ProRes` enthält.
     - Beispiel: `Compressor Name: Apple ProRes 422 HQ`
     - Verschiedene Varianten wie `Apple ProRes 422 LT`, `Apple ProRes 4444` usw. werden ebenfalls berücksichtigt.
   - ProRes-Masterdateien werden nicht in die Mediensets aufgenommen und separat behandelt.

5. **Behandlung von ProRes-Masterdateien**:
   - ProRes-Masterdateien werden nicht in die Medienset-Verzeichnisse verschoben.
   - Nach erfolgreicher Erstellung des zugehörigen Mediensets werden ProRes-Masterdateien in ein spezifisches Verzeichnis verschoben:
     - Standardmäßig: Masterdateien von integrierten Mediensets.
     - Alternativ kann ein vom Benutzer angegebenes Verzeichnis verwendet werden.
   - Wenn die Medienseterstellung fehlschlägt, verbleiben die ProRes-Masterdateien an ihrem ursprünglichen Ort.

6. **Benutzerinteraktion**:
   - Der Algorithmus fragt den Benutzer einmalig, ob er mit dem Verschieben aller Dateien einverstanden ist.
   - Diese Abfrage kann mit einem Kommandozeilen-Flag unterdrückt werden (z. B. `--no-prompt`).
   - Die Abfrage wird nicht für jede Gruppe wiederholt; sie gilt für den gesamten Prozess.

7. **Verarbeitungssequenz**:
   - Die Verarbeitung erfolgt sequenziell; jede Gruppe wird nacheinander verarbeitet.
   - Jede Gruppe kann individuell erfolgreich verarbeitet werden oder fehlschlagen, ohne die Verarbeitung der anderen Gruppen zu beeinflussen.

8. **Ausgabe des Algorithmus**:
   - Der Algorithmus gibt eine Liste von Metadatenquellendateien aus, eine pro Gruppe (potenzielles Medienset).
   - Diese Liste kann verwendet werden, um den bestehenden Medienset-Erstellungsalgorithmus für jede Datei aufzurufen.
   - Die Ausgabe kann als einfache Liste von Dateipfaden oder als Zuordnung von Gruppentiteln zu Dateipfaden präsentiert werden.

## Algorithmusschritte

1. **Sammeln aller Mediendateien**:
   - Durchsuche das Hauptverzeichnis nach Dateien mit unterstützten Erweiterungen:
     - Videodateien: `.mov`, `.mp4`, `.m4v`
     - Bilddateien: `.png`, `.jpg`, `.jpeg`
   - Schließe ProRes-Masterdateien aus, die über das Metadatenfeld `Compressor Name` mit dem Wert `Apple ProRes` identifiziert werden.

2. **Metadatenextraktion und Gruppierung**:
   - Extrahiere für jede gesammelte Datei (außer ProRes-Masterdateien) die relevanten Metadaten mithilfe von EXIFTool, insbesondere das Titel-Feld.
   - Gruppiere Dateien, die denselben Titel in den Metadaten haben.
   - Dateien mit demselben Titel werden als Teil desselben potenziellen Mediensets betrachtet.

3. **Auswahl der Metadatenquelle für jede Gruppe**:
   - Für jede Gruppe:
     - Prüfe auf `.mov`-Dateien (ohne ProRes-Masterdateien).
     - Wähle die erste gefundene `.mov`-Datei oder priorisiere nach bestimmten Kriterien (z. B. Dateigröße).
     - Falls keine `.mov`-Dateien vorhanden sind, wähle die größte `.mp4`- oder `.m4v`-Datei (nach Dateigröße) als Metadatenquelle.
     - Wenn keine geeignete Metadatenquelle gefunden wird, kann die Gruppe übersprungen oder der Benutzer informiert werden.

4. **Erstellung der Liste von Metadatenquellen**:
   - Erstelle eine Liste der ausgewählten Metadatenquellendateien, eine pro Gruppe.

5. **Benutzerbestätigung**:
   - Fordere den Benutzer einmalig auf, das Verschieben aller Dateien zu bestätigen:
     - Beispiel: “Sind Sie einverstanden, dass alle Dateien für die identifizierten Mediensets verschoben werden?”
     - Die Abfrage kann mit einem Flag unterdrückt werden (z. B. `--no-prompt`).

6. **Sequenzielle Verarbeitung jeder Gruppe**:
   - Für jede Gruppe:
     - Wenn die Benutzerbestätigung vorliegt (oder die Abfrage unterdrückt wurde), fahre fort.
     - Übergebe die ausgewählte Metadatenquelle an den bestehenden Medienset-Erstellungsalgorithmus.
     - Behandle Erfolg oder Fehlschlag individuell für jede Gruppe.

7. **Behandlung von ProRes-Masterdateien**:
   - Nach erfolgreicher Erstellung eines Mediensets für eine Gruppe:
     - Verschiebe zugehörige ProRes-Masterdateien in das spezifizierte Verzeichnis.
   - Bei Fehlschlag der Medienseterstellung:
     - Belasse die ProRes-Masterdateien am ursprünglichen Ort.

## Beispielanwendung des Algorithmus

Gegeben sind die folgenden Dateien im Hauptverzeichnis:

```
/Users/patrickkurmann/Movies/Compressed Media/ 
├── 2024-06-11 Geburtstagssinge Zwillinge - 1.m4v 
├── 2024-06-11 Geburtstagssinge Zwillinge - 2.mov 
├── 2024-06-11 Geburtstagssinge Zwillinge - 3.m4v 
├── 2024-06-11 Geburtstagssinge Zwillinge.jpg 
├── 2024-10-10 Leah will Krokodil zeigen (Test) 4K-Internet Fast mov.m4v 
├── 2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov 
├── 2024-10-10 Leah will Krokodil zeigen (Test) 540p-Internet mov.m4v 
├── 2024-10-10 Leah will Krokodil zeigen (Test) 1080p-Internet mov.m4v 
├── 2024-10-10 Leah will Krokodil zeigen (Test) Master ProRes.mov
```


**Schritte:**

1. **Sammeln der Mediendateien**:
   - Schließe `2024-10-10 Leah will Krokodil zeigen (Test) Master ProRes.mov` aus (identifiziert als ProRes-Masterdatei über `Compressor Name` mit `Apple ProRes`).
   - Sammle die restlichen Mediendateien.

2. **Metadatenextraktion und Gruppierung**:
   - Verwende EXIFTool, um das Titel-Feld jeder Datei zu extrahieren.
   - Gruppiere Dateien basierend auf dem Titel:
     - **Gruppe 1**: Titel `2024-06-11 Geburtstagssinge Zwillinge`
     - **Gruppe 2**: Titel `2024-10-10 Leah will Krokodil zeigen (Test)`

3. **Auswahl der Metadatenquellen**:
   - **Gruppe 1**:
     - Verfügbare `.mov`-Dateien: `2024-06-11 Geburtstagssinge Zwillinge - 2.mov`
     - Ausgewählte Metadatenquelle: `2024-06-11 Geburtstagssinge Zwillinge - 2.mov`
   - **Gruppe 2**:
     - Verfügbare `.mov`-Dateien:
       - `2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov`
     - Ausgewählte Metadatenquelle: `2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov`

4. **Erstellung der Liste von Metadatenquellen**:
   - Liste:
     - `/Users/patrickkurmann/Movies/Compressed Media/2024-06-11 Geburtstagssinge Zwillinge - 2.mov`
     - `/Users/patrickkurmann/Movies/Compressed Media/2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov`

5. **Benutzerbestätigung**:
   - Fordere den Benutzer einmalig auf:
     - “Sind Sie einverstanden, dass alle Dateien für die identifizierten Mediensets verschoben werden?”
     - Der Benutzer kann zustimmen oder ablehnen.
     - Die Abfrage kann mit `--no-prompt` unterdrückt werden.

6. **Sequenzielle Verarbeitung jeder Gruppe**:
   - **Gruppe 1**:
     - Übergebe `2024-06-11 Geburtstagssinge Zwillinge - 2.mov` an den Medienset-Erstellungsalgorithmus.
     - Verarbeite Erfolg oder Fehlschlag.
   - **Gruppe 2**:
     - Übergebe `2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov` an den Medienset-Erstellungsalgorithmus.
     - Verarbeite Erfolg oder Fehlschlag.

7. **Behandlung von ProRes-Masterdateien**:
   - **Gruppe 2**:
     - Nach erfolgreicher Medienseterstellung:
       - Verschiebe `2024-10-10 Leah will Krokodil zeigen (Test) Master ProRes.mov` in das Verzeichnis `Masterdateien von integrierten Mediensets`.
   - **Gruppe 1**:
     - Falls zutreffend, entsprechende Behandlung.

## Vorteile dieses Ansatzes

- **Klarheit der Ausgabe**:
  - Die Ausgabe ist eine eindeutige Liste von Metadatenquellen pro Gruppe, was die weitere Verarbeitung vereinfacht.
- **Effizienz**:
  - Durch die Suche in nur einem Verzeichnis wird der Algorithmus vereinfacht und effizienter.
- **Benutzerfreundlichkeit**:
  - Die einmalige Abfrage verbessert die Nutzererfahrung.
  - Die Möglichkeit, die Abfrage zu unterdrücken, bietet Flexibilität für automatisierte Prozesse.
- **Modularität**:
  - Der Algorithmus ist ein reiner Such- und Findungsalgorithmus, dessen Ergebnis nahtlos an den bestehenden Medienset-Erstellungsprozess übergeben werden kann.

## Implementierungsüberlegungen

- **Metadatenextraktion**:
  - Effiziente Nutzung von EXIFTool, ggf. Batch-Verarbeitung zur Reduzierung von Overhead.
- **Fehlerbehandlung**:
  - Robuste Behandlung von Fehlern bei der Metadatenextraktion oder beim Zugriff auf Dateien.
- **Identifikation von ProRes-Dateien**:
  - Sicherstellen, dass alle Varianten von ProRes-Codecs korrekt über das `Compressor Name`-Feld identifiziert werden.
- **Benutzerabfragen und Flags**:
  - Implementierung von Kommandozeilenoptionen zur Steuerung der Benutzerinteraktion (z. B. `--no-prompt`).
- **Integration mit bestehendem Algorithmus**:
  - Sicherstellen, dass die Ausgabe (Liste der Metadatenquellen) im richtigen Format für den bestehenden Medienset-Erstellungsalgorithmus vorliegt.

## Schlussfolgerung

Dieser aktualisierte Algorithmus identifiziert effizient potenzielle Mediensets, indem er Dateien anhand von Metadaten aus einem einzigen Verzeichnis gruppiert. Er liefert eine Liste von Metadatenquellen, die an den bestehenden Medienset-Erstellungsalgorithmus übergeben werden können, was eine nahtlose Integration und Verarbeitung ermöglicht. ProRes-Masterdateien werden angemessen behandelt, und die Benutzerinteraktion wird durch Minimierung von Abfragen benutzerfreundlich gestaltet.
