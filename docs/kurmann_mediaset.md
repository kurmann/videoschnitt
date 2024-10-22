# Kurmann-Medienset Spezifikation

**Version 1.0 vom 21. Oktober 2024**  
**Autor: Patrick Kurmann**

## 1. Überblick

Ein Kurmann-Medienset ist eine standardisierte Sammlung von Mediendateien zu einem bestimmten Ereignis, Thema oder Projekt, organisiert in einem eigenen Verzeichnis. Jedes Medienset enthält die Medien selbst sowie eine strukturierte Metadaten-Datei (`Metadaten.yaml`), die die wichtigsten Informationen über das Set beschreibt. Die Metadaten-Datei ermöglicht eine effiziente Archivierung, Kategorisierung und zukünftige Verarbeitung der Mediendateien.

### Änderungen gegenüber Version 0.9

- **Entfernung der `Mediendateien` aus der `Metadaten.yaml`:** Die Mediendateien werden nicht mehr in der `Metadaten.yaml` aufgeführt. Das Dateisystem dient als Single Point of Truth für die Mediendateien. Dies vereinfacht die Struktur und vermeidet Redundanz.
  
- **Klarstellung der Verzeichnisstruktur für Filmfassungen:** Es wurde bestätigt, dass es immer eine Standardfassung gibt, die im Hauptverzeichnis liegt. Zusätzliche Filmfassungen werden in entsprechend benannten Unterverzeichnissen abgelegt, und diese müssen mit dem Attribut `Filmfassung_Name` in den Metadaten übereinstimmen.

- **Vereinfachung der Beispiele:** In den Beispielen für Versionen und Filmfassungen wurden die Inhalte der Unterverzeichnisse abgekürzt, um die Übersichtlichkeit zu verbessern.

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

## 3. Inhalt des Verzeichnisses pro Medienset-Typ

In dieser Spezifikation wird der **Familienfilm**-Medienset-Typ detailliert beschrieben.

### 3.1 Familienfilm-Medienset Struktur

Die folgende Struktur beschreibt den Inhalt eines Familienfilm-Mediensets:

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

**Wichtig:** Mindestens eine Videodatei muss vorhanden sein (entweder `Video-Medienserver.mov` oder eine der `Video-Internet-*.m4v` Dateien).

### Dateinamenkonventionen für Familienfilm-Mediensets

- **Video-Internet-4K.m4v**: Hochauflösende Version für das Internet (4K) (optional)
- **Video-Internet-HD.m4v**: HD-Version für das Internet (optional)
- **Video-Internet-SD.m4v**: SD-Version für das Internet (optional)
- **Video-Medienserver.mov**: Datei für den Medienserver (optional)
- **Titelbild.png**: Titelbild des Mediensets (zwingend)
- **Projekt.tar**: Archivierte Projektdatei (optional)
- **Metadaten.yaml**: Metadaten-Datei mit allen relevanten Informationen (zwingend)

**Hinweis zu Dateitypen:** Abgesehen von der `Metadaten.yaml` sind die Dateitypen nicht vorgegeben.

## 4. Versionierung und Filmfassungen

### Aktuelle Version

Die aktuelle Version eines Mediensets befindet sich immer direkt im Wurzelverzeichnis des Mediensets oder der jeweiligen Filmfassung. Dies erleichtert den direkten Zugriff, ohne in Unterverzeichnisse navigieren zu müssen. Die Versionsnummer ist im Attribut **`Version`** der `Metadaten.yaml` angegeben.

### Vorherige Versionen

- **Eine vorherige Version:** Wenn es nur eine frühere Version gibt, wird diese in einem Unterverzeichnis namens `Vorherige_Version` abgelegt.

- **Mehrere vorherige Versionen:** Bei mehreren früheren Versionen werden diese in einem Unterverzeichnis namens `Vorherige_Versionen` abgelegt. Dieses enthält Unterverzeichnisse `Version_X` für jede Version.

**Beispiel:**

```
Vorherige_Versionen/
├── Version_1/
│   └── (...)  [Inhalt der Version 1]
└── Version_2/
    └── (...)  [Inhalt der Version 2]
```

### Filmfassungen

Es gibt immer eine Standardfassung, die direkt im Hauptverzeichnis des Mediensets liegt. Filmfassungen für bestimmte Zielgruppen werden in separaten Unterverzeichnissen organisiert. Wenn in der `Metadaten.yaml` das optionale Attribut `Filmfassung_Name` vorhanden ist, muss ein entsprechendes Unterverzeichnis mit diesem Namen existieren (abgeleitet mit Unterstrichen und nur ASCII-Zeichen).

**Beispiel:**

Attribut `Filmfassung_Name`: "Filmfassung für Familie"  
Unterverzeichnis: `Filmfassung_fuer_Familie/`

**Beispiel für Versionen und Filmfassungen:**

```
2024_Wanderung_auf_den_Napf_mit_Uebernachtung/
├── Filmfassung_fuer_Familie/
│   ├── Video-Internet-4K.m4v
│   ├── ...
│   └── Vorherige_Version/
│       └── (...)  [Inhalt der vorherigen Version]
├── Video-Internet-4K.m4v
├── ...
└── Vorherige_Versionen/
    ├── Version_1/
    │   └── (...)  [Inhalt der Version 1]
    └── Version_2/
        └── (...)  [Inhalt der Version 2]
```

## 5. Metadaten-Datei (`Metadaten.yaml`)

Die `Metadaten.yaml`-Datei enthält alle relevanten Informationen zu einem Medienset und wird durch ein YAML-Schema validiert. Das Schema definiert die Struktur und die Anforderungen an die Metadaten, einschließlich spezifischer Pflichtfelder für verschiedene Medienset-Typen und Untertypen.

### 5.1 Verwendung des YAML-Schemas

Die `Metadaten.yaml`-Datei verweist auf das entsprechende Schema, welches über die absoluten URLs verfügbar ist:

```yaml
$schema: "https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml"
```

### 5.2 Beispiel für eine `Metadaten.yaml` für Untertyp "Ereignis"

```yaml
$schema: "https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml"

Spezifikationsversion: "1.0"
Id: "01JA0X08NCSKRSB6VF4C51MEB6"
Titel: "Wanderung auf den Napf mit Übernachtung"
Typ: "Familienfilm"
Untertyp: "Ereignis"
Jahr: "2024"
Aufnahmedatum: "2024-10-10"
Mediatheksdatum: "2024-10-21"
Version: 1
Beschreibung: "Als ganze Familie auf dem Gipfel des Napfs mit einer prächtigen Rundumsicht..."
Notiz: "Überarbeitung der Titelanimation."
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
Filmfassung_Beschreibung: "Mit längerer Szene mit den Kindervelos auf dem Parcours."
```

### 5.3 Beispiel für eine `Metadaten.yaml` für Untertyp "Rückblick"

```yaml
$schema: "https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml"

Spezifikationsversion: "1.0"
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
```

**Hinweis:** In den `Metadaten.yaml`-Dateien werden keine Mediendateien mehr aufgeführt. Die Mediendateien sind im Dateisystem organisiert und folgen den in der Spezifikation festgelegten Konventionen.

## 6. Tabellenübersicht der Metadatenfelder

### 6.1 Gemeinsame Eigenschaften für alle Mediensets

| Feldname              | Beschreibung                                                                                                                      | Pflichtfeld |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| Spezifikationsversion | Version der Medienset-Spezifikation, die für das Medienset verwendet wurde (z. B. "1.0").                                         | Ja          |
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

### 6.3 Spezifische Pflichtfelder für den Untertyp "Ereignis"

| Feldname      | Beschreibung                                                                                                         | Pflichtfeld |
| ------------- | -------------------------------------------------------------------------------------------------------------------- | ----------- |
| Aufnahmedatum | Datum der Aufnahme im Format "YYYY-MM-DD". (Bei mehreren Tagen das Datum des letzten Tages)                           | Ja          |

### 6.4 Spezifische Pflichtfelder für den Untertyp "Rückblick"

| Feldname | Beschreibung                                                                                                  | Pflichtfeld |
| -------- | ------------------------------------------------------------------------------------------------------------- | ----------- |
| Zeitraum | Zeitraum des Rückblicks, flexibel im Format (z. B. "2023", "2022-2023", "Januar 2023 bis Dezember 2023").     | Ja          |

## 7. Validierung der `Metadaten.yaml`

Die Validierung der `Metadaten.yaml`-Datei erfolgt mithilfe des YAML-Schemas, welches über die absoluten URLs verfügbar ist. Durch die Verwendung des Schemas wird sichergestellt, dass die Metadaten vollständig und korrekt sind.

## 8. Fazit

Die Kurmann-Medienset Spezifikation in Version 1.0 bietet eine klare und strukturierte Methode zur Organisation von Mediensets, insbesondere für Familienfilme. Durch die Vereinfachungen und die Fokussierung auf das Dateisystem als Single Point of Truth für Mediendateien wird die Verwaltung erleichtert und Redundanz vermieden.

## Anhang: YAML-Schema-Dateien

Die YAML-Schema-Dateien sind über die folgenden absoluten URLs verfügbar:

- **[Basisschema](https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/basis.yaml)**
- **[Familienfilm-Schema](https://raw.githubusercontent.com/kurmann/videoschnitt/refs/heads/main/docs/schema/medienset/familienfilm.yaml)**

## 9. Verwendung der Schemas

Um die `Metadaten.yaml`-Dateien zu validieren, können Sie einen YAML-Editor oder ein Tool verwenden, das JSON Schema unterstützt. Durch den Verweis auf das Schema am Anfang der `Metadaten.yaml`-Datei wird die Validierung automatisch ermöglicht.
