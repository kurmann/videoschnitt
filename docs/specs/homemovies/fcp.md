## **Automatisierte Workflow-Spezifikation: Final Cut Pro zu Emby NFO-Metadatendateien**

**Version 1.0 vom 26. Oktober 2024**  
**Autor: Patrick Kurmann**

### Inhaltsverzeichnis

1. Überblick  
2. Anforderungen  
3. Projektmetadaten in Final Cut Pro  
4. Automatisierte NFO-Erstellung  
5. Zusätzliche Empfehlungen  
6. Beispiele  
7. Fazit

### 1. Überblick

Diese Spezifikation beschreibt den automatisierten Workflow vom Videoschnitt in Final Cut Pro zur Erstellung von NFO-Metadatendateien für die Emby Mediathek. Ziel ist es, durch die Vorgabe spezifischer Projektmetadaten in Final Cut Pro die manuelle Pflege von Metadaten zu minimieren und eine konsistente, fehlerfreie Integration in die Emby-Mediathek zu gewährleisten.

### 2. Anforderungen

- **Final Cut Pro (FCP):** Videobearbeitungssoftware zur Eingabe und Verwaltung von Projektmetadaten.
- **Automatisierungsskript:** Ein Skript (z.B. in Python) zur Verarbeitung der FCP Metadaten und zur Generierung der NFO-Dateien.
- **Emby Mediathek:** Medienserver, der die generierten NFO-Dateien zur Metadatenpflege nutzt.
- **Dateisystem:** Strukturierte Verzeichnisse gemäß der [Emby Mediathek Spezifikation für Familienfilme Version 1.6](#emby-mediathek-spezifikation-für-familienfilme).

### 3. Projektmetadaten in Final Cut Pro

Um eine automatische Erstellung der NFO-Dateien zu ermöglichen, müssen folgende Projektmetadaten in Final Cut Pro hinterlegt werden:

#### 3.1 Titel

- **Format:** `[ISO-Aufnahmedatum] [Titel des Videos]`
  - **Beispiel:** `2024-09-15 Paula MTB-Finale Huttwil`
- **Richtlinien für Ereignisse:** Das Aufnahmedatum entspricht dem letzten Tag des Zeitintervalls.
  - **Beispiel:** Für ein Jahresrückblick-Video von 2023 wird das Datum `2023-12-31` verwendet.
- **Vorteile:**
  - Sortierbarkeit der Projekte nach Datum.
  - Eindeutigkeit bei mehrfachen ähnlichen Titeln durch unterschiedliche Aufnahmedaten.

#### 3.2 Beschreibung

- **Format für Sets und Tags:**
  - **Sets:** `Set: Familienausflüge, Familie, Ausflüge` oder `Sets: Familienausflüge, Familie, Ausflüge`
  - **Tags:** `Tags: Apple Log, Sport` oder `Tag: Apple Log, Sport`
- **Beispiel:**
  ```
  Set: Familienausflüge, Familie, Ausflüge
  Tags: Apple Log, Sport
  Text: Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
  ```
- **Hinweise:**
  - Das Skript erkennt und extrahiert die Sets und Tags basierend auf diesem Format.
  - Das Aufnahmedatum wird nicht in der Beschreibung hinterlegt, sondern automatisch in der NFO-Datei eingefügt.

#### 3.3 Fassung (Version)

- **Format im Titel:** `[Fassung für Verein]`
  - **Beispiel:** `2024-09-15 Paula MTB-Finale Huttwil [Fassung für Verein]`
- **Richtlinien:**
  - Ähnliche Handhabung wie bei Titeln für Ereignisse zur Eindeutigkeit.

#### 3.4 Keywords

- **Automatische Erstellung:** Final Cut Pro generiert automatisch Keywords basierend auf dem Projektinhalt.
- **Verwendung:** Diese Keywords werden in den Metadaten des Videos enthalten, werden jedoch von Emby ignoriert.
- **Empfehlung:** Nutzen Sie stattdessen die `<tag>`-Struktur im Beschreibungstext (`Tags:`), um relevante Tags zu definieren und in den NFO-Dateien zu verwenden.

#### 3.5 Titelbild

- **Format in Final Cut Pro:** PNG
- **Anmerkung:** Das exportierte Titelbild ist häufig im Rec.2020 Farbraum aufgrund der Dolby Vision HLG Bearbeitung.
- **Anforderungen für Emby:**
  - **Format:** JPG
  - **Farbraum:** Adobe RGB
- **Hinweis:** Das Automatisierungsskript muss das Titelbild entsprechend konvertieren.

#### 3.6 Album

- **Format:** Im Projektmetadatenfeld von Final Cut Pro wird das Album definiert, z.B. `Album: Familie Kurmann Glück`.
- **Beispiel:** `Album: Familie Kurmann Glück`
- **Hinweise:**
  - Das Skript liest die Album-Information aus und organisiert die Medien entsprechend in der Emby Mediathek.
  - Albums dienen der thematischen Gruppierung und Berechtigungssteuerung.

### 4. Automatisierte NFO-Erstellung

Ein Python-Skript (oder ein anderes geeignetes Tool) übernimmt die folgenden Schritte zur Generierung der NFO-Dateien:

#### 4.1 Titel-Verarbeitung

1. **Extraktion des ISO-Aufnahmedatums und des Titels:**
   - Trennung des Titels am ersten Leerzeichen nach dem Datum.
   - **Beispiel:** `2024-09-15 Paula MTB-Finale Huttwil` wird in:
     - **Aufnahmedatum:** `2024-09-15`
     - **Titel:** `Paula MTB-Finale Huttwil`

#### 4.2 Beschreibung-Verarbeitung

1. **Erkennung von Sets und Tags:**
   - Parsing der Beschreibung, um Zeilen mit `Set:`, `Sets:`, `Tag:`, oder `Tags:` zu identifizieren.
   - Extraktion der Sets und Tags.
   - **Beispiel:**
     ```
     Set: Familienausflüge, Familie, Ausflüge
     Tags: Apple Log, Sport
     Text: Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
     ```
     - **Sets:** `Familienausflüge`, `Familie`, `Ausflüge`
     - **Tags:** `Apple Log`, `Sport`
     - **Text:** `Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.`

#### 4.3 Generierung der NFO-Datei

1. **Vorlage der NFO-Struktur:**
   - Nutzung der extrahierten Metadaten zur Befüllung der XML-Tags.
   - Einbindung des Aufnahmedatums im `<plot>`-Tag mit dem Wochentagskürzel.
2. **Beispielhafte NFO-Datei:**

    ```xml
    <?xml version="1.0" encoding="utf-8" standalone="yes"?>
    <movie>
      <plot><![CDATA[So. 15.09.2024. Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.]]></plot>
      <title>Paula MTB-Finale Huttwil</title>
      <year>2024</year>
      <sorttitle>Paula MTB-Finale Huttwil</sorttitle>
      <premiered>2024-09-15</premiered>
      <releasedate>2024-09-15</releasedate>
      <copyright>© Patrick Kurmann 2024</copyright>
      <published>2024-09-15</published>
      <keywords>17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann</keywords>
      <producers>
        <name>Patrick Kurmann</name>
      </producers>
      <directors></directors>
      <tag>Apple Log</tag>
      <tag>Sport</tag>
      <set>
        <name>Familienausflüge</name>
      </set>
      <set>
        <name>Familie</name>
      </set>
      <set>
        <name>Ausflüge</name>
      </set>
      <actor>
        <name>Paula Gorycka</name>
        <role>Rennfahrerin</role>
        <type>Actor</type>
      </actor>
      <actor>
        <name>Andreas Kurmann sr</name>
        <role>Teamchef</role>
        <type>Actor</type>
      </actor>
      <actor>
        <name>Patrick Kurmann</name>
        <role>Videoschnitt</role>
        <type>Actor</type>
      </actor>
      <actor>
        <name>Patrick Kurmann</name>
        <role>Filmaufnahmen</role>
        <type>Actor</type>
      </actor>
      <actor>
        <name>Silvan Kurmann</name>
        <role>Filmaufnahmen</role>
        <type>Actor</type>
      </actor>
    </movie>
    ```

#### 4.4 Titelbild-Konvertierung

- **Ausgangslage:**
  - Titelbild in Final Cut Pro: PNG, Rec.2020
- **Anforderungen:**
  - Konvertierung zu JPG und Adobe RGB
- **Hinweis:** Die Konvertierung des Titelbildes muss in geeigneter Weise erfolgen, ist jedoch nicht Teil dieser Spezifikation.

### 5. Zusätzliche Empfehlungen

#### 5.1 Editierbarkeit der NFO-Dateien

- **Vorgenerierte NFO-Dateien:** 
  - Die NFO-Dateien werden automatisch erstellt und bedürfen keiner manuellen Anpassung.
  - Dies minimiert Fehler und sorgt für Konsistenz.
  
- **Emby Metadaten-Editor:** 
  - Änderungen und Korrekturen können direkt im Emby Metadaten-Editor vorgenommen werden.
  - Emby fungiert als Single-Point-of-Truth für die Metadaten.

#### 5.2 Metadatensynchronisation (Optional)

- **Ziel:** Synchronisation von Metadaten zwischen verschiedenen Speicherorten (z.B. iCloud-Verzeichnis).
- **Implementierung:** Kann durch zusätzliche Skripte oder Tools realisiert werden, ist jedoch nicht Teil dieser Spezifikation.
- **Empfehlung:** Evaluieren Sie den Bedarf und implementieren Sie bei Bedarf eine Synchronisationslösung, um Metadaten konsistent zu halten.

#### 5.3 Ausschluss von Final Cut Pro Keywords

- **Begründung:** Die automatisch in Final Cut Pro generierten Keywords werden von Emby ignoriert.
- **Lösung:** Nutzen Sie stattdessen die `<tag>`-Struktur im Beschreibungstext (`Tags:`), um relevante Tags zu definieren und in den NFO-Dateien zu verwenden.

### 6. Beispiele

#### 6.1 Beispielhafte Projektmetadaten in Final Cut Pro

- **Titel:** `2024-09-15 Paula MTB-Finale Huttwil`
- **Beschreibung:**
  ```
  Set: Familienausflüge, Familie, Ausflüge
  Tags: Apple Log, Sport
  Text: Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
  ```
- **Album:** `Familie Kurmann Glück`
- **Titelbild:** `titelbild.png` (Rec.2020, PNG)

#### 6.2 Generierte NFO-Datei

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<movie>
  <plot><![CDATA[So. 15.09.2024. Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.]]></plot>
  <title>Paula MTB-Finale Huttwil</title>
  <year>2024</year>
  <sorttitle>Paula MTB-Finale Huttwil</sorttitle>
  <premiered>2024-09-15</premiered>
  <releasedate>2024-09-15</releasedate>
  <copyright>© Patrick Kurmann 2024</copyright>
  <published>2024-09-15</published>
  <keywords>17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann</keywords>
  <producers>
    <name>Patrick Kurmann</name>
  </producers>
  <directors></directors>
  <tag>Apple Log</tag>
  <tag>Sport</tag>
  <set>
    <name>Familienausflüge</name>
  </set>
  <set>
    <name>Familie</name>
  </set>
  <set>
    <name>Ausflüge</name>
  </set>
  <actor>
    <name>Paula Gorycka</name>
    <role>Rennfahrerin</role>
    <type>Actor</type>
  </actor>
  <actor>
    <name>Andreas Kurmann sr</name>
    <role>Teamchef</role>
    <type>Actor</type>
  </actor>
  <actor>
    <name>Patrick Kurmann</name>
    <role>Videoschnitt</role>
    <type>Actor</type>
  </actor>
  <actor>
    <name>Patrick Kurmann</name>
    <role>Filmaufnahmen</role>
    <type>Actor</type>
  </actor>
  <actor>
    <name>Silvan Kurmann</name>
    <role>Filmaufnahmen</role>
    <type>Actor</type>
  </actor>
</movie>
```

### 7. Fazit

Diese Spezifikation bietet einen klaren und strukturierten Ansatz zur automatisierten Erstellung von NFO-Metadatendateien aus Final Cut Pro Projekten. Durch die genaue Vorgabe der erforderlichen Projektmetadaten und die Nutzung eines automatisierten Skripts wird der Workflow effizienter gestaltet, der manuelle Aufwand minimiert und die Konsistenz der Metadaten gewährleistet. Der empfohlene Workflow fördert eine nahtlose Integration in die Emby Mediathek und stellt sicher, dass alle Metadaten korrekt und vollständig sind. Zusätzliche Empfehlungen wie die Metadatensynchronisation bieten Flexibilität für zukünftige Erweiterungen und Anpassungen.

**Ende der Spezifikation**
