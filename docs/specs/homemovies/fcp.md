## **Automatisierte Workflow-Spezifikation: Final Cut Pro zu Emby NFO-Metadatendateien**

**Version 1.1 vom 26. Oktober 2024**  
**Autor: Patrick Kurmann**

### Inhaltsverzeichnis

1. [Einleitung](#1-einleitung)
    - 1.1 [Versionierung und Änderungen](#11-versionierung-und-änderungen)
2. [Überblick](#2-überblick)
3. [Anforderungen](#3-anforderungen)
4. [Projektmetadaten in Final Cut Pro](#4-projektmetadaten-in-final-cut-pro)
    - 4.1 Titel
    - 4.2 Beschreibung
    - 4.3 Fassung (Version)
    - 4.4 Keywords
    - 4.5 Titelbild
    - 4.6 Album
    - 4.7 Mapping der FCP-Felder zu EXIF-Tags und NFO-Tags
5. [Automatisierte NFO-Erstellung](#5-automatisierte-nfo-erstellung)
    - 5.1 Beschreibung-Verarbeitung
    - 5.2 Generierung der NFO-Datei
    - 5.3 Titelbild-Konvertierung
6. [Zusätzliche Empfehlungen](#6-zusätzliche-empfehlungen)
    - 6.1 Editierbarkeit der NFO-Dateien
    - 6.2 Metadatensynchronisation (Optional)
    - 6.3 Ausschluss von Final Cut Pro Keywords
7. [Beispiele](#7-beispiele)
    - 7.1 Beispielhafte Projektmetadaten in Final Cut Pro
    - 7.2 Generierte NFO-Datei
8. [Fazit](#8-fazit)

### 1. Einleitung

#### 1.1 Versionierung und Änderungen

Diese Spezifikation wurde von **Version 1.0** auf **Version 1.1** aktualisiert. Die wesentlichen Änderungen umfassen:

- **Änderung der Titel-Verarbeitung:**
  - **Version 1.0:** Das Aufnahmedatum wurde als Präfix im Titel verwendet und im Dateinamen vorangestellt.
  - **Version 1.1:** Das Aufnahmedatum wird nun manuell im Feld "Datum" in Final Cut Pro als ISO-Datum (`YYYY-MM-DD`) eingetragen. Der Titel enthält ausschließlich den Namen des Videos ohne vorangestelltes Datum. Der Dateiname enthält kein ISO-Datum mehr vorangestellt; dieses ist nun in den Metadaten verankert.

- **Anpassung der Feldzuordnungen zur Vereinfachung:**
  - **Kategorie:** Wird nun direkt für die Emby-Sets (Sammlungen) verwendet, anstatt in der Beschreibung eingebettet zu werden. Dies vereinfacht das Parsing und reduziert die Komplexität.
  - **Attribute:** Wird ausschließlich für die manuellen Tags genutzt, z.B. `Apple Log`. Dadurch entfällt die Notwendigkeit, Tags aus der Beschreibung zu extrahieren.
  - **Genre:** Standardmäßig auf "Family" gesetzt, um eine einheitliche Kategorisierung der Familienfilme zu gewährleisten.
  - **Produktion:** Füllt den Namen des Videoschnitts im Format "Vorname Name" ein. Bei mehreren Personen werden diese durch Semikolon getrennt.
  - **Regie:** Enthält alle Personen, die die Originalaufnahmen beigesteuert haben, getrennt durch Semikolon.
  - **Sendung:** Entspricht dem Album-Tag der EXIF-Daten und wird in der Verzeichnishierarchie auf Level 2 genutzt (/Familienfilme/Album/Jahr/). Wird nicht in die NFO-Datei übernommen.

Diese Änderungen verbessern die Klarheit und Konsistenz des Workflows und gewährleisten eine nahtlose Integration in die Emby-Mediathek.

### 2. Überblick

Diese Spezifikation beschreibt den automatisierten Workflow vom Videoschnitt in Final Cut Pro zur Erstellung von NFO-Metadatendateien für die Emby-Mediathek. Ziel ist es, durch die Vorgabe spezifischer Projektmetadaten in Final Cut Pro die manuelle Pflege von Metadaten zu minimieren und eine konsistente, fehlerfreie Integration in die Emby-Mediathek zu gewährleisten.

### 3. Anforderungen

- **Final Cut Pro (FCP):** Videobearbeitungssoftware zur Eingabe und Verwaltung von Projektmetadaten.
- **Automatisierungsskript:** Ein Skript (z.B. in Python) zur Verarbeitung der FCP-Metadaten und zur Generierung der NFO-Dateien.
- **Emby Mediathek:** Medienserver, der die generierten NFO-Dateien zur Metadatenpflege nutzt.
- **Dateisystem:** Strukturierte Verzeichnisse gemäß der [Emby Mediathek Spezifikation für Familienfilme Version 1.5](docs/specs/emby_media_family_films.md).


### 4. Projektmetadaten in Final Cut Pro

Um eine automatische Erstellung der NFO-Dateien zu ermöglichen, müssen folgende Projektmetadaten in Final Cut Pro hinterlegt werden:

#### 4.1 Titel

- **Format:** `[Titel des Videos]`
  - **Beispiel:** `Paula MTB-Finale Huttwil`
- **Richtlinien:**
  - Der Titel enthält nur den Namen des Videos ohne vorangestelltes Aufnahmedatum.
- **Vorteile:**
  - Klare und prägnante Titel ohne zusätzliche Datumsangaben.
  - Vereinfachung der Dateinamensstruktur.

#### 4.2 Beschreibung

- **Format für Sammlungen und Tags:**
  - **Sammlungen:** `Sammlung: Familienausflüge, Familie, Ausflüge` oder `Sammlungen: Familienausflüge, Familie, Ausflüge`
  - **Tags:** `Tags: Apple Log, Sport` oder `Tag: Apple Log, Sport`
- **Beispiel:**

Sammlung: Familienausflüge, Familie, Ausflüge
Tags: Apple Log, Sport
Text: Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.

- **Hinweise:**
- Das Skript erkennt und extrahiert die Sammlungen und Tags basierend auf diesem Format.
- Das Aufnahmedatum wird nicht in der Beschreibung hinterlegt, sondern im Feld "Datum" manuell als ISO-Datum eingetragen.

#### 4.3 Fassung (Version)

- **Format im Titel:** `[Fassung für Verein]`
- **Beispiel:** `Paula MTB-Finale Huttwil [Fassung für Verein]`
- **Richtlinien:**
- Ähnliche Handhabung wie bei Titeln für Ereignisse zur Eindeutigkeit.

#### 4.4 Keywords

- **Manuelle Erstellung:** Anstelle der automatischen Erstellung durch Final Cut Pro werden die Tags manuell im Feld "Attribute" gesetzt, z.B. `Apple Log`.
- **Verwendung:** Diese Keywords werden in den Metadaten des Videos enthalten und von Emby verwendet.
- **Vorteil:**
- Vermeidung des mühsamen Herausparsens von Tags aus der Beschreibung.
- Präzisere und zielgerichtete Tagging-Möglichkeiten.

#### 4.5 Titelbild

- **Format in Final Cut Pro:** PNG
- **Anmerkung:** Das exportierte Titelbild ist häufig im Rec.2020 Farbraum aufgrund der Dolby Vision HLG Bearbeitung.
- **Anforderungen für Emby:**
- **Format:** JPG
- **Farbraum:** Adobe RGB
- **Hinweis:** Das Automatisierungsskript muss das Titelbild entsprechend konvertieren.

#### 4.6 Album

- **Format:** Im Projektmetadatenfeld von Final Cut Pro wird das Album definiert, z.B. `Album: Familie Kurmann Glück`.
- **Beispiel:** `Album: Familie Kurmann Glück`
- **Hinweise:**
- Das Skript liest die Album-Information aus und organisiert die Medien entsprechend in der Emby-Mediathek.
- Albums dienen der thematischen Gruppierung und Berechtigungssteuerung.

#### 4.7 Mapping der FCP-Felder zu EXIF-Tags und NFO-Tags

Um die Felder von Final Cut Pro korrekt den EXIF-Tags und NFO-Tags zuzuordnen, wird die folgende Zuordnung verwendet:

##### Titel
- **Final Cut Pro Feldname:** **Titel**
- **EXIF-Tag:** `Title`
- **NFO-Tag:** `<title>`
- **Beschreibung:** Der Titel des Videos, wird in Emby prominent angezeigt.

##### Beschreibung
- **Final Cut Pro Feldname:** **Beschreibung**
- **EXIF-Tag:** `Description`
- **NFO-Tag:** `<plot>`, `<description>`
- **Beschreibung:** Die Beschreibung des Videos, wird in Emby unter dem Vorschaubild angezeigt.

##### Attribute
- **Final Cut Pro Feldname:** **Attribute**
- **EXIF-Tag:** -
- **NFO-Tag:** `<keywords>`
- **Beschreibung:** Manuell gesetzte Tags zur zusätzlichen Beschreibung außerhalb der EXIF-Daten, werden in Emby verwendet, um zusätzliche Suchbegriffe bereitzustellen.

##### Ersteller
- **Final Cut Pro Feldname:** **Ersteller**
- **EXIF-Tag:** `Author`
- **NFO-Tag:** `<author>`, `<producers>`
- **Beschreibung:** Der Autor oder Ersteller des Videos, wird in Emby unter den Produktionsdetails angezeigt.

##### Datum
- **Final Cut Pro Feldname:** **Datum**
- **EXIF-Tag:** `Creation Date`
- **NFO-Tag:** `<premiered>`, `<releasedate>`, `<published>`, `<dateadded>`
- **Beschreibung:**
  - Das Feld **Datum** in Final Cut Pro wird auf `Creation Date` gemappt. Im Sinne der Familienfilme tragen wir dort das **Aufnahmedatum** ein.
  - Die Felder **Create Date** und **Modify Date** werden automatisch auf das **Exportdatum** gesetzt.
  - In den meisten Fällen (sofern das Video nicht nachträglich neu exportiert wurde) kann das **Create Date** als **Tag des Videoschnitts** interpretiert werden.
  - Das Videoschnitt-Datum (aus `Creation Date`) wird relativ genau (abgesehen von der Uhrzeit) in den NFO-Tag `<dateadded>` übernommen.
  - Angesichts der Tatsache, dass das Video automatisch (mit unserem Algorithmus) nach dem Export in die Emby-Mediathek übernommen wird, kann man das ungefähr auch als **Videoschnittdatum** interpretieren.

##### Genre
- **Final Cut Pro Feldname:** **Genre**
- **EXIF-Tag:** `Genre`
- **NFO-Tag:** `<genre>`
- **Beschreibung:** Das Genre des Videos, wird in Emby zur Kategorisierung verwendet. Im Sinne der Familienfilme ist dieses Feld immer auf "Family" gesetzt.

##### Kategorie
- **Final Cut Pro Feldname:** **Kategorie**
- **EXIF-Tag:** -
- **NFO-Tag:** `<set>`
- **Beschreibung:** Wird nun für die Emby-Sets (Sammlungen) verwendet. Die Berechtigungsgruppe und Themengruppe (z.B. "Familie Kurmann", "Kurmann-Glück Highlights") werden über das Album gesteuert. Dieses Feld bleibt aktuell unausgefüllt, wenn es nicht für ein Set verwendet wird.

##### Produktion
- **Final Cut Pro Feldname:** **Produktion**
- **EXIF-Tag:** `Producer`
- **NFO-Tag:** `<producer>`
- **Beschreibung:** Der Produzent des Videos, wird in Emby unter den Produktionsdetails angezeigt. Der Name des Videoschnitts wird hier im Format "Vorname Name" eingetragen. Bei mehreren Produzenten werden diese durch Semikolon getrennt.

##### Regie
- **Final Cut Pro Feldname:** **Regie**
- **EXIF-Tag:** `Director`
- **NFO-Tag:** `<director>`
- **Beschreibung:** Der Regisseur des Videos, wird in Emby unter den Regisseurdetails angezeigt. Alle Personen, die die Originalaufnahmen beigesteuert haben, werden hier eingetragen, getrennt durch Semikolon.

##### Sendung
- **Final Cut Pro Feldname:** **Sendung**
- **EXIF-Tag:** `Album`
- **NFO-Tag:** -
- **Beschreibung:** Entspricht dem Album-Tag der EXIF-Daten und wird in der Verzeichnishierarchie auf Level 2 genutzt (/Familienfilme/Album/Jahr/). Wird nicht in die NFO-Datei übernommen.

**Hinweise zur Zuordnung und Sonderlogik:**

##### Mapping Einfluss
- Die Zuordnung der Final Cut Pro-Feldnamen zu den EXIF-Tags kann nicht direkt beeinflusst werden. Unser Automatisierungsskript übernimmt die Überführung der relevanten EXIF-Tags in die entsprechenden NFO-Tags.

##### Sonderlogik in Final Cut Pro
- **Sammlungen und Tags:** Werden nun über das Feld "Kategorie" für Emby-Sets (Sammlungen) und das Feld "Attribute" für Tags gesetzt, was eine klare Strukturierung und einfache Extraktion durch das Skript ermöglicht.
- **Titelpräfix:** Wird nicht mehr verwendet. Stattdessen wird das Aufnahmedatum manuell im Feld "Datum" als ISO-Datum eingetragen, und der Titel enthält nur den Namen des Videos.

Diese Sonderlogiken dienen der klaren Strukturierung der Metadaten und erleichtern die automatisierte Verarbeitung durch das Skript, um eine konsistente und fehlerfreie Integration in die Emby-Mediathek zu gewährleisten.

### 5. Automatisierte NFO-Erstellung

Ein Python-Skript (oder ein anderes geeignetes Tool) übernimmt die folgenden Schritte zur Generierung der NFO-Dateien:

#### 5.1 Generierung der NFO-Datei

1. **Vorlage der NFO-Struktur:**
 - Nutzung der extrahierten Metadaten zur Befüllung der XML-Tags.
 - Einbindung des Aufnahmedatums im `<plot>`-Tag mit dem Wochentagskürzel.
 - Übernahme des Videoschnittdatums in das `<dateadded>`-Tag.

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
    <keywords>Familienausflüge,Familie,Ausflüge,Apple Log,Sport</keywords>
    <producers>
      <name>Patrick Kurmann</name>
    </producers>
    <directors>
      <name>Patrick Kurmann</name>
      <name>Kathrin Glück</name>
    </directors>
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
    <dateadded>2024-04-27</dateadded>
  </movie>
  ```

#### 5.2 Titelbild-Konvertierung

- **Ausgangslage:**
- Titelbild in Final Cut Pro: PNG, Rec.2020
- **Anforderungen:**
- Konvertierung zu JPG und Adobe RGB
- **Hinweis:** Die Konvertierung des Titelbildes muss in geeigneter Weise erfolgen, ist jedoch nicht Teil dieser Spezifikation.

### 6. Zusätzliche Empfehlungen

#### 6.1 Editierbarkeit der NFO-Dateien

- **Vorgenerierte NFO-Dateien:** 
- Die NFO-Dateien werden automatisch erstellt und bedürfen keiner manuellen Anpassung.
- Dies minimiert Fehler und sorgt für Konsistenz.

- **Emby Metadaten-Editor:** 
- Änderungen und Korrekturen können direkt im Emby Metadaten-Editor vorgenommen werden.
- Emby fungiert als Single-Point-of-Truth für die Metadaten.

#### 6.2 Metadatensynchronisation (Optional)

- **Ziel:** Synchronisation von Metadaten zwischen verschiedenen Speicherorten (z.B. iCloud-Verzeichnis).
- **Implementierung:** Kann durch zusätzliche Skripte oder Tools realisiert werden, ist jedoch nicht Teil dieser Spezifikation.
- **Empfehlung:** Evaluieren Sie den Bedarf und implementieren Sie bei Bedarf eine Synchronisationslösung, um Metadaten konsistent zu halten.

#### 6.3 Ausschluss von Final Cut Pro Keywords

- **Begründung:** Die automatisch in Final Cut Pro generierten Keywords werden von Emby ignoriert.
- **Lösung:** Nutzen Sie stattdessen die `<tag>`-Struktur im Beschreibungstext (`Tags:`), um relevante Tags zu definieren und in den NFO-Dateien zu verwenden.

### 7. Beispiele

#### 7.1 Beispielhafte Projektmetadaten in Final Cut Pro

- **Titel:** `Paula MTB-Finale Huttwil`
- **Beschreibung:**

Sammlung: Familienausflüge, Familie, Ausflüge
Tags: Apple Log, Sport
Text: Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.

- **Album:** `Familie Kurmann Glück`
- **Datum:** `2024-09-15` (ISO-Format)
- **Titelbild:** `titelbild.png` (Rec.2020, PNG)

#### 7.2 Generierte NFO-Datei

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
<keywords>Familienausflüge,Familie,Ausflüge,Apple Log,Sport</keywords>
<producers>
  <name>Patrick Kurmann</name>
</producers>
<directors>
  <name>Patrick Kurmann</name>
  <name>Kathrin Glück</name>
</directors>
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
<dateadded>2024-04-27</dateadded>
</movie>
```

### 8. Fazit

Diese Spezifikation bietet einen klaren und strukturierten Ansatz zur automatisierten Erstellung von NFO-Metadatendateien aus Final Cut Pro Projekten. Durch die genaue Vorgabe der erforderlichen Projektmetadaten und die Nutzung eines automatisierten Skripts wird der Workflow effizienter gestaltet, der manuelle Aufwand minimiert und die Konsistenz der Metadaten gewährleistet. Der empfohlene Workflow fördert eine nahtlose Integration in die Emby-Mediathek und stellt sicher, dass alle Metadaten korrekt und vollständig sind. Zusätzliche Empfehlungen wie die Metadatensynchronisation bieten Flexibilität für zukünftige Erweiterungen und Anpassungen.

Ende der Spezifikation
