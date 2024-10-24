# Spezifikation zur Erstellung eines Mediensets ausgehend von einer Videodatei

## 1. Zielsetzung

Entwicklung eines Algorithmus, der basierend auf einer gegebenen Videodatei ein vollständiges Medienset erstellt. Das Medienset besteht aus zusammengehörigen Mediendateien, die anhand bestimmter Kriterien identifiziert und zugewiesen werden.

## 2. Ausgangslage

- **Referenzdatei**: Ein Pfad zu einer Videodatei, die als Referenz dient.
- **Zusätzliches Verzeichnis (optional)**: Ein weiteres Verzeichnis, in dem nach zusammengehörigen Dateien gesucht werden kann.

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
  - Maximale vertikale Auflösung von 540 Pixeln.

##### 7.2.2.2 HD (High Definition)

- **Dateiformate**: MP4, M4V, MOV
- **Kriterien**:
  - Vertikale Auflösung von genau 1080 Pixeln.

##### 7.2.2.3 4K (Ultra High Definition)

- **Dateiformate**: MP4, M4V, MOV
- **Kriterien**:
  - Minimale vertikale Auflösung von 2048 Pixeln.
  - Gemäß Final Cut Pro gilt eine Auflösung von mindestens 4096 x 2048 Pixeln als 4K.

## 8. Priorisierung bei der Klassifizierung

- **Bitrate als Differenzierungsmerkmal**:
  - Wenn nur eine 4K-Datei vorhanden ist, wird anhand der Bitrate unterschieden, ob es sich um die Medienserver-Datei oder die Internet-Version handelt.
  - Dateien mit Bitraten über 50 Mbps werden der Medienserver-Kategorie zugeordnet.
  - Dateien mit Bitraten bis zu 50 Mbps werden den Internet-Dateien zugeordnet.

## 9. Algorithmusablauf

1. **Ermittlung des Titels**:
   - Extrahiere den Titel aus den Metadaten der Referenzdatei.
2. **Sammlung passender Dateien**:
   - Durchsuche Verzeichnis 1 und optional das zusätzliche Verzeichnis nach Dateien, deren Dateinamen mit dem ermittelten Titel beginnen.
3. **Zuweisung von Dateien**:
   - **Titelbild**:
     - Suche nach einer passenden PNG-Datei in Verzeichnis 1.
     - Falls nicht vorhanden, suche nach einer JPG/JPEG-Datei.
     - Wenn in Verzeichnis 1 nichts gefunden wird, wiederhole die Suche im zusätzlichen Verzeichnis.
   - **Videodateien**:
     - Für jede gefundene Videodatei:
       - Bestimme das Dateiformat (MP4, M4V, MOV).
       - Ermittle die Auflösung und Bitrate der Datei.
       - Klassifiziere die Datei entsprechend den Kriterien für Medienserver-Datei oder Internet-Dateien (SD, HD, 4K).
4. **Handhabung mehrerer Versionen**:
   - Wenn mehrere Dateien in einer Kategorie gefunden werden, priorisiere nach Bitrate oder anderen relevanten Merkmalen.

## 10. Beispielhafte Anwendung

### 10.1 Ermittlung des Titels

- **Gegebene Referenzdatei**:  
  `/Users/patrickkurmann/Movies/Compressed Media/2024-10-10 Leah will Krokodil zeigen (Test) - 1.mov`
- **Titel aus Metadaten**:  
  `2024-10-10 Leah will Krokodil zeigen (Test)`
- **Verzeichnis 1**:  
  `/Users/patrickkurmann/Movies/Compressed Media/`
- **Zusätzliches Verzeichnis**:  
  `/Users/patrickkurmann/Movies/Final Cut Export/`

### 10.2 Sammlung passender Dateien

**In Verzeichnis 1 gefunden**:

- **Videodateien**:
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-10-10 Leah will Krokodil zeigen (Test) - 1.mov`
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-10-10 Leah will Krokodil zeigen (Test) - 2.m4v`
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-10-10 Leah will Krokodil zeigen (Test) - 3.m4v`
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-10-10 Leah will Krokodil zeigen (Test) - 4.m4v`
- **Titelbild**:
  - Falls vorhanden: `/Users/patrickkurmann/Movies/Compressed Media/2024-10-10 Leah will Krokodil zeigen (Test).png` oder `.jpg`

**In zusätzlichem Verzeichnis gefunden**:

- **Titelbild**:
  - `/Users/patrickkurmann/Movies/Final Cut Export/2024-10-10 Leah will Krokodil zeigen (Test).png`

### 10.3 Zuweisung von Dateien

#### 10.3.1 Titelbild

- **Suchreihenfolge**:
  - **Zuerst in Verzeichnis 1**: Keine passende Datei gefunden (angenommen, dass dort kein Titelbild vorhanden ist).
  - **Dann im zusätzlichen Verzeichnis**:  
    Datei gefunden: `/Users/patrickkurmann/Movies/Final Cut Export/2024-10-10 Leah will Krokodil zeigen (Test).png`
  - **Kriterien erfüllt**:
    - Dateiname beginnt mit dem Titel.
    - Dateiformat: PNG.

#### 10.3.2 Videodateien

- **Medienserver-Datei**:
  - **Datei**: `2024-10-10 Leah will Krokodil zeigen (Test) - 1.mov`
  - **Kriterien erfüllt**:
    - Bitrate: >50 Mbps (angenommen zwischen 80 und 100 Mbps)
    - Auflösung: Kann 4K oder eine andere sein.
- **Internet-Dateien (für Streaming geeignet)**:
  - **4K-Version**:
    - **Datei**: `2024-10-10 Leah will Krokodil zeigen (Test) - 2.m4v`
    - **Kriterien erfüllt**:
      - Auflösung: ≥2048 Pixel vertikal (angenommen 3840 x 2160 Pixel)
      - Bitrate: ≤50 Mbps
  - **HD-Version**:
    - **Datei**: `2024-10-10 Leah will Krokodil zeigen (Test) - 3.m4v`
    - **Kriterien erfüllt**:
      - Auflösung: 1080 Pixel vertikal
  - **SD-Version**:
    - **Datei**: `2024-10-10 Leah will Krokodil zeigen (Test) - 4.m4v`
    - **Kriterien erfüllt**:
      - Auflösung: ≤540 Pixel vertikal

#### 10.3.3 Andere Dateien im Verzeichnis

- **Nicht zum aktuellen Medienset gehörig**:
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-06-11 Geburtstagssinge Zwillinge - 1.m4v`
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-06-11 Geburtstagssinge Zwillinge - 2.mov`
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-06-11 Geburtstagssinge Zwillinge - 3.m4v`
  - `/Users/patrickkurmann/Movies/Compressed Media/2024-06-11 Geburtstagssinge Zwillinge.jpg`
- Diese Dateien haben einen anderen Titel und werden daher nicht berücksichtigt.

### 10.4 Besonderheiten

- **SD, HD und 4K gehören zur Obergruppe "Internet-Dateien"** und sind für Internet-Streaming geeignet.
- **Medienserver-Datei** wird anhand der hohen Bitrate identifiziert.

## 11. Besonderheiten

### 11.1 Mehrere Mediensets in einem Verzeichnis

- **Eindeutige Identifikation**:
  - Durch den Titel aus den Metadaten der Referenzdatei.
- **Isolierung von Mediensets**:
  - Nur Dateien, deren Dateinamen mit dem ermittelten Titel beginnen, werden berücksichtigt.

### 11.2 Suche in Verzeichnissen

- **Suchreihenfolge für Titelbild**:
  - Zuerst in Verzeichnis 1.
  - Dann im zusätzlichen Verzeichnis.
- **Gründe für die Suchreihenfolge**:
  - Möglicherweise befinden sich aktuelle Versionen oder bevorzugte Dateien im Verzeichnis der Referenzdatei.

## 12. Zusammenfassung

Der Algorithmus ermöglicht es, ausgehend von einer einzelnen Videodatei ein vollständiges Medienset zusammenzustellen, indem er anhand des Titels zusammengehörige Dateien identifiziert und sie entsprechend ihrer Eigenschaften klassifiziert. Dabei werden sowohl Auflösung als auch Bitrate berücksichtigt, um die Dateien den richtigen Kategorien zuzuweisen. SD, HD und 4K-Dateien werden als Internet-Dateien zusammengefasst, da sie für Internet-Streaming geeignet sind. Die Medienserver-Datei wird anhand ihrer hohen Bitrate identifiziert und separat behandelt.
