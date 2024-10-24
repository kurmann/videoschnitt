# Spezifikation zur Erstellung eines Mediensets ausgehend von einer Videodatei

## 1. Zielsetzung

Entwicklung eines Algorithmus, der basierend auf einer gegebenen Videodatei ein vollständiges Medienset erstellt. Das Medienset besteht aus zusammengehörigen Mediendateien, die anhand bestimmter Kriterien identifiziert und zugewiesen werden. Zusätzlich werden Metadaten aus der Videodatei extrahiert und in einer `metadaten.yaml` gespeichert.

## 2. Ausgangslage

- **Referenzdatei**: Ein Pfad zu einer Videodatei, die als Referenz dient.
- **Zusätzliches Verzeichnis (optional)**: Ein weiteres Verzeichnis, in dem nach zusammengehörigen Dateien gesucht werden kann.

## 3. Definition eines Mediensets

Ein Medienset ist eine Gruppe von zusammenhängenden Mediendateien, die auf einem gemeinsamen Titel basieren.

## 4. Ermittlung des Titels

- Der Titel wird aus den Metadaten der Referenzdatei gelesen.
- Mögliche EXIFTool-Felder für den Titel:
  - **Title**
  - **DisplayName**
  - **Name**
- Falls der Titel im Format `YYYY-MM-DD Titel` vorliegt, wird das Datum extrahiert und als Aufnahmedatum verwendet.

## 5. Zusammenstellen des Mediensets

- Im selben Verzeichnis wie die Referenzdatei (Verzeichnis 1) werden alle Dateien gesucht, deren Dateinamen mit dem ermittelten vollständigen Titel beginnen.
- Optional kann in einem zusätzlichen Verzeichnis nach weiteren passenden Dateien gesucht werden.
- **Vollständiger Titel**: Der Titel inklusive Datum, wie er aus den Metadaten extrahiert wurde.

## 6. Umgang mit mehreren Mediensets in einem Verzeichnis

- Da es in einem Verzeichnis mehrere Mediensets geben kann, ist die Angabe der Referenzdatei notwendig, um das gewünschte Medienset eindeutig zu identifizieren.
- Nur Dateien, die mit dem vollständigen Titel der Referenzdatei beginnen, werden dem Medienset zugeordnet.

## 7. Zuweisung der Mediendateien zu Typen

Die gefundenen Dateien werden basierend auf bestimmten Kriterien den folgenden Typen zugewiesen:

### 7.1 Titelbild

- **Priorität 1**: Die erste PNG-Datei, deren Dateiname mit dem vollständigen Titel beginnt.
- **Priorität 2**: Wenn keine passende PNG-Datei gefunden wird, wird die erste JPG/JPEG-Datei mit passendem Dateinamen verwendet.
- **Suchreihenfolge**:
  - Zuerst in Verzeichnis 1.
  - Dann im zusätzlichen Verzeichnis (falls angegeben).

### 7.2 Videodateien

Videodateien werden anhand ihrer Auflösung und Bitrate wie folgt klassifiziert:

#### 7.2.1 Medienserver-Datei

- **Kriterien**:
  - Bitrate über 50 Mbps (gewöhnlich zwischen 80 und 100 Mbps).
  - Kann jede der unten genannten Auflösungen haben (SD, HD, 4K).
- **Anmerkung**:
  - Medienserver-Dateien sind spezielle Dateien mit hoher Bitrate, die für Internet-Streaming ungeeignet sind.

#### 7.2.2 Internet-Dateien

Diese Dateien können für Internet-Streaming verwendet werden und werden in folgende Unterkategorien eingeteilt:

##### 7.2.2.1 SD (Standard Definition)

- **Dateiformate**: MP4, M4V, MOV
- **Kriterien**:
  - Vertikale Auflösung ≤ 540 Pixel.

##### 7.2.2.2 HD (High Definition)

- **Dateiformate**: MP4, M4V, MOV
- **Kriterien**:
  - Vertikale Auflösung = 1080 Pixel.

##### 7.2.2.3 4K (Ultra High Definition)

- **Dateiformate**: MP4, M4V, MOV
- **Kriterien**:
  - Vertikale Auflösung ≥ 2048 Pixel.

## 8. Priorisierung bei der Klassifizierung

- **Bitrate als Differenzierungsmerkmal**:
  - Wenn nur eine 4K-Datei vorhanden ist, wird anhand der Bitrate unterschieden, ob es sich um die Medienserver-Datei oder die Internet-Version handelt.
  - Dateien mit Bitraten über 50 Mbps werden der Medienserver-Kategorie zugeordnet.
  - Dateien mit Bitraten bis zu 50 Mbps werden den Internet-Dateien zugeordnet.

## 9. Erstellung des Medienset-Verzeichnisses

- Der Verzeichnisname wird nach dem Muster `{Jahr}_{Titel}` gebildet.
- Der Titel wird dabei dateisystemkonform formatiert (z.B. durch Ersetzen von Leerzeichen durch Unterstriche).

## 10. Verschieben und Umbenennen der Dateien

- Die klassifizierten Dateien werden in das Medienset-Verzeichnis verschoben.
- Sie werden gemäß den erwarteten Dateinamen umbenannt:
  - **Medienserver-Datei**: `Video-Medienserver.mov`
  - **4K-Internet-Datei**: `Video-Internet-4K.m4v`
  - **HD-Internet-Datei**: `Video-Internet-HD.m4v`
  - **SD-Internet-Datei**: `Video-Internet-SD.m4v`
  - **Titelbild**: `Titelbild.png` oder `Titelbild.jpg`

## 11. Erstellung der `metadaten.yaml`

### 11.1 Allgemeine Struktur

Die `metadaten.yaml` enthält alle relevanten Metadaten des Mediensets. Diese werden entweder aus den Metadaten der Referenzdatei (mittels EXIFTool) extrahiert oder durch Benutzerangaben überschrieben.

### 11.2 Mapping der EXIFTool-Felder zu `metadaten.yaml`

| Metadaten.yaml Feld        | Quelle (EXIFTool-Feld)       | Verarbeitung                               |
|---------------------------|-----------------------------|-------------------------------------------|
| Spezifikationsversion     | -                           | Fester Wert: ‘1.0’                       |
| Id                        | -                           | Generierte ULID                           |
| Titel                     | Title, DisplayName, Name    | Wie in EXIFTool, ggf. überschrieben       |
| Typ                       | -                           | Fester Wert: ‘Familienfilm’              |
| Untertyp                  | AppleProappsShareCategory   | Standard: ‘Ereignis’, Großschreibung      |
| Jahr                      | Aus Aufnahmedatum oder Datum| Jahr extrahiert, ggf. aus Änderungsdatum  |
| Version                   | -                           | Fester Wert: 1                            |
| Aufnahmedatum             | Aus Titel oder Metadaten    | Datum im Format ‘YYYY-MM-DD’              |
| Zeitraum                  | -                           | Nur bei Untertyp ‘Rückblick’             |
| Beschreibung              | Description                 | Text, ggf. überschrieben                  |
| Notiz                     | -                           | Nur aus Benutzerangabe                    |
| Schlüsselwörter           | Genre                       | Getrennt durch Komma oder Semikolon       |
| Album                     | Album                       | Text, ggf. überschrieben                  |
| Videoschnitt              | Producer                    | Getrennt durch Komma oder Semikolon       |
| Kameraführung             | Director                    | Getrennt durch Komma oder Semikolon       |
| Dauer_in_Sekunden         | Duration                    | Umgerechnet in Sekunden                   |
| Studio                    | Studio                      | Text, ggf. überschrieben                  |
| Filmfassung_Name          | -                           | Nur aus Benutzerangabe                    |
| Filmfassung_Beschreibung  | -                           | Nur aus Benutzerangabe                    |

### 11.3 Details zur Verarbeitung

- **Titel**:
  - Wird aus den EXIFTool-Feldern Title, DisplayName oder Name extrahiert.
  - Kann durch Benutzerangabe überschrieben werden.
- **Untertyp**:
  - Wird aus AppleProappsShareCategory extrahiert.
  - Standardwert: ‘Ereignis’.
  - Großschreibung des ersten Buchstabens.
- **Aufnahmedatum**:
  - Wird aus dem Titel extrahiert, wenn dieser im Format `YYYY-MM-DD Titel` vorliegt.
  - Kann aus EXIFTool-Feldern wie CreateDate oder ContentCreateDate stammen.
  - Falls nicht vorhanden, wird das Änderungsdatum der Datei verwendet.

### 11.4 Verarbeitung mehrerer Personenfelder

- **Videoschnitt (Director)** und **Kameraführung (Producer)**:
  - Wenn die EXIFTool-Felder mehrere Namen enthalten, werden sie anhand von Komma oder Semikolon getrennt und in Listen umgewandelt.
  - Beispiel: "Patrick Kurmann; Kathrin Glück" wird zu `["Patrick Kurmann", "Kathrin Glück"]`.

### 11.5 Beispielhafte `metadaten.yaml`

```yaml
Spezifikationsversion: '1.0'
Id: 01JAZC4E1E147SPCKPG8834Y9D
Titel: Leah will Krokodil zeigen (Test)
Typ: Familienfilm
Untertyp: Ereignis
Jahr: '2024'
Version: 1
Aufnahmedatum: '2024-10-10'
Beschreibung: Leah will mir das Krokodil ("Schildkröte") auf meinem grünen Pullover zeigen (Test-Video).
Schlüsselwörter:
  - Familienfilm
Album: Familie Kurmann-Glück
Videoschnitt:
  - Patrick Kurmann
  - Kathrin Glück
Kameraführung:
  - Patrick Kurmann
Dauer_in_Sekunden: 19
```

## 12. Besonderheiten

### 12.1 Fehlende Metadaten

- Falls bestimmte Metadaten nicht in den EXIFTool-Feldern vorhanden sind, können sie durch Benutzerangaben ergänzt werden.
- Sind weder EXIFTool-Felder noch Benutzerangaben vorhanden, werden die entsprechenden Felder ausgelassen oder es wird ein Standardwert verwendet.

### 12.2 Priorisierung von Benutzerangaben

- Benutzerangaben haben Vorrang vor den aus den Metadaten extrahierten Werten.
- Dies ermöglicht es, Metadaten manuell zu überschreiben oder zu ergänzen.

### 12.3 Standardwerte

- **Typ**: Immer 'Familienfilm'.
- **Untertyp**: Standardmäßig 'Ereignis', falls nicht anders angegeben.
- **Version**: Immer 1.

## 13. Ablauf des Algorithmus

1. **Metadatenextraktion**:
   - Extrahiere relevante Metadaten aus der Referenzdatei mittels EXIFTool.
2. **Titelermittlung**:
   - Bestimme den Titel und ggf. das Aufnahmedatum aus dem Titel.
3. **Jahrermittlung**:
   - Bestimme das Jahr aus dem Aufnahmedatum oder den Metadaten.
4. **Dateisuche**:
   - Suche nach passenden Dateien in Verzeichnis 1 und optional im zusätzlichen Verzeichnis.
5. **Dateiklassifizierung**:
   - Klassifiziere die gefundenen Dateien gemäß den Kriterien.
6. **Metadatenzusammenstellung**:
   - Erstelle die `metadaten.yaml` unter Berücksichtigung von Metadaten und Benutzerangaben.
7. **Verzeichnis- und Dateimanagement**:
   - Erstelle das Medienset-Verzeichnis.
   - Verschiebe und benenne die Dateien gemäß den Vorgaben um.
8. **Abschluss**:
   - Bestätige die erfolgreiche Erstellung des Mediensets.

## Zusammenfassung

Die aktualisierte Spezifikation beschreibt detailliert den Prozess zur Erstellung eines Mediensets ausgehend von einer Videodatei. Sie legt fest, wie Dateien identifiziert, klassifiziert und verarbeitet werden. Zudem wird genau beschrieben, wie Metadaten aus den EXIFTool-Feldern extrahiert und in der `metadaten.yaml` gespeichert werden, einschließlich der Zuordnung der einzelnen Felder und der Verarbeitung von Mehrfachwerten.
