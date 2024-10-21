# Kurmann-Medienset Spezifikation

**Version 0.7 vom 21. Oktober 2024**  
**Autor: Patrick Kurmann**

## 1. Überblick

Ein Kurmann-Medienset ist eine standardisierte Sammlung von Mediendateien, die zu einem bestimmten Ereignis, Thema oder Projekt gehören und in einem eigenen Verzeichnis organisiert sind. Jedes Medienset enthält sowohl die Medien selbst als auch eine strukturierte Metadaten-Datei (`Metadaten.yaml`), welche die wichtigsten Informationen über das Set beschreibt. Die Metadaten-Datei ermöglicht eine effiziente Archivierung, Kategorisierung und zukünftige Verarbeitung der Mediendateien.

Version 0.7 erweitert die Spezifikation um Anpassungen bei den Dateinamen, neue Metadatenfelder für Notizen und Versionierung sowie eine erweiterte Verzeichnisstruktur für Versionen und Filmfassungen. Der Medientyp **"Familienfilm"** wird neu eingeführt, welcher eine Spezifizierung des Typs "Video" darstellt. Diese Änderungen verbessern die Organisation und Handhabung von Mediensets, insbesondere für verschiedene Filmversionen und -fassungen.

### Eigenschaften eines Kurmann-Mediensets

1. **Ein Verzeichnis pro Medienset**

   - Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name sich aus dem Aufnahmedatum und dem Projekttitel zusammensetzt.
   - Beispiel: `2024-10-10_Wanderung_auf_den_Napf_mit_Übernachtung/`

2. **Verschiedene Mediendateien**

   - Ein Medienset enthält mehrere Mediendateien (z. B. Familienfilme in verschiedenen Auflösungen, Titelbilder, Projektdateien), die nach ihrem Zweck benannt sind (z. B. `Video-Internet-4K.m4v`, `Video-Medienserver.mov`, `Titelbild.png`).

3. **Zentrale Metadaten-Datei**

   - Jede Sammlung enthält eine Datei namens `Metadaten.yaml`, welche die strukturierten Informationen zum Medienset enthält (z. B. Titel, Datum, Dauer des Films, beteiligte Personen). Die Metadaten-Datei enthält nun auch ein Attribut **"Notiz"** für interne Bemerkungen sowie Felder für **Versionierung** und **Versionskommentar**.

4. **Ein Typ pro Medienset**

   - Ein Medienset hat einen klar definierten Medientyp, wie **Familienfilm**, Foto, Dokument oder Audio. Mischformen gibt es nicht, aber ein Medienset kann verschiedene unterstützende Dateien wie Projekte oder Titelbilder enthalten.

5. **Zentrale Identifikationsnummer (ID)**

   - Jedes Medienset hat eine eindeutige ID (basierend auf einer ULID), die in der `Metadaten.yaml` gespeichert wird. Diese ID bleibt konstant, auch wenn der Titel oder das Datum des Mediensets geändert wird.

6. **Zukunftssicherheit durch Typen**

   - Die aktuelle Version fokussiert sich auf Familienfilm-basierte Mediensets. In Zukunft sind auch Foto-, Dokument- und Audio-basierte Mediensets vorgesehen, welche jedoch auf der gleichen Struktur basieren. Alle Medientypen teilen sich die grundlegenden Pflichtfelder (ID, Titel, Typ, Erstellung, Mediatheksdatum). Zusätzliche Pflichtfelder sind abhängig vom jeweiligen Medienset-Untertyp.

## 2. Verzeichnisstruktur

Die Verzeichnisstruktur gilt für alle Medienset-Typen. Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name das Aufnahmedatum und den Titel enthält.

### Allgemeine Struktur des Verzeichnisses

Der Name des Verzeichnisses besteht immer aus dem Datum und dem Titel des Mediensets:

```
[YYYY-MM-DD]_[Titel]/
```

**Beispiel:**

```
2024-10-10_Wanderung_auf_den_Napf_mit_Übernachtung/
├── Video-Internet-4K.m4v
├── Video-Internet-HD.m4v
├── Video-Internet-SD.m4v
├── Video-Medienserver.mov
├── Titelbild.png
├── Projekt.fcpbundle
└── Metadaten.yaml
```

### Versionierung und Filmfassungen

**Versionen:**  
Versionen eines Mediensets werden in Unterverzeichnissen mit dem Namen `Version X` organisiert, wobei `X` für die Versionsnummer steht. Wenn keine Version angegeben ist, wird Version 1 angenommen und kein Unterverzeichnis erstellt.

**Filmfassungen:**  
Filmfassungen für bestimmte Zielgruppen oder technisch aufbereitete Versionen werden in separaten Unterverzeichnissen organisiert, z. B. `/Filmfassung für Vereinsmitglieder` oder `/Filmfassung für Familie`. Diese können in seltenen Fällen ebenfalls in Versionen unterteilt werden.

**Beispiel für Versionen und Filmfassungen:**

```
2024-10-10_Wanderung_auf_den_Napf_mit_Übernachtung/
├── Filmfassung für Familie/
│   ├── Version 1/
│   │   ├── Video-Internet-4K.m4v
│   │   ├── Video-Internet-HD.m4v
│   │   ├── Video-Internet-SD.m4v
│   │   ├── Video-Medienserver.mov
│   │   ├── Titelbild.png
│   │   ├── Projekt.fcpbundle
│   │   └── Metadaten.yaml
│   └── Version 2/
│       └── ...
└── Metadaten.yaml
```

## 3. Inhalt des Verzeichnisses pro Medienset-Typ

In dieser Spezifikation wird nur der **Familienfilm**-Medienset-Typ detailliert beschrieben.

### 3.1 Familienfilm-Medienset Struktur

Die folgende Struktur beschreibt den Inhalt eines Familienfilm-basierten Mediensets:

```
[YYYY-MM-DD]_[Titel]/
├── Video-Internet-4K.m4v
├── Video-Internet-HD.m4v
├── Video-Internet-SD.m4v
├── Video-Medienserver.mov
├── Titelbild.png
├── Projekt.fcpbundle
└── Metadaten.yaml
```

### Dateinamenkonventionen für Familienfilm-Mediensets

Innerhalb des Familienfilm-Medienset-Verzeichnisses sind die Dateien nach ihrem Zweck benannt, was die Identifikation und Verarbeitung der Dateien erleichtert.

- **Video-Internet-4K.m4v**: Hochauflösende Version für das Internet (4K)
- **Video-Internet-HD.m4v**: HD-Version für das Internet
- **Video-Internet-SD.m4v**: SD-Version für das Internet (neu hinzugekommen)
- **Video-Medienserver.mov**: Datei für den Medienserver
- **Titelbild.png**: Titelbild des Mediensets
- **Projekt.fcpbundle**: Videoschnitt-Projektdatei (z. B. von Final Cut Pro)
- **Metadaten.yaml**: Metadaten-Datei, die alle relevanten Informationen zum Medienset enthält

## 4. Metadaten-Datei (`Metadaten.yaml`)

Die `Metadaten.yaml`-Datei enthält alle relevanten Informationen zu einem Medienset. Diese Datei ermöglicht eine strukturierte Speicherung und einfache Weiterverarbeitung der Metadaten.

### 4.1 Allgemeine Struktur der `Metadaten.yaml`

Die `Metadaten.yaml`-Datei ist für alle Medienset-Typen relevant. Es gibt verschiedene optionale Datumsfelder, um den Lebenszyklus des Mediensets abzubilden:

- **Aufnahmedatum**: Das Datum, an dem die Medien aufgenommen wurden (z. B. das Filmdatum eines Familienfilms).
- **Erstellungsdatum**: Das Datum, an dem das Medienset zusammengestellt und der Videoschnitt abgeschlossen wurde.
- **Bearbeitungsdatum**: Das Datum, an dem das Medienset zuletzt angepasst wurde, z. B. bei einer Korrektur oder einer neuen Version eines Films.
- **Mediatheksdatum**: Das Datum, an dem das Medienset in die Mediathek aufgenommen wurde. Dieses Feld ist obligatorisch und wird automatisch gesetzt.
- **Version**: Die Versionsnummer des Mediensets (Ganzzahl). Wenn nicht angegeben, wird Version 1 angenommen.
- **Versionskommentar**: Ein Kommentar zur Version, z. B. Änderungen oder Anpassungen in dieser Version.

Zusätzlich gibt es das neue Attribut **"Notiz"**, das für interne Bemerkungen vorgesehen ist.

### Beispiel für eine `Metadaten.yaml`

**Für Untertyp “Ereignis”**:

```yaml
Spezifikationsversion: "0.7"
Id: "01JA0X08NCSKRSB6VF4C51MEB6"
Titel: "Wanderung auf den Napf mit Übernachtung"
Typ: "Familienfilm"
Untertyp: "Ereignis"
Erstellung: "2024"
Mediatheksdatum: "2024-10-21"
Aufnahmedatum: "2024-10-10"
Version: 1
Versionskommentar: ""
Beschreibung: "Als ganze Familie auf dem Gipfel des Napfs mit einer prächtigen Rundumsicht in die Alpen und das Mittelland einschliesslich Übernachtung im Berghotel."
Notiz: "Interne Bemerkung: Erste Schnittversion."
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
```

**Für Untertyp “Rückblick”**:

```yaml
Spezifikationsversion: "0.7"
Id: "01JAF1DFEWXZX9N9N5PRZ97KC3"
Titel: "Rückblick auf das Jahr 2023"
Typ: "Familienfilm"
Untertyp: "Rückblick"
Erstellung: "2023"
Mediatheksdatum: "2024-10-21"
Zeitraum: "2023"
Version: 2
Versionskommentar: "Überarbeitete Fassung mit zusätzlichen Szenen."
Beschreibung: "Ein Rückblick auf die wichtigsten Ereignisse und Projekte des Jahres 2023."
Notiz: "Interne Bemerkung: Zusätzliche Szenen aus dem Sommerurlaub hinzugefügt."
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

## 6. Tabellenübersicht der Metadatenfelder

### 6.1 Gemeinsame Eigenschaften für alle Mediensets

| Feldname              | Beschreibung                                                                                                                     | Pflichtfeld |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| Spezifikationsversion | Die Version der Medienset-Spezifikation, die für das Medienset verwendet wurde (z. B. "0.7").                                    | Ja          |
| Id                    | Eindeutige Identifikationsnummer für das Medienset (ULID).                                                                       | Ja          |
| Titel                 | Der Titel des Mediensets.                                                                                                        | Ja          |
| Typ                   | Haupttyp des Mediensets, z. B. "Familienfilm", "Audio", "Fotoalbum", "Dokument" etc.                                             | Ja          |
| Erstellung            | Das Jahr oder das genaue Datum der Erstellung des Mediensets (Format: "YYYY" oder "YYYY-MM-DD").                                 | Ja          |
| Mediatheksdatum       | Das Datum, an dem das Medienset in die Mediathek aufgenommen wurde (Format: "YYYY-MM-DD").                                       | Ja          |
| Beschreibung          | Eine detaillierte Beschreibung des Mediensets.                                                                                   | Nein        |
| Notiz                 | Interne Bemerkungen zum Medienset.                                                                                               | Nein        |
| Schlüsselwörter       | Eine Liste von Schlüsselwörtern zur Kategorisierung des Mediensets.                                                              | Nein        |
| Album                 | Der Name des Albums oder der Sammlung, zu dem das Medienset gehört.                                                              | Nein        |
| Version               | Die Versionsnummer des Mediensets (Ganzzahl). Wenn nicht angegeben, wird Version 1 angenommen.                                    | Nein        |
| Versionskommentar     | Ein Kommentar zur Version, z. B. Änderungen oder Anpassungen in dieser Version.                                                  | Nein        |

### 6.2 Gemeinsame Eigenschaften für alle Familienfilme

| Feldname            | Beschreibung                                                                       | Pflichtfeld |
| ------------------- | ---------------------------------------------------------------------------------- | ----------- |
| Videoschnitt        | Eine Liste der Personen, die für den Videoschnitt verantwortlich sind.             | Nein        |
| Kameraführung       | Eine oder mehrere Personen, die für die Kameraführung verantwortlich sind.         | Nein        |
| Studio              | Informationen über das Studio oder den Ort der Produktion.                         | Nein        |
| Untertyp            | Spezifischer Untertyp des Mediensets, z. B. "Ereignis" oder "Rückblick".           | Nein        |
| Dauer_in_Sekunden   | Die Gesamtdauer des Films in Sekunden.                                             | Nein        |

### 6.3 Spezifische Pflichtfelder für den Untertyp “Ereignis”

| Feldname                     | Beschreibung                                                                                                                                         | Pflichtfeld |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| Aufnahmejahr / Aufnahmedatum | Das Jahr, in dem die Aufnahme stattgefunden hat oder das genaue Datum der Aufnahme im Format "YYYY-MM-DD". (Eines der beiden Felder ist obligatorisch) | Ja          |

### 6.4 Spezifische Pflichtfelder für den Untertyp “Rückblick”

| Feldname | Beschreibung                                                                                                               | Pflichtfeld |
| -------- | -------------------------------------------------------------------------------------------------------------------------- | ----------- |
| Zeitraum | Der Zeitraum des Rückblicks, flexibel im Format (z. B. "2023", "2022-2023", "Januar 2023 bis Dezember 2023").              | Ja          |
