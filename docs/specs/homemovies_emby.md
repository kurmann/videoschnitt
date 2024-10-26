## **Emby Mediathek Spezifikation für Familienfilme**

**Version 1.0 vom 26. Oktober 2024**  
**Autor: Patrick Kurmann**

### Inhaltsverzeichnis

1. Überblick  
2. Verzeichnisstruktur  
3. Dateinamenkonventionen  
4. Metadaten-Dateien (NFO)  
5. Integration mit Emby  
6. Aufnahmedatum  
7. Sets und Tags  
8. Beteiligte Personen  
9. Album  
10. Beispiele  
11. Validierung und Transportierbarkeit  
12. Hinweise zum Workflow  
13. Anhang: Verzeichnisbeispiele

### 1. Überblick

Diese Spezifikation beschreibt die strukturierte Organisation und Archivierung von Familienfilmen innerhalb des Emby Mediaservers. Ziel ist es, durch konsistente Naming-Konventionen und detaillierte Metadaten eine optimale Darstellung und Verwaltung der Medien zu gewährleisten. Die Struktur ist modular aufgebaut, um zukünftige Erweiterungen wie Kinofilme oder Internetfilme problemlos zu integrieren.

#### Zielsetzungen

- **Konsistente Benennung:** Einheitliche Namensgebung für einfache Navigation und Identifikation.
- **Strukturierte Metadaten:** Nutzung von NFO-Dateien zur Speicherung und Ergänzung von Metadaten.
- **Optimale Emby-Integration:** Sicherstellung, dass Emby die Metadaten effizient einliest und darstellt.
- **Hohe technische Qualität:** Bereitstellung von Videos in höchster Qualität ohne visuelle Einbußen gegenüber den Originalaufnahmen. Für 4K-Videos mit 60 Frames pro Sekunde in Dolby Vision wird eine Bitrate von mindestens 80 Mbit/s für die Codierung in HEVC empfohlen.
- **Erweiterbarkeit:** Möglichkeit zur einfachen Hinzufügung weiterer Medientypen und Kategorien.

### 2. Verzeichnisstruktur

Die Verzeichnisstruktur ist nach Medientypen organisiert. Jeder Medientyp hat sein eigenes Hauptverzeichnis, unter dem die jeweiligen Medien nach Album, Jahr und weiteren Kriterien sortiert werden.

#### Allgemeine Struktur

```
/[Medientyp]/
└── [Album]/
    └── [Jahr]/
        ├── [Aufnahmedatum]_[Titel].mov
        ├── [Aufnahmedatum]_[Titel].nfo
        └── [Aufnahmedatum]_[Titel]-poster.jpg
```

#### Beispiel:

```
/Familienfilme/
└── Familie Kurmann/
    └── 2024/
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.mov
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.nfo
        └── 2024-09-15_Paula_MTB-Finale_Huttwil-poster.jpg
```

**Hinweis:** Wenn es eine zusätzliche Fassung gibt, sollen diese ebenfalls eigene NFO-Dateien und Poster haben, z.B.:

```
/Familienfilme/
└── Familie Kurmann/
    └── 2024/
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.mov
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil [Fassung für Verein].mov
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.nfo
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil [Fassung für Verein].nfo
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil-poster.jpg
        └── 2024-09-15_Paula_MTB-Finale_Huttwil [Fassung für Verein]-poster.jpg
```

#### Verzeichnisstruktur für zentral verwaltete Metadaten

```
/Volumes/Familienfilme/Emby-Metadaten/metadata/
├── people/
│   ├── p/
│   │   └── Paula Gorycka/
│   │       ├── Paula Gorycka.nfo
│   │       └── Paula Gorycka-poster.jpg
│   ├── a/
│   │   └── Andreas Kurmann sr/
│   │       ├── Andreas Kurmann sr.nfo
│   │       └── Andreas Kurmann sr-poster.jpg
│   └── others/
│       └── ... (weitere Personen)
├── collections/
│   └── Familienausflüge/
│       ├── collection.nfo
│       └── poster.png
└── ... (weitere Metadatentypen)
```

**Hinweis:** Die filmbezogenen Metadaten befinden sich in den Sidecar-Dateien innerhalb der jeweiligen Filmverzeichnisse und nicht in einem separaten "movies"-Verzeichnis.

### 3. Dateinamenkonventionen

Die Konsistenz der Dateinamen ist entscheidend für die einfache Verwaltung und Auffindbarkeit der Medien.

#### Aufbau des Dateinamens

```
[Aufnahmedatum]_[Titel]_[Fassung].ext
```

- **Aufnahmedatum:** ISO-Format YYYY-MM-DD.
- **Titel:** Klarer und prägnanter Titel des Films.
- **Fassung:** Optional, in eckigen Klammern angegeben, z.B. `[Fassung für Verein]`.
- **Erweiterung:** Dateiformat, z.B. `.mov`, `.m4v`, `.nfo`, `-poster.jpg`.

#### Beispiele

- **Standardfassung:**

  ```
  2024-09-15_Paula_MTB-Finale_Huttwil.mov
  ```

- **Zusätzliche Fassung:**

  ```
  2024-09-15_Paula_MTB-Finale_Huttwil [Fassung für Verein].mov
  ```

- **Posterbild:**

  ```
  2024-09-15_Paula_MTB-Finale_Huttwil-poster.jpg
  ```

**Hinweise:**

- **Eindeutigkeit:** Jeder Dateiname innerhalb eines Albums und Jahres muss eindeutig sein.

### 4. Metadaten-Dateien (NFO)

Für jede Mediendatei wird eine entsprechende NFO-Datei erstellt, die die Metadaten enthält. Diese NFO-Datei hat denselben Namen wie die Mediendatei, jedoch mit der Erweiterung `.nfo`.

#### Struktur der NFO-Datei

Die NFO-Datei ist im XML-Format und enthält spezifische Tags, die von Emby erkannt und verwendet werden.

##### Grundstruktur

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<movie>
  <plot><![CDATA[Wochentag. TT.MM.JJJJ. Beschreibung des Films]]></plot>
  <title>Titel des Films</title>
  <year>YYYY</year>
  <sorttitle>Titel des Films</sorttitle>
  <premiered>YYYY-MM-DD</premiered>
  <releasedate>YYYY-MM-DD</releasedate>
  <copyright>© Name Jahr</copyright>
  <published>YYYY-MM-DD</published>
  <keywords>Schlüsselwort1,Schlüsselwort2,...</keywords>
  <producers>
    <name>Name des Videoschnitts</name>
  </producers>
  <directors></directors>
  <tag>Tag1</tag>
  <tag>Tag2</tag>
  <set>
    <name>Set1</name>
  </set>
  <set>
    <name>Set2</name>
  </set>
  <actor>
    <name>Name der Person</name>
    <role>Rolle</role>
    <type>Actor</type>
  </actor>
  <!-- Weitere Tags nach Bedarf -->
</movie>
```

##### Detaillierte Beschreibung der Tags

| Tag             | Beschreibung                                                                                      | Beispielnutzung                                                                                                                                                            |
|-----------------|---------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `<plot>`        | Enthält die Beschreibung des Films, wird in Emby prominent unter dem Vorschaubild angezeigt.      | So. 15.09.2024. Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.                                                                                |
| `<title>`       | Titel des Films.                                                                                  | Paula MTB-Finale Huttwil                                                                                                                                                    |
| `<year>`        | Jahr des Films, abgeleitet aus dem Aufnahmedatum oder dem letzten Jahr bei Rückblicken.           | 2024                                                                                                                                                                        |
| `<premiered>`   | Aufnahmedatum bei Ereignissen oder das Ende des Zeitraums bei Rückblicken.                        | 2024-09-15                                                                                                                                                                 |
| `<releasedate>` | Entspricht dem Aufnahmedatum.                                                                      | 2024-09-15                                                                                                                                                                 |
| `<copyright>`   | Name des Videoschnitts mit dem entsprechenden Jahr.                                              | © Patrick Kurmann 2024                                                                                                                                                      |
| `<published>`   | Datum des Videoschnitts.                                                                          | 2024-09-15                                                                                                                                                                 |
| `<keywords>`    | Liste von Schlüsselwörtern zur Kategorisierung (kann ignoriert werden, da Emby sie nicht prominent anzeigt). | 17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann                                                                    |
| `<producers>`   | Person des Videoschnitts.                                                                         | `<name>Patrick Kurmann</name>`                                                                                                                                            |
| `<directors>`   | Wird in diesem Kontext nicht verwendet.                                                           | Leer                                                                                                                                                                       |
| `<tag>`         | Zusätzliche Kennzeichnungen für weniger prominente Zusammenhänge.                                | `<tag>Apple Log</tag>`<br>`<tag>Sport</tag>`                                                                                                                                 |
| `<set>`         | Thematische Gruppierungen der Filme, ähnlich wie Sammlungen.                                     | `<set><name>Familienausflüge</name></set><set><name>Familie</name></set><set><name>Ausflüge</name></set>`                                                                |
| `<actor>`       | Liste der beteiligten Personen mit Rollen.                                                       | `<actor><name>Paula Gorycka</name><role>Rennfahrerin</role><type>Actor</type></actor>`                                                                                      |

##### Beispiel der Tag-Nutzung

```xml
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
```

**Zusätzliche Empfehlung:**

Es wird empfohlen, dass in der `<plot>`-Beschreibung das Aufnahmedatum einschließlich des Kürzels des Wochentags am Anfang eingebettet wird, z.B.:

```
So. 15.09.2024. Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
```

### 5. Integration mit Emby

Der Emby-Medienserver wird so konfiguriert, dass er die NFO-Dateien als Basis für die Metadaten verwendet. Emby ergänzt diese eigenständig um zusätzliche XML-Tags, insbesondere `streamdetails`.

#### Konfigurationsschritte

1. **Metadatenquelle festlegen:**
   - Stellen Sie sicher, dass der Pfad zu den Metadaten auf einem zugänglichen Netzlaufwerk liegt, z.B. `/Volumes/Familienfilme/Emby-Metadaten/`.

2. **Emby Einstellungen anpassen:**
   - Navigieren Sie in Emby zu den Bibliothekseinstellungen.
   - Aktivieren Sie die Option, NFO-Dateien als Metadatenquelle zu verwenden.
   - Deaktivieren Sie die automatische Metadatenbeschaffung, falls nur die NFO-Dateien verwendet werden sollen.

3. **Automatische Ergänzungen:**
   - Emby wird eigenständig `streamdetails` und andere erforderliche Tags hinzufügen.

### 6. Aufnahmedatum

Das **Aufnahmedatum** eines Videos ist eine wichtige Organisationsgrundlage und definiert das Datum, das zur Sortierung und Kategorisierung der Videos verwendet wird.

#### Grundsatz

Das Aufnahmedatum eines Videos ist immer das jüngste Datum der Filmaufnahmen, die im betreffenden Videoschnitt verwendet wurden.

#### Rückblicke

Bei Rückblicken gibt es naturgemäß mehrere Aufnahmedaten, da sie eine Zeitspanne umfassen. Für solche Rückblicke wird als Aufnahmedatum immer der **letzte Tag** der Zeitspanne verwendet.

- **Jahresrückblicke:** Das Aufnahmedatum ist der letzte Tag des Jahres, z.B. der **31.12.2023** für einen Rückblick von 2019-2023.
  
- **Monats- oder Jahreszeitrückblicke:** Hier wird ebenfalls der letzte Tag des Zeitraums verwendet, z.B. der **letzte Tag des Sommers** für einen Sommer-Rückblick.

Das **Jahr** wird auch in diesen Fällen vom Aufnahmedatum abgeleitet.

### 7. Sets und Tags

#### Sets

Sets dienen zur thematischen Gruppierung von Filmen. Sie können Sammlungen wie „Familienausflüge“, „Vater-Kinder-Tag“ etc. umfassen. Bei Familienfilmen interpretieren wir die Sets als Themen, ähnlich den „Sammlungen“ in Emby.

**Beispiel:**

```xml
<set>
  <name>Familienausflüge</name>
</set>
<set>
  <name>Familie</name>
</set>
<set>
  <name>Ausflüge</name>
</set>
```

**Zusätzliche Empfehlung:**

Für zentral verwaltete Sammlungen wird empfohlen, ein quadratisches Titelbild (`poster.png`) hinzuzufügen, um die optimale Darstellung in Emby zu gewährleisten. Diese Poster sollten im Verzeichnis der jeweiligen Sammlung abgelegt werden.

**Beispielpfade:**

```
/Volumes/Familienfilme/Emby-Metadaten/metadata/collections/Familienausflüge/poster.png
/Volumes/Familienfilme/Emby-Metadaten/metadata/collections/Familienausflüge/collection.nfo
```

#### Tags

Tags sind für weniger prominente Zusammenhänge gedacht, z.B. „Apple Log“, um Filme bestimmten Profilen zuzuordnen.

**Beispiel:**

```xml
<tag>Apple Log</tag>
<tag>Sport</tag>
```

#### Kombinierte Nutzung

**Beispiel:**

```xml
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
```

#### Übersichtstabelle der Sets und Tags

| Kategorie | Tag/Set          | Beschreibung                                                                                   | Beispiel                                                       |
|-----------|------------------|-----------------------------------------------------------------------------------------------|---------------------------------------------------------------|
| Tag       | Apple Log        | Kennzeichnung für Filme, die im Apple Log-Profil aufgenommen wurden.                         | `<tag>Apple Log</tag>`                                        |
| Tag       | Sport            | Kennzeichnung für sportliche Aktivitäten und Veranstaltungen.                                | `<tag>Sport</tag>`                                            |
| Set       | Familienausflüge | Thematische Sammlung aller Videoschnitte von Familienausflügen.                               | `<set><name>Familienausflüge</name></set>`                    |
| Set       | Familie          | Sammlung aller familienbezogenen Videos.                                                     | `<set><name>Familie</name></set>`                             |
| Set       | Ausflüge         | Sammlung aller Ausflüge, unabhängig von der spezifischen Familienaktivität.                   | `<set><name>Ausflüge</name></set>`                            |

---

### 8. Beteiligte Personen

Beteiligte Personen können in der NFO-Datei aufgenommen werden, inklusive ihrer Rollen. Dank dieser Vorgehensweise sieht man in der Emby Mediathek schön dargestellt, wer den Videoschnitt gemacht hat und wer die Filmaufnahmen durchgeführt hat – ein Zeichen des Respekts gegenüber der geleisteten Arbeit.

#### Struktur

```xml
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
```

#### Hinweise

- **Rollenbezeichnungen:** Statt klassischer Rollen wie „Regisseur“ oder „Produzent“ werden spezifische Rollen wie „Filmaufnahmen“ und „Videoschnitt“ verwendet. Dank dieser Vorgehensweise sieht man in der Emby Mediathek schön dargestellt, wer den Videoschnitt gemacht hat und wer die Filmaufnahmen durchgeführt hat – ein Zeichen des Respekts gegenüber der geleisteten Arbeit.
- **Erweiterbarkeit:** Weitere Personen können hinzugefügt werden, inklusive zusätzlicher Informationen wie Geburtstag.

### 9. Album

#### Definition und Zweck

Das **Album** ist eine übergeordnete Kategorie unterhalb des Hauptordners `Familienfilme`. Albums sind Unterkategorien, die spezifische Familiengruppen oder thematische Zusammenhänge repräsentieren. Sie dienen als Grundlage, um festzulegen, welche Videos für welche Familienmitglieder zugänglich sind und ermöglichen eine thematische Organisation der Inhalte.

#### Beispiele für Albums

- **Familie Kurmann:** Unsere Großfamilie.
- **Familie Kurmann Glück:** Videos von unserer Familie in Lissach.
- **Kurmann Glück Highlights:** Highlights von unserer Familie in Lissach, die auch andere Familienmitglieder interessant finden könnten.

#### Verzeichnisstruktur mit Album

```
/Familienfilme/
└── [Album]/
    └── [Jahr]/
        ├── [Aufnahmedatum]_[Titel].mov
        ├── [Aufnahmedatum]_[Titel].nfo
        └── [Aufnahmedatum]_[Titel]-poster.jpg
```

**Beispiel:**

```
/Familienfilme/
└── Familie Kurmann Glück/
    └── 2024/
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.mov
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.nfo
        └── 2024-09-15_Paula_MTB-Finale_Huttwil-poster.jpg
```

#### Integration in Final Cut Pro

Albums können in Final Cut Pro bei den Projektmetadaten hinterlegt werden. Der automatisierte Workflow liest diese Album-Informationen aus und organisiert die Medien entsprechend in der Emby Mediathek.

**Zweck der Albums:**

- **Berechtigungssteuerung:** Bestimmt, welche Videos an welche Familienmitglieder verteilt werden können.
- **Themenhafte Organisation:** Gruppiert Videos thematisch nach Familienmitgliedern oder speziellen Anlässen.

### 10. Beispiele

#### 10.1 NFO-Datei für einen Ereignis-Familienfilm

Das folgende Beispiel zeigt eine NFO-Datei für das Bike-Rennen-Medium „Paula MTB-Finale Huttwil“.

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

#### 10.2 Automatisierte Workflow-Spezifikation

Verweise auf die [Automatisierte Workflow-Spezifikation: Final Cut Pro zu Emby NFO-Metadatendateien Version 1.2](#automatisierte-workflow-spezifikation-final-cut-pro-zu-emby-nfo-metadatendateien-version-1.2), um Doppelspurigkeiten zu vermeiden.

### 11. Validierung und Transportierbarkeit

#### Validierung der NFO-Dateien

Die NFO-Dateien sollten syntaktisch korrektes XML sein. Nutzen Sie XML-Validatoren oder spezielle Tools, um die Struktur und die Inhalte zu überprüfen.

#### Transportierbarkeit

Da die Metadaten auf einem zugänglichen Netzlaufwerk gespeichert sind, können sie problemlos auf andere Emby-Server übertragen werden. Stellen Sie sicher, dass alle relevanten NFO-Dateien und zugehörigen Bilder (z.B. `-poster.jpg` und `poster.png`) vorhanden sind.

### 12. Hinweise zum Workflow

#### Erstellung der NFO-Dateien

Es wird empfohlen, die NFO-Dateien automatisch vorzugenerieren, um den manuellen Aufwand bei der Metadatenpflege zu minimieren. Dies kann durch einen bestimmten Algorithmus, beispielsweise ein Python-Skript, erfolgen. Der Workflow könnte folgendermaßen aussehen:

1. **Metadaten während des Videoschnitts vergeben:**
   - Beim Videoschnittprogramm (z.B. Final Cut Pro) werden die relevanten Metadaten direkt eingegeben, einschließlich Album.

2. **Automatische Generierung der NFO-Dateien:**
   - Nach dem Export des Videos wird automatisch eine NFO-Datei anhand der im Videoschnittprogramm hinterlegten Metadaten generiert.
   - Dieses Skript sorgt dafür, dass die NFO-Dateien konsistent und fehlerfrei erstellt werden.

3. **Verschiebung in die Mediathek:**
   - Die automatisch generierten NFO-Dateien sowie die Videos werden in die entsprechende Mediathek verschoben.
   - Emby interpretiert die Metadaten und fügt die Medien in die Bibliothek ein.

**Vorteile dieses Workflows:**

- **Reduzierter manueller Aufwand:** Minimierung der Handarbeit bei der Metadatenpflege.
- **Konsistenz:** Sicherstellung, dass alle Metadaten einheitlich und korrekt sind.
- **Automatisierung:** Effiziente Integration neuer Medien in die Emby-Mediathek.

**Zusätzliche Hinweise:**

- **Metadaten-Editor:** Trotz der automatischen Generierung ist vorgesehen, dass Änderungen und Korrekturen direkt im Emby Metadaten-Editor vorgenommen werden können. Dies ermöglicht eine flexible Anpassung und Fehlerkorrektur.
  
- **Single-Point-of-Truth:** Emby dient als Single-Point-of-Truth für die Metadaten. Die NFO-Dateien müssen nicht manuell angepasst werden können, da alle Änderungen über den Metadaten-Editor in Emby erfolgen. Dies gewährleistet eine zentrale und konsistente Verwaltung der Metadaten.
  
- **Metadatensynchronisation (optional):** In einer eigenen Implementierung kann eine Metadatensynchronisation sinnvoll sein, um beispielsweise Metadaten zwischen verschiedenen Speicherorten (z.B. iCloud-Verzeichnis) zu synchronisieren. Diese Funktion ist jedoch nicht Teil dieser Spezifikation und kann je nach Bedarf separat implementiert werden.

#### Pflege der Metadaten

- **Aktualisierungen:** Bei Änderungen am Film, wie z.B. zusätzlichen Szenen, sollten die NFO-Dateien entsprechend aktualisiert werden. Dies erfolgt idealerweise automatisch durch das vorgenerierende Skript.
- **Versionskontrolle:** Frühere Versionen können in separaten Unterverzeichnissen archiviert werden, um eine Rückverfolgung und Wiederherstellung zu ermöglichen.

### 13. Anhang: Verzeichnisbeispiele

#### 13.1 Verzeichnis für einen Film

```
/Familienfilme/
└── Familie Kurmann Glück/
    └── 2024/
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.mov
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil [Fassung für Verein].mov
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil.nfo
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil [Fassung für Verein].nfo
        ├── 2024-09-15_Paula_MTB-Finale_Huttwil-poster.jpg
        └── 2024-09-15_Paula_MTB-Finale_Huttwil [Fassung für Verein]-poster.jpg
```

#### 13.2 Verzeichnis für Metadaten und Personen

```
/Volumes/Familienfilme/Emby-Metadaten/metadata/
├── people/
│   ├── p/
│   │   └── Paula Gorycka/
│   │       ├── Paula Gorycka.nfo
│   │       └── Paula Gorycka-poster.jpg
│   ├── a/
│   │   └── Andreas Kurmann sr/
│   │       ├── Andreas Kurmann sr.nfo
│   │       └── Andreas Kurmann sr-poster.jpg
│   └── others/
│       └── ... (weitere Personen)
├── collections/
│   └── Familienausflüge/
│       ├── collection.nfo
│       └── poster.png
└── ... (weitere Metadatentypen)
```

### Fazit

Diese aktualisierte Spezifikation bietet eine klare und strukturierte Methode zur Organisation von Familienfilmen in der Emby Mediathek, inklusive der Integration von Albums zur thematischen Gruppierung und Berechtigungssteuerung. Durch die konsistente Benennung und detaillierte Metadaten wird die Verwaltung der Medien erleichtert und eine optimale Darstellung in Emby gewährleistet. Die prominente Darstellung der Tags und Sets in Tabellenform sowie die konsequente Nutzung des Bike-Rennen-Beispiels fördern die Übersichtlichkeit und Anwendbarkeit der Spezifikation. Die modulare Struktur, einschließlich der Integration von Albums, ermöglicht zudem eine einfache Erweiterung für weitere Medientypen und die Steuerung von Berechtigungen. Durch den empfohlenen automatisierten Workflow zur Generierung der NFO-Dateien wird der manuelle Aufwand minimiert und eine effiziente, konsistente Metadatenpflege sichergestellt.

**Ende der Spezifikation**
