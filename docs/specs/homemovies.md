# Integrierte Spezifikation zur Verwaltung, Verarbeitung und Ablage von Familienfilmen

Diese Spezifikation definiert einen einheitlichen Workflow zur Organisation, Verarbeitung und Ablage von Familienfilmen. Der Workflow erstreckt sich von der Videobearbeitung in Final Cut Pro (FCP) über die Integration in die Emby-Mediathek bis hin zur Ablage in der iCloud. Ziel ist eine konsistente Metadatenverwaltung und eine effiziente Automatisierung des gesamten Prozesses.

## 1. Einleitung

Diese Spezifikation beschreibt die strukturierte Organisation, Verarbeitung und Ablage von Familienfilmen. Sie deckt den gesamten Workflow ab, beginnend beim Videoschnitt in Final Cut Pro, über die Integration und Verwaltung in der Emby-Mediathek bis hin zur Ablage und dem Teilen in der iCloud-Mediathek. Durch konsistente Dateinamenkonventionen, detaillierte Metadatenverwaltung und automatisierte Prozesse wird eine effiziente und fehlerfreie Handhabung der Medien gewährleistet.

## 2. Überblick über den Workflow

Der Workflow umfasst die folgenden Hauptschritte:

1. **Videoschnitt in Final Cut Pro (FCP)**
   - Eingabe und Verwaltung von Projektmetadaten.
   - Export der bearbeiteten Videodateien mit eingebetteten Metadaten.
2. **Automatisierte Verarbeitung und Metadaten-Erstellung**
   - Generierung von NFO-Dateien für die Emby-Mediathek.
   - Konvertierung und Einbettung von Titelbildern.
3. **Integration in die Emby-Mediathek**
   - Strukturierte Ablage der Medien gemäß der definierten Verzeichnisstruktur.
   - Nutzung der NFO-Dateien zur detaillierten Metadatenverwaltung.
4. **Ablage in der iCloud-Mediathek**
   - Strukturierte und kompatible Ablage zur einfachen Freigabe und als Backup.
   - Einbettung von Metadaten direkt in die Videodateien oder als macOS-Tags erfasst.

## 3. Anforderungen

- **Final Cut Pro (FCP)**: Videobearbeitungssoftware zur Eingabe und Verwaltung von Projektmetadaten.
- **Emby Mediaserver**: Medienserver zur Verwaltung und Wiedergabe der Familienfilme.
- **iCloud**: Cloud-Speicherlösung zur Ablage und zum Teilen der Medien.
- **Automatisierungsskripte**: Skripte (z.B. in Python) zur Verarbeitung von Metadaten und zur Automatisierung des Workflows.
- **Dateisystem**: Strukturierte Verzeichnisse gemäß den nachfolgenden Spezifikationen.

## 4. Verzeichnisstruktur

Die Verzeichnisstruktur ist so gestaltet, dass sie die Anforderungen sowohl der Emby-Mediathek als auch der iCloud-Ablage erfüllt. Sie ermöglicht eine klare Organisation nach Alben und Jahren, wobei die Dateinamenkonventionen konsistent eingehalten werden.

### Allgemeine Struktur:

```
/Familienfilme/
├── [Album]/
│   └── [Jahr]/
│       ├── [Titel] (YYYY).mov
│       └── [Titel] (YYYY)-poster.jpg
/Volumes/Emby/metadata/
├── people/
│   ├── p/
│   │   └── [Personenname]/
│   │       ├── [Personenname].nfo
│   │       └── [Personenname]-poster.jpg
│   └── a/
│       └── [Personenname]/
│           ├── [Personenname].nfo
│           └── [Personenname]-poster.jpg
├── collections/
│   └── [Sammlungsname]/
│       ├── collection.nfo
│       └── poster.png
└── ... (weitere Metadatentypen)
```

### Beispiel:

```
/Familienfilme/
├── Familie Kurmann Glück/
│   └── 2024/
│       ├── Paula MTB-Finale Huttwil (2024).mov
│       └── Paula MTB-Finale Huttwil (2024)-poster.jpg
/Volumes/Emby/metadata/
├── people/
│   ├── p/
│   │   └── Paula Gorycka/
│   │       ├── Paula Gorycka.nfo
│   │       └── Paula Gorycka-poster.jpg
│   └── a/
│       └── Andreas Kurmann sr/
│           ├── Andreas Kurmann sr.nfo
│           └── Andreas Kurmann sr-poster.jpg
├── collections/
│   └── Familienausflüge/
│       ├── collection.nfo
│       └── poster.png
└── ... (weitere Metadatentypen)
```

## 5. Dateinamenkonventionen

Die Konsistenz der Dateinamen ist entscheidend für die einfache Verwaltung, Auffindbarkeit und Integration in die Emby-Mediathek sowie für die iCloud-Ablage.

**Aufbau des Dateinamens:**

```
[Titel] (YYYY).[ext]
```
- **Titel**: Klarer und prägnanter Titel des Films.
- **Jahr**: ISO-Format YYYY.
- **Erweiterung**: Dateiformat, z.B. .mov, .m4v, -poster.jpg.

### Beispiele:

- **Standardfassung:**
  - Paula MTB-Finale Huttwil (2024).mov
  - Paula MTB-Finale Huttwil (2024)-poster.jpg

- **Zusätzliche Fassung:**
  - Paula MTB-Finale Huttwil [Fassung für Verein] (2024).mov
  - Paula MTB-Finale Huttwil [Fassung für Verein] (2024)-poster.jpg

**Hinweise:**

- **Eindeutigkeit**: Jeder Dateiname innerhalb eines Albums und Jahres muss eindeutig sein.
- **Umlaute**: Umlaute und Sonderzeichen sind in den Dateinamen erlaubt.
- **Klarheit**: Die Struktur ermöglicht eine klare Identifikation der Dateien und ihrer Fassungen ohne die Verwendung von Underscores.
- **Standardkonformität**: Das Format [Titel] (YYYY).ext entspricht gängigen Standards von Kinofilmen und Mediatheken weltweit (z.B. IMDb) sowie persönlichen Dokumentenarchiven.
- **Format-Auswahl**: Es gibt niemals gleichzeitig eine QuickTime-Datei und eine M4V-Datei für denselben Film. Bei älteren Familienfilmen kann das Format M4V oder MP4 sein, während bei neueren Filmen fast ausschließlich QuickTime-Dateien verwendet werden.

## 6. Mapping der FCP-Felder zu EXIF-Tags und NFO-Tags

Um die Felder von Final Cut Pro korrekt den EXIF-Tags und NFO-Tags zuzuordnen, wird folgende Zuordnung verwendet:

| FCP Feldname    | EXIF-Tag       | NFO-Tag           | Beschreibung                                       |
|-----------------|----------------|-------------------|---------------------------------------------------|
| Titel           | Title          | `<title>`         | Der Titel des Videos, prominent in Emby angezeigt. |
| Beschreibung    | Description    | `<plot>`, `<description>` | Beschreibung des Videos, z.B. mit Datum und Event. |
| Attribute       | -              | `<tag>`           | Schlüsselwörter zur zusätzlichen Einteilung, z.B. "Apple Log". |
| Ersteller       | Author         | `<author>`, `<producers>` | Autor/Ersteller des Videos, angezeigt unter Produktionsdetails in Emby. |
| Datum           | Creation Date  | `<premiered>`, `<releasedate>`, `<published>`, `<dateadded>` | Aufnahmedatum oder letzter Tag bei Rückblicken. |
| Genre           | Genre          | `<genre>`         | Genre des Videos, immer "Family" für Familienfilme. |
| Kategorie       | -              | `<set>`           | Emby-Sets zur thematischen Gruppierung, z.B. "Vater-Kinder-Tag". |
| Produktion      | Producer       | `<producers>`     | Verantwortlicher für den Videoschnitt, angezeigt in Emby. |
| Regie           | Director       | `<directors>`, `<director>` | Personen, die Originalaufnahmen beigesteuert haben, angezeigt in Emby. |
| Sendung         | Album          | -                 | Album-Tag für Verzeichnishierarchie, nicht in NFO-Datei. |

## 7. Aufnahmedatum

Das Aufnahmedatum eines Videos ist die Grundlage für die Sortierung und Kategorisierung.

### Grundsatz:

- **Einzelereignisse**: Jüngstes Datum der Filmaufnahmen.
- **Rückblicke**: Letzter Tag der Zeitspanne (z.B. 31.12.2023 für einen Jahresrückblick 2019-2023).

## 8. Sets und Tags

**Sets**: Thematische Gruppierungen von Filmen, ähnlich den “Sammlungen” in Emby.

**Beispiele**:

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

**Empfehlung**: Quadratisches Titelbild (poster.png) für zentrale Sammlungen hinzufügen.

**Beispielpfade**:

```
/Volumes/Emby/metadata/collections/Familienausflüge/poster.png
/Volumes/Emby/metadata/collections/Familienausflüge/collection.nfo
```

**Tags**: Zusätzliche Kennzeichnungen für weniger prominente Zusammenhänge.

**Beispiele**:

```xml
<tag>Apple Log</tag>
<tag>Sport</tag>
```

**Kombinierte Nutzung**:

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

## 9. Metadatenverwaltung

### 9.1 Metadaten-Dateien (NFO)

Für jede Mediendatei wird eine NFO-Datei im XML-Format erstellt, die die Metadaten enthält. Diese NFO-Dateien werden ausschließlich in der Emby-Mediathek verwendet und nicht in der iCloud-Ablage abgelegt.

**Struktur der NFO-Datei**:

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<movie>
  <plot><![CDATA[So. 15.09.2025. Start von Paula Gorycka am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.]]></plot>
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
  <director>Patrick Kurmann</director>
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
  <director>Patrick Kurmann</director>
  <director>Silvan Kurmann</director>
  <dateadded>2024-10-26</dateadded>
</movie>
```

**Wichtig**:

- **`<keywords>`**: Befindet sich relativ weit unten in der NFO-Datei, um Verwechslungen mit den `<set>` und `<tag>` Tags zu vermeiden.
- **Produzenten und Direktoren**: Im `<producers>`-Tag der NFO-Datei wird der Produzent erfasst, während die Direktoren in flacher Struktur mit `<director>`-Tags dargestellt werden.

## 10. Integration mit Emby Mediathek

Der Emby-Mediaserver wird so konfiguriert, dass er die NFO-Dateien als Basis für die Metadaten verwendet.

**Konfigurationsschritte**:

1. **Metadatenquelle festlegen**:
   - Pfad zu den Metadaten außerhalb des Familienfilme-Verzeichnisses, z.B.:

```
/Volumes/Emby/metadata/
├── people/
│   ├── p/
│   │   └── [Personenname]/
│   │       ├── [Personenname].nfo
│   │       └── [Personenname]-poster.jpg
│   └── a/
│       └── [Personenname]/
│           ├── [Personenname].nfo
│           └── [Personenname]-poster.jpg
├── collections/
│   └── [Sammlungsname]/
│       ├── collection.nfo
│       └── poster.png
└── ... (weitere Metadatentypen)
```

2. **Emby Einstellungen anpassen**:
   - Navigieren Sie in Emby zu den Bibliothekseinstellungen.
   - Aktivieren Sie die Option, NFO-Dateien als Metadatenquelle zu verwenden.
   - Deaktivieren Sie die automatische Metadatenbeschaffung, falls nur die NFO-Dateien verwendet werden sollen.

3. **Automatische Ergänzungen**:
   - Emby ergänzt eigenständig streamdetails und andere erforderliche Tags.

**Optimale Emby-Integration**:

- **Sets und Tags**: Nutzung von Sets zur thematischen Gruppierung und Tags zur zusätzlichen Kategorisierung.
- **Beteiligte Personen**: Darstellung der Rollen und Beiträge der beteiligten Personen in den Metadaten.
- **Album-Integration**: Alben aus den Final Cut Pro Metadaten werden als übergeordnete Kategorien in Emby verwendet.

## 11. Automatisierung und Skripte

### 11.1 Erstellung der NFO-Dateien

Automatische Generierung der NFO-Dateien zur Minimierung des manuellen Aufwands.

**Workflow**:

1. **Metadaten während des Videoschnitts vergeben**:
   - In Final Cut Pro werden die relevanten Metadaten direkt eingegeben, einschließlich Album und Tags.
2. **Automatische Generierung der NFO-Dateien**:
   - Nach dem Export des Videos wird automatisch eine NFO-Datei anhand der im Videoschnittprogramm hinterlegten Metadaten generiert.
   - Dieses Skript sorgt für konsistente und fehlerfreie Erstellung der NFO-Dateien.
3. **Verschiebung in die Mediathek**:
   - Automatisch generierte NFO-Dateien sowie die Videos werden in die entsprechende Emby-Mediathek verschoben.
   - Emby interpretiert die Metadaten und fügt die Medien in die Bibliothek ein.
4. **Ablage in die iCloud-Mediathek**:
   - Die Videodateien werden in die entsprechende iCloud-Verzeichnisstruktur verschoben, wo sie automatisch synchronisiert werden.
   - **Wichtig**: In der iCloud werden nur die Videodateien abgelegt. Es gibt keine separaten NFO-Dateien oder Titelbild-Dateien. Die Metadaten sind direkt in die Videodateien eingebettet oder als macOS-Tags erfasst, und das Titelbild ist als Adobe RGB eingebettet.

### 11.2 Metadatensynchronisation (Optional)

Erweiterte Implementierung zur Synchronisation von Metadaten zwischen verschiedenen Speicherorten (z.B. Emby und iCloud) durch zusätzliche Skripte oder Tools.

## 12. Automatisierter Workflow

Der automatisierte Workflow umfasst die folgenden Schritte:

1. **Metadaten während des Videoschnitts vergeben**:
   - In Final Cut Pro werden alle relevanten Projektmetadaten eingegeben, einschließlich Album und Tags.
2. **Export des Videos**:
   - Das Video wird aus Final Cut Pro exportiert, wobei die Metadaten eingebettet sind.
3. **Automatische Generierung der NFO-Dateien**:
   - Das Automatisierungsskript liest die Projektmetadaten aus und erstellt die entsprechenden NFO-Dateien.
4. **Konvertierung des Titelbildes**:
   - Das Titelbild wird automatisch konvertiert und in die Videodatei eingebettet.
5. **Ablage in die Emby-Mediathek**:
   - Die Videodateien und NFO-Dateien werden in die Emby-Verzeichnisstruktur verschoben, wodurch Emby die Metadaten einliest und die Medien in die Bibliothek integriert.
6. **Ablage in die iCloud-Mediathek**:
   - Die Videodateien werden in die entsprechende iCloud-Verzeichnisstruktur verschoben, wo sie automatisch synchronisiert werden.
   - **Wichtig**: In der iCloud existieren keine separaten NFO- oder Poster-Dateien. Alle Metadaten sind direkt in die Videodatei eingebettet oder als macOS-Tags erfasst.
7. **Qualitätssicherung**:
   - Das Skript überprüft die Konsistenz und Vollständigkeit der Metadaten sowie die korrekte Ablage der Dateien.

**Vorteile dieses Workflows**:

- **Reduzierter manueller Aufwand**: Minimierung der Handarbeit bei der Metadatenpflege und Dateiverwaltung.
- **Konsistenz**: Sicherstellung, dass alle Metadaten einheitlich und korrekt sind.
- **Automatisierung**: Effiziente Integration neuer Medien in die Emby-Mediathek und iCloud-Ablage.
- **Single-Point-of-Truth**: Emby dient als zentrale Quelle für die Metadaten, während die iCloud-Ablage als Backup und Freigabeplattform fungiert.

## 13. Beteiligte Personen

Beteiligte Personen können in den NFO-Dateien aufgenommen werden, inklusive ihrer Rollen. Dies ermöglicht eine detaillierte Darstellung der Beiträge jeder Person in der Emby-Mediathek.

**Struktur**:

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
```

**Hinweise**:

- **Rollenbezeichnungen**: Statt klassischer Rollen wie „Regisseur“ oder „Produzent“ werden spezifische Rollen wie „Filmaufnahmen“ und „Videoschnitt“ verwendet, um die tatsächlichen Beiträge der Personen widerzuspiegeln.
- **Erweiterbarkeit**: Weitere Personen können hinzugefügt werden, inklusive zusätzlicher Informationen wie Geburtstag.

## 14. Album

**Definition und Zweck**:

Das Album ist eine übergeordnete Kategorie unterhalb des Hauptordners Familienfilme. Albums repräsentieren spezifische Familiengruppen oder thematische Zusammenhänge und dienen der Berechtigungssteuerung sowie der thematischen Organisation der Inhalte.

**Beispiele für Albums**:

- **Familie Kurmann**: Unsere Großfamilie.
- **Familie Kurmann Glück**: Videos von unserer Familie in Lyssach.
- **Kurmann Glück Highlights**: Highlights von unserer Familie in Lyssach, die auch andere Familienmitglieder interessant finden könnten.

**Verzeichnisstruktur mit Album**:

```
/Familienfilme/
└── [Album]/
    └── [Jahr]/
        ├── [Titel] (YYYY).mov
        ├── [Titel] (YYYY).m4v
        └── [Titel] (YYYY)-poster.jpg
```

**Beispiel**:

```
/Familienfilme/
└── Familie Kurmann Glück/
    └── 2024/
        ├── Paula MTB-Finale Huttwil (2024).mov
        ├── Paula MTB-Finale Huttwil (2024).m4v
        └── Paula MTB-Finale Huttwil (2024)-poster.jpg
```

**Integration in Final Cut Pro**:

Albums werden in Final Cut Pro bei den Projektmetadaten hinterlegt. Der automatisierte Workflow liest diese Album-Informationen aus und organisiert die Medien entsprechend sowohl in der Emby-Mediathek als auch in der iCloud-Ablage.

**Zwecke der Albums**:

- **Berechtigungssteuerung**: Bestimmt, welche Videos an welche Familienmitglieder verteilt werden können.
- **Themenhafte Organisation**: Gruppiert Videos thematisch nach Familienmitgliedern oder speziellen Anlässen.

## 15. Automatisierte NFO-Erstellung

Ein Python-Skript (oder ein anderes geeignetes Tool) übernimmt die folgenden Schritte zur Generierung der NFO-Dateien und zur Vorbereitung der Dateien für die iCloud-Ablage:

### 15.1 Generierung der NFO-Datei

1. **Vorlage der NFO-Struktur**:
   - Nutzung der extrahierten Metadaten zur Befüllung der XML-Tags.
   - Einbindung des Aufnahmedatums im `<plot>`-Tag mit dem Wochentagskürzel.
   - Übernahme des Videoschnittdatums in das `<dateadded>`-Tag.
2. **Beispielhafte NFO-Datei**:

```xml
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<movie>
  <plot><![CDATA[So. 15.09.2025. Start von Paula Gorycka am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.]]></plot>
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
  <director>Patrick Kurmann</director>
  <director>Silvan Kurmann</director>
  <dateadded>2024-10-26</dateadded>
</movie>
```

### 15.2 Titelbild-Konvertierung

- **Ausgangslage**:
  - Titelbild in Final Cut Pro: PNG, Adobe RGB Farbraum.
- **Anforderungen**:
  - Konvertierung zu JPG und Adobe RGB.
  - Automatisierte und verlustfreie Konvertierung des Titelbildes.

## 16. Vorteile der Integrierten Spezifikation

- **Maximale Kompatibilität**: Konsistente Nutzung von Metadatenformaten und Dateinamenkonventionen gewährleistet die Nutzbarkeit in Emby und iCloud.
- **Einfaches Teilen und Backup**: iCloud-Ablage dient als zuverlässiges Backup und ermöglicht einfaches Teilen mit Freunden und Familie.
- **Effiziente Metadatenverwaltung**: Nutzung von NFO-Dateien und eingebetteten Metadaten gewährleistet detaillierte und konsistente Metadatenverwaltung.
- **Reduzierter manueller Aufwand**: Automatisierte Skripte minimieren den manuellen Aufwand und erhöhen die Effizienz des Workflows.
- **Klarheit und Struktur**: Einheitliche Verzeichnisstruktur und konsistente Dateinamen erleichtern Verwaltung und Auffindbarkeit der Medien.
- **Erweiterbarkeit**: Modulare Struktur ermöglicht einfache Erweiterung für weitere Medientypen und Anpassungen an zukünftige Anforderungen.

## 17. Fazit

Diese integrierte Spezifikation bietet eine umfassende und effiziente Methode zur Organisation, Verarbeitung und Ablage von Familienfilmen. Durch die Kombination der einzelnen Spezifikationen für Emby, Final Cut Pro und iCloud entsteht ein nahtloser Workflow, der die Verwaltung der Medien erleichtert, die Qualität der Metadaten sicherstellt und gleichzeitig eine flexible Nutzung und Sicherung der Dateien ermöglicht. Die konsistente Struktur und die detaillierten Metadaten fördern eine optimale Darstellung in der Emby-Mediathek und eine ansprechende Ablage in der iCloud, wodurch die Medien sowohl innerhalb als auch außerhalb des Familienkreises leicht zugänglich sind.

