# Kurmann-Medienset Spezifikation

**Version 0.9 vom 21. Oktober 2024**  
**Autor: Patrick Kurmann**

## 1. Überblick

Ein Kurmann-Medienset ist eine standardisierte Sammlung von Mediendateien, die zu einem bestimmten Ereignis, Thema oder Projekt gehören und in einem eigenen Verzeichnis organisiert sind. Jedes Medienset enthält sowohl die Medien selbst als auch eine strukturierte Metadaten-Datei (`Metadaten.yaml`), welche die wichtigsten Informationen über das Set beschreibt. Die Metadaten-Datei ermöglicht eine effiziente Archivierung, Kategorisierung und zukünftige Verarbeitung der Mediendateien.

### Änderungen gegenüber Version 0.8

- **Einführung eines YAML-Schemas:** Die `Metadaten.yaml`-Dateien werden nun durch ein YAML-Schema validiert, um die Struktur und Konsistenz der Metadaten zu gewährleisten. Kommentare in den YAML-Dateien wurden entfernt, um sie kurz und übersichtlich zu halten. Die Schemas sind im Verzeichnis `schema/medienset/` verfügbar und können über die absoluten URLs referenziert werden:
  - **[Basisschema](https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/basis.yaml)**
  - **[Familienfilm-Schema](https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml)**

- **Anpassungen bei den Mediendateien:** Es wurde klargestellt, dass nur das Titelbild, die `Metadaten.yaml` und mindestens eine Videodatei (entweder vom Typ Internet oder Medienserver) zwingend sind. Obwohl die Videodateien optional sind, muss mindestens eine vorhanden sein.

- **Neuorganisation der Versionierung:** Die Struktur für vorherige Versionen wurde angepasst. Vorherige Versionen werden in einem Unterverzeichnis namens `Vorherige_Version` oder `Vorherige_Versionen` abgelegt. Dies ermöglicht eine bessere Übersichtlichkeit und Organisation der verschiedenen Versionen eines Mediensets.

- **Einführung von Filmfassungs-Metadaten:** Für Familienfilme können nun optionale Attribute `Filmfassung_Name` und `Filmfassung_Beschreibung` in der `Metadaten.yaml` angegeben werden. Wenn das Attribut `Filmfassung_Name` vorhanden ist, muss es ein Unterverzeichnis mit diesem Namen geben (abgeleitet mit Unterstrichen und nur ASCII-Zeichen). Diese beschreiben spezielle Filmfassungen, die in entsprechenden Unterverzeichnissen gespeichert sind.

## 2. Verzeichnisstruktur

Die Verzeichnisstruktur gilt für alle Medienset-Typen. Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name das Jahr und den Titel enthält.

### Allgemeine Struktur des Verzeichnisses

Der Name des Verzeichnisses besteht immer aus dem Jahr und dem Titel des Mediensets. Umlaute und Sonderzeichen werden dabei vermieden, um maximale Kompatibilität mit allen Systemen zu gewährleisten.

```
[YYYY]_[Titel]/
```

**Beispiel:**

```
2024_Wanderung_auf_den_Napf_mit_Uebernachtung/
├── Video-Internet-4K.m4v   (optional, mindestens eine Videodatei erforderlich)
├── Video-Internet-HD.m4v   (optional)
├── Video-Internet-SD.m4v   (optional)
├── Video-Medienserver.mov  (optional)
├── Titelbild.png           (zwingend)
├── Projekt.tar             (optional)
└── Metadaten.yaml          (zwingend)
```

### Hinweise zu Verzeichnis- und Dateinamen

- **Keine Umlaute und Sonderzeichen in Verzeichnis- und Dateinamen:** Verzeichnis- und Dateinamen dürfen nur ASCII-Zeichen enthalten. Umlaute werden durch `ae`, `oe`, `ue` ersetzt, und Sonderzeichen werden weggelassen.

- **Eindeutige Titel pro Jahr:** Innerhalb eines Jahres muss jeder Medienset-Titel eindeutig sein.

## 3. Versionierung und Filmfassungen

### Aktuelle Version

Die aktuelle Version eines Mediensets befindet sich immer direkt im Wurzelverzeichnis des Mediensets oder der jeweiligen Filmfassung. Dies erleichtert den direkten Zugriff, ohne manuell in Unterverzeichnisse navigieren zu müssen. Die Versionsnummer ist im Attribut **`Version`** der `Metadaten.yaml` angegeben.

### Vorherige Versionen

- **Eine vorherige Version:** Wenn es nur eine frühere Version gibt, wird diese in einem Unterverzeichnis namens `Vorherige_Version` abgelegt, welches direkt die Dateien der vorherigen Version enthält.

- **Mehrere vorherige Versionen:** Wenn es mehrere frühere Versionen gibt, werden diese in einem Unterverzeichnis namens `Vorherige_Versionen` abgelegt. Dieses enthält Unterverzeichnisse `Version_X` für jede Version, in denen die Dateien der jeweiligen Version liegen.

### Filmfassungen

Filmfassungen für bestimmte Zielgruppen werden in separaten Unterverzeichnissen organisiert. Wenn in der `Metadaten.yaml` das optionale Attribut `Filmfassung_Name` vorhanden ist, muss es ein entsprechendes Unterverzeichnis mit diesem Namen geben (abgeleitet mit Unterstrichen und nur ASCII-Zeichen).

**Beispiel:**

Attribut `Filmfassung_Name`: "Filmfassung für Familie"  
Unterverzeichnis: `Filmfassung_fuer_Familie/`

**Beispiel für Versionen und Filmfassungen:**

```
2024_Wanderung_auf_den_Napf_mit_Uebernachtung/
├── Filmfassung_fuer_Familie/
│   ├── Video-Internet-4K.m4v
│   ├── Video-Internet-HD.m4v
│   ├── Video-Internet-SD.m4v
│   ├── Video-Medienserver.mov
│   ├── Titelbild.png
│   ├── Projekt.tar
│   ├── Metadaten.yaml
│   └── Vorherige_Version/
│       ├── Video-Internet-4K.m4v
│       ├── Video-Internet-HD.m4v
│       ├── Video-Internet-SD.m4v
│       ├── Video-Medienserver.mov
│       ├── Titelbild.png
│       ├── Projekt.tar
│       └── Metadaten.yaml
├── Video-Internet-4K.m4v
├── Video-Internet-HD.m4v
├── Video-Internet-SD.m4v
├── Video-Medienserver.mov
├── Titelbild.png
├── Projekt.tar
├── Metadaten.yaml
└── Vorherige_Versionen/
    ├── Version_1/
    │   ├── Video-Internet-4K.m4v
    │   ├── Video-Internet-HD.m4v
    │   ├── Video-Internet-SD.m4v
    │   ├── Video-Medienserver.mov
    │   ├── Titelbild.png
    │   ├── Projekt.tar
    │   └── Metadaten.yaml
    └── Version_2/
        ├── Video-Internet-4K.m4v
        ├── Video-Internet-HD.m4v
        ├── Video-Internet-SD.m4v
        ├── Video-Medienserver.mov
        ├── Titelbild.png
        ├── Projekt.tar
        └── Metadaten.yaml
```

## 4. Inhalt des Verzeichnisses pro Medienset-Typ

In dieser Spezifikation wird nur der **Familienfilm**-Medienset-Typ detailliert beschrieben.

### 4.1 Familienfilm-Medienset Struktur

Die folgende Struktur beschreibt den Inhalt eines Familienfilm-basierten Mediensets:

```
[YYYY]_[Titel]/
├── Video-Internet-4K.m4v   (optional, mindestens eine Videodatei erforderlich)
├── Video-Internet-HD.m4v   (optional)
├── Video-Internet-SD.m4v   (optional)
├── Video-Medienserver.mov  (optional)
├── Titelbild.png           (zwingend)
├── Projekt.tar             (optional)
└── Metadaten.yaml          (zwingend)
```

**Wichtig:** Obwohl die Videodateien optional sind, muss **mindestens eine Videodatei** vorhanden sein (entweder `Video-Medienserver.mov` oder eine der `Video-Internet-*.m4v` Dateien).

### Dateinamenkonventionen für Familienfilm-Mediensets

Innerhalb des Familienfilm-Medienset-Verzeichnisses sind die Dateien nach ihrem Zweck benannt, was die Identifikation und Verarbeitung der Dateien erleichtert.

- **Video-Internet-4K.m4v**: Hochauflösende Version für das Internet (4K) (optional)
- **Video-Internet-HD.m4v**: HD-Version für das Internet (optional)
- **Video-Internet-SD.m4v**: SD-Version für das Internet (optional)
- **Video-Medienserver.mov**: Datei für den Medienserver (optional)
- **Titelbild.png**: Titelbild des Mediensets (zwingend)
- **Projekt.tar**: Archivierte Projektdatei (optional)
- **Metadaten.yaml**: Metadaten-Datei, die alle relevanten Informationen zum Medienset enthält (zwingend)

**Hinweis zu Dateitypen:** Abgesehen von der `Metadaten.yaml` sind die Dateitypen nicht vorgegeben. Die Medienserver-Datei kann z. B. auch eine `.mp4` oder `.m4v` sein. Die Projektdatei kann beliebige Formate haben und sollte bei mehreren Dateien archiviert werden.

## 5. Metadaten-Datei (`Metadaten.yaml`)

Die `Metadaten.yaml`-Datei enthält alle relevanten Informationen zu einem Medienset und wird durch ein YAML-Schema validiert. Das Schema definiert die Struktur und die Anforderungen an die Metadaten, einschließlich spezifischer Pflichtfelder für verschiedene Medienset-Typen und Untertypen.

### 5.1 Verwendung des YAML-Schemas

Die `Metadaten.yaml`-Datei verweist auf das entsprechende Schema, welches im Verzeichnis `schema/medienset/` verfügbar ist. Das Schema stellt sicher, dass die Metadaten konsistent und korrekt strukturiert sind.

**Einbindung des Schemas in der `Metadaten.yaml`:**

Am Anfang der `Metadaten.yaml`-Datei wird das Schema mit der absoluten URL referenziert:

```yaml
$schema: "https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml"
```

### 5.2 Beispiel für eine `Metadaten.yaml` für Untertyp "Ereignis"

```yaml
$schema: "https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml"

Spezifikationsversion: "0.9"
Id: "01JA0X08NCSKRSB6VF4C51MEB6"
Titel: "Wanderung auf den Napf mit Übernachtung"
Typ: "Familienfilm"
Untertyp: "Ereignis"
Jahr: "2024"
Aufnahmedatum: "2024-10-10"
Mediatheksdatum: "2024-10-21"
Version: 1
Beschreibung: "Als ganze Familie auf dem Gipfel des Napfs mit einer prächtigen Rundumsicht..."
Notiz: "Erstes Video mit Apple Log-Aufnahme."
Schlüsselwörter:
  - "Touren"
  - "Familie"
Album: "Familie Kurmann-Glück"
Videoschnitt:
  - "Patrick Kurmann"
Kameraführung:
  - "Patrick Kurmann"
Dauer_in_Sekunden: 425
Studio: "Privates Videoschnitt-Studio Lyssach"
Filmfassung_Name: "Filmfassung für Familie"
Filmfassung_Beschreibung: "Mit längerer Szene mit den Vorbereitungen zuhause und der Autofahrt zur Wiggerehütte."
Mediendateien:
  Titelbild: "file:Titelbild.png"
  Video_Medienserver: "file:Video-Medienserver.mov"
```

### 5.3 Beispiel für eine `Metadaten.yaml` für Untertyp "Rückblick"

```yaml
$schema: "https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml"

Spezifikationsversion: "0.9"
Id: "01JAF1DFEWXZX9N9N5PRZ97KC3"
Titel: "Rückblick auf das Jahr 2023"
Typ: "Familienfilm"
Untertyp: "Rückblick"
Jahr: "2023"
Zeitraum: "2023"
Mediatheksdatum: "2024-10-21"
Version: 2
Versionskommentar: "Überarbeitete Fassung mit zusätzlichen Szenen."
Beschreibung: "Ein Rückblick auf die wichtigsten Ereignisse und Projekte des Jahres 2023."
Notiz: "Zusätzliche Szenen aus dem Sommerurlaub hinzugefügt."
Schlüsselwörter:
  - "Rückblick"
  - "Jahresbericht"
Album: "Jahresberichte"
Videoschnitt:
  - "Patrick Kurmann"
Kameraführung:
  - "Patrick Kurmann"
  - "Kathrin Glück"
Dauer_in_Sekunden: 300
Studio: "Privates Videoschnitt-Studio Lyssach"
Mediendateien:
  Titelbild: "file:Titelbild.png"
  Videos_fuer_Internetstreaming:
    HD: "file:Video-Internet-HD.m4v"
    SD: "file:Video-Internet-SD.m4v"
```

**Hinweis zu den Mediendateien:** In diesen Beispielen sind nur die zwingenden Dateien (Titelbild und mindestens eine Videodatei) enthalten. Die Videodateien sind mit dem `file:`-Schema angegeben, sodass sie in unterstützten Editoren direkt geöffnet werden können.

**Wichtig:** In den YAML-Metadaten-Dateien können Unicode-Zeichen, einschließlich Umlauten, verwendet werden. Die Beschränkung auf ASCII-Zeichen gilt nur für Verzeichnis- und Dateinamen.

## 6. Tabellenübersicht der Metadatenfelder

### 6.1 Gemeinsame Eigenschaften für alle Mediensets

| Feldname              | Beschreibung                                                                                                                      | Pflichtfeld |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| Spezifikationsversion | Version der Medienset-Spezifikation, die für das Medienset verwendet wurde (z. B. "0.9").                                         | Ja          |
| Id                    | Eindeutige Identifikationsnummer für das Medienset (ULID).                                                                        | Ja          |
| Titel                 | Titel des Mediensets.                                                                                                             | Ja          |
| Typ                   | Haupttyp des Mediensets, z. B. "Familienfilm", "Audio", "Fotoalbum", "Dokument" etc.                                              | Ja          |
| Jahr                  | Jahr des Mediensets, abgeleitet vom Aufnahmedatum oder Zeitraum.                                                                  | Ja          |
| Mediatheksdatum       | Datum, an dem das Medienset in die Mediathek integriert wurde (Format: "YYYY-MM-DD").                                             | Ja          |
| Beschreibung          | Detaillierte Beschreibung des Mediensets.                                                                                         | Nein        |
| Notiz                 | Interne Bemerkungen zum Medienset.                                                                                                | Nein        |
| Schlüsselwörter       | Liste von Schlüsselwörtern zur Kategorisierung des Mediensets.                                                                    | Nein        |
| Album                 | Name des Albums oder der Sammlung, zu dem das Medienset gehört.                                                                   | Nein        |
| Version               | Versionsnummer des Mediensets (Ganzzahl). Wenn nicht angegeben, wird Version 1 angenommen.                                         | Nein        |
| Versionskommentar     | Kommentar zur Version, z. B. Änderungen oder Anpassungen in dieser Version.                                                       | Nein        |

### 6.2 Gemeinsame Eigenschaften für alle Familienfilme

| Feldname                 | Beschreibung                                                                                                             | Pflichtfeld |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------ | ----------- |
| Untertyp                 | Spezifischer Untertyp des Mediensets, z. B. "Ereignis" oder "Rückblick".                                                 | Ja          |
| Videoschnitt             | Personen, die für den Videoschnitt verantwortlich sind.                                                                  | Nein        |
| Kameraführung            | Personen, die für die Kameraführung verantwortlich sind.                                                                 | Nein        |
| Studio                   | Informationen über das Studio oder den Ort der Produktion.                                                               | Nein        |
| Dauer_in_Sekunden        | Gesamtdauer des Films in Sekunden.                                                                                       | Nein        |
| Filmfassung_Name         | Name der Filmfassung (z. B. "Filmfassung für Familie"). Wenn vorhanden, muss ein entsprechendes Unterverzeichnis existieren. | Nein        |
| Filmfassung_Beschreibung | Beschreibung der Filmfassung.                                                                                            | Nein        |
| Mediendateien            | Informationen zu den Mediendateien, einschließlich Dateipfaden.                                                          | Ja          |

### 6.3 Spezifische Pflichtfelder für den Untertyp "Ereignis"

| Feldname      | Beschreibung                                                                                                         | Pflichtfeld |
| ------------- | -------------------------------------------------------------------------------------------------------------------- | ----------- |
| Aufnahmedatum | Datum der Aufnahme im Format "YYYY-MM-DD". (Bei mehreren Tagen das Datum des letzten Tages)                           | Ja          |

### 6.4 Spezifische Pflichtfelder für den Untertyp "Rückblick"

| Feldname | Beschreibung                                                                                                  | Pflichtfeld |
| -------- | ------------------------------------------------------------------------------------------------------------- | ----------- |
| Zeitraum | Zeitraum des Rückblicks, flexibel im Format (z. B. "2023", "2022-2023", "Januar 2023 bis Dezember 2023").     | Ja          |

### 6.5 Mediendateien

Innerhalb des Feldes `Mediendateien` sind folgende Unterfelder definiert:

| Feldname                      | Beschreibung                                                                | Pflichtfeld |
| ----------------------------- | --------------------------------------------------------------------------- | ----------- |
| Titelbild                     | Titelbild des Mediensets.                                                   | Ja          |
| Video_Medienserver            | Datei für den Medienserver.                                                 | Nein        |
| Videos_fuer_Internetstreaming | Videos für Internetstreaming in verschiedenen Auflösungen (4K, HD, SD).     | Nein        |
| Projektdatei                  | Archivierte Projektdatei.                                                   | Nein        |

**Hinweis:** Mindestens eine Videodatei (entweder `Video_Medienserver` oder `Videos_fuer_Internetstreaming`) muss vorhanden sein.

## 7. Validierung der `Metadaten.yaml`

Die Validierung der `Metadaten.yaml`-Datei erfolgt mithilfe des YAML-Schemas, welches im Verzeichnis `schema/medienset/` verfügbar ist. Durch die Verwendung des Schemas wird sichergestellt, dass die Metadaten vollständig und korrekt sind.

## 8. Fazit

Die Kurmann-Medienset Spezifikation in Version 0.9 bietet eine klare und strukturierte Methode zur Organisation von Mediensets, insbesondere für Familienfilme. Die Einführung eines YAML-Schemas zur Validierung der `Metadaten.yaml`-Dateien erhöht die Konsistenz und erleichtert die Verarbeitung der Metadaten. Durch die klare Definition von Pflichtfeldern und optionalen Elementen wird Flexibilität ermöglicht, ohne die Integrität der Mediensets zu beeinträchtigen.
