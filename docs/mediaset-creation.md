# Spezifikation zur Erstellung eines Mediensets ausgehend von einer Videodatei

## 1. Zielsetzung

Entwicklung eines Algorithmus, der basierend auf einer gegebenen Videodatei ein vollständiges Medienset erstellt. Das Medienset besteht aus zusammengehörigen Mediendateien, die anhand bestimmter Kriterien identifiziert und zugewiesen werden.

## 2. Ausgangslage

- **Referenzdatei**: Ein Pfad zu einer Videodatei, die als Referenz dient.
- **Zusätzliches Verzeichnis (optional)**: Ein weiteres Verzeichnis, in dem nach zusammengehörigen Dateien gesucht wird.

## 3. Definition eines Mediensets

Ein Medienset ist eine Gruppe von zusammenhängenden Mediendateien, die auf einem gemeinsamen Titel basieren.

## 4. Ermittlung des Titels

- Der Titel wird aus den Metadaten der Referenzdatei gelesen.
- Die Referenzdatei dient als Quelle für die Metadaten und bestimmt den Titel, nach dem gesucht wird.

## 5. Zusammenstellen des Mediensets

- Im selben Verzeichnis wie die Referenzdatei (Verzeichnis 1) werden alle Dateien gesucht, deren Dateinamen mit dem ermittelten Titel beginnen.
- Optional kann in einem zusätzlichen Verzeichnis nach weiteren passenden Dateien gesucht werden.

## 6. Umgang mit mehreren Mediensets in einem Verzeichnis

- Da es in einem Verzeichnis mehrere Mediensets geben kann, ist die Angabe der Referenzdatei notwendig, um das gewünschte Medienset eindeutig zu identifizieren.
- Nur Dateien, die mit dem Titel der Referenzdatei beginnen, werden dem Medienset zugeordnet.

## 7. Zuweisung der Mediendateien zu Typen

Die gefundenen Dateien werden basierend auf bestimmten Kriterien den folgenden Typen zugewiesen:

### 7.1 Titelbild

- **Priorität 1**: Die erste PNG-Datei, deren Dateiname mit dem Titel beginnt.
- **Priorität 2**: Wenn keine passende PNG-Datei gefunden wird, wird die erste JPG/JPEG-Datei mit passendem Dateinamen verwendet.
- **Suchreihenfolge**:
  - Zuerst im zusätzlichen Verzeichnis (falls angegeben).
  - Dann in Verzeichnis 1.

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
- **Kriterien**: Maximale vertikale Auflösung von 540 Pixeln.

##### 7.2.2.2 HD (High Definition)

- **Dateiformate**: MP4, M4V, MOV
- **Kriterien**: Vertikale Auflösung von genau 1080 Pixeln.

##### 7.2.2.3 4K (Ultra High Definition)

- **Dateiformate**: MP4, M4V, MOV
- **Kriterien**: Minimale vertikale Auflösung von 2048 Pixeln.
- **Anmerkung**: Gemäß Final Cut Pro gilt eine Auflösung von mindestens 4096 x 2048 Pixeln als 4K.

## 8. Priorisierung bei der Klassifizierung

- **Bitrate als Differenzierungsmerkmal**:
  - Wenn nur eine 4K-Datei vorhanden ist, wird anhand der Bitrate unterschieden, ob es sich um die Medienserver-Datei oder die Internet-Version handelt.
  - Dateien mit Bitraten über 50 Mbps werden der Medienserver-Kategorie zugeordnet.
  - Dateien mit Bitraten bis zu 50 Mbps werden den Internet-Dateien zugeordnet.

## 9. Algorithmusablauf

1. **Ermittlung des Titels**:
   - Extrahiere den Titel aus den Metadaten der Referenzdatei.
2. **Sammlung passender Dateien**:
   - Durchsuche Verzeichnis 1 und das optionale zusätzliche Verzeichnis nach Dateien, deren Dateinamen mit dem ermittelten Titel beginnen.
3. **Zuweisung von Dateien**:
   - **Titelbild**:
     - Suche nach einer passenden PNG-Datei im zusätzlichen Verzeichnis.
     - Falls nicht vorhanden, suche nach einer JPG/JPEG-Datei.
     - Wenn im zusätzlichen Verzeichnis nichts gefunden wird, wiederhole die Suche in Verzeichnis 1.
   - **Videodateien**:
     - Für jede gefundene Videodatei:
       - Bestimme das Dateiformat (MP4, M4V, MOV).
       - Ermittle die Auflösung und Bitrate der Datei.
       - Klassifiziere die Datei entsprechend den Kriterien für Medienserver-Datei oder Internet-Dateien (SD, HD, 4K).
4. **Handhabung mehrerer Versionen**:
   - Wenn mehrere Dateien in einer Kategorie gefunden werden, priorisiere nach Bitrate oder anderen relevanten Merkmalen.

## 10. Beispielhafte Anwendung

Gegebene Referenzdatei:

`/Users/patrickkurmann/Movies/Tests/2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov`

### 10.1 Ermittlung des Titels

- **Titel aus Metadaten**: "2024-10-10 Leah will Krokodil zeigen (Test)"
- **Verzeichnis 1**: `/Users/patrickkurmann/Movies/Tests/`
- **Zusätzliches Verzeichnis**: `/Users/patrickkurmann/Movies/Test-Export/`

### 10.2 Sammlung passender Dateien

In Verzeichnis 1 gefunden:

- **Videodateien**:
  - `/Users/patrickkurmann/Movies/Tests/2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov`
  - `/Users/patrickkurmann/Movies/Tests/2024-10-10 Leah will Krokodil zeigen (Test) 4K-Internet Fast mov.m4v`
  - `/Users/patrickkurmann/Movies/Tests/2024-10-10 Leah will Krokodil zeigen (Test) 540p mov.m4v`
  - `/Users/patrickkurmann/Movies/Tests/2024-10-10 Leah will Krokodil zeigen (Test) 1080p-Internet mov.m4v`

In zusätzlichem Verzeichnis gefunden:

- **Titelbild**:
  - `/Users/patrickkurmann/Movies/Test-Export/2024-10-10 Leah will Krokodil zeigen (Test).png`

### 10.3 Zuweisung von Dateien

- **Medienserver-Datei**:
  - Datei: `2024-10-10 Leah will Krokodil zeigen (Test) 4K60-Medienserver mov.mov`
  - Kriterien erfüllt: Bitrate > 50 Mbps, Auflösung: 4K (z.B. 3840 x 2160 Pixel)
- **Internet-Dateien (für Streaming geeignet)**:
  - **4K-Version**: Datei: `2024-10-10 Leah will Krokodil zeigen (Test) 4K-Internet Fast mov.m4v`
  - **HD-Version**: Datei: `2024-10-10 Leah will Krokodil zeigen (Test) 1080p-Internet mov.m4v`
  - **SD-Version**: Datei: `2024-10-10 Leah will Krokodil zeigen (Test) 540p mov.m4v`
- **Titelbild**: Datei: `/Users/patrickkurmann/Movies/Test-Export/2024-10-10 Leah will Krokodil zeigen (Test).png`

### 10.4 Besonderheiten

- **SD, HD und 4K** gehören zur Obergruppe “Internet-Dateien” und sind für Internet-Streaming geeignet.
- **Mehrere Mediensets im Verzeichnis**: Andere Dateien im Verzeichnis, z.B. `/Users/patrickkurmann/Movies/Tests/2024-06-11 Geburtstagssinge Zwillinge für Grossbabi-4K-Internet.m4v` werden nicht dem aktuellen Medienset zugeordnet, da sie einen anderen Titel haben.

## 11. Besonderheiten

### 11.1 Mehrere Mediensets in einem Verzeichnis

- **Eindeutige Identifikation**: Durch den Titel aus den Metadaten der Referenzdatei.
- **Isolierung von Mediensets**: Nur Dateien, deren Dateinamen mit dem ermittelten Titel beginnen, werden berücksichtigt.

### 11.2 Suche in zusätzlichen Verzeichnissen

- **Erweiterte Suche**: Das zusätzliche Verzeichnis wird verwendet, um weitere Mediendateien zu finden, die zum Medienset gehören, insbesondere Titelbilder.
- **Suchreihenfolge**: Zuerst im zusätzlichen Verzeichnis, dann in Verzeichnis 1.

## 12. Zusammenfassung

Der Algorithmus ermöglicht es, ausgehend von einer einzelnen Videodatei ein vollständiges Medienset zusammenzustellen, indem er anhand des Titels zusammengehörige Dateien identifiziert und sie entsprechend ihrer Eigenschaften klassifiziert. Dabei werden sowohl Auflösung als auch Bitrate berücksichtigt, um die Dateien den richtigen Kategorien zuzuweisen. SD, HD und 4K Dateien werden als Internet-Dateien zusammengefasst, da sie für Internet-Streaming geeignet sind. Die Medienserver-Datei wird anhand ihrer hohen Bitrate identifiziert und separat behandelt.

