# Kurmann-Medienset Spezifikation

**Version 0.8 vom 21. Oktober 2024**  
**Autor: Patrick Kurmann**

## 1. Überblick

Ein Kurmann-Medienset ist eine standardisierte Sammlung von Mediendateien, die zu einem bestimmten Ereignis, Thema oder Projekt gehören und in einem eigenen Verzeichnis organisiert sind. Jedes Medienset enthält sowohl die Medien selbst als auch eine strukturierte Metadaten-Datei (`Metadaten.yaml`), welche die wichtigsten Informationen über das Set beschreibt. Die Metadaten-Datei ermöglicht eine effiziente Archivierung, Kategorisierung und zukünftige Verarbeitung der Mediendateien.

Version 0.8 bringt folgende Änderungen:

- **Verzeichnisnamen ohne ISO-Datum:** Das Verzeichnis beginnt nicht mehr mit dem ISO-Datum, sondern besteht aus dem Jahr und dem Titel. Dies berücksichtigt Mediensets, die nur ein Erstellungsjahr haben.
- **Verwendung von "Jahr":** Der Begriff "Jahr" ersetzt "Erstellungsjahr" und wird je nach Medienset-Typ aus dem Aufnahmedatum oder Zeitraum abgeleitet.
- **Eindeutige Titel pro Jahr:** Es darf pro Jahr nur einen eindeutigen Titel pro Medienset geben.
- **Keine Umlaute und Sonderzeichen in Verzeichnis- und Dateinamen:** Zur maximalen Kompatibilität mit allen Systemen dürfen Verzeichnis- und Dateinamen nur ASCII-Zeichen enthalten.
- **Archivierung von Projektdateien:** Wenn die Projektdatei aus mehreren Dateien besteht (z. B. Mac-Bundle), sollte eine Archivdatei wie TAR verwendet werden.
- **Flexible Dateitypen:** Abgesehen von der `Metadaten.yaml` sind die Dateitypen nicht vorgegeben, sondern nur die Namen. Beispielsweise kann die Medienserver-Datei auch eine M4V- oder MP4-Datei sein.
- **Verwendung von `.mov` für Medienserver-Datei:** Die Standarderweiterung für die Medienserver-Datei ist `.mov`, kann aber auch anderweitig sein.
- **Versionierung ohne Unterverzeichnis für aktuelle Version:** Die aktuelle Version eines Mediensets befindet sich direkt im Wurzelverzeichnis. Ältere Versionen können in Unterverzeichnissen organisiert werden.
- **Versionierungskonzept angepasst:** Nur die Versionen, die in der Mediathek behalten werden sollen, werden versioniert. Fehlerhafte Versionen können durch Überschreiben ersetzt werden.
- **Klarstellung zu Unicode in Metadaten:** Die Beschränkung auf ASCII-Zeichen betrifft nur Dateinamen und Verzeichnisnamen. Der Inhalt der `Metadaten.yaml` ist Unicode und darf Umlaute enthalten.

### Eigenschaften eines Kurmann-Mediensets

1. **Ein Verzeichnis pro Medienset**

   - Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name sich aus dem Jahr und dem Projekttitel zusammensetzt.
   - Beispiel: `2024_Wanderung_auf_den_Napf_mit_Uebernachtung/`

2. **Verschiedene Mediendateien**

   - Ein Medienset enthält mehrere Mediendateien (z. B. Familienfilme in verschiedenen Auflösungen, Titelbilder, Projektdateien), die nach ihrem Zweck benannt sind (z. B. `Video-Internet-4K.m4v`, `Video-Medienserver.mov`, `Titelbild.png`).

3. **Zentrale Metadaten-Datei**

   - Jede Sammlung enthält eine Datei namens `Metadaten.yaml`, welche die strukturierten Informationen zum Medienset enthält (z. B. Titel, Jahr, Dauer des Films, beteiligte Personen). Die Metadaten-Datei enthält auch ein Attribut **"Notiz"** für interne Bemerkungen sowie Felder für **Versionierung** und **Versionskommentar**.

4. **Ein Typ pro Medienset**

   - Ein Medienset hat einen klar definierten Medientyp, wie **Familienfilm**, Foto, Dokument oder Audio. Mischformen gibt es nicht, aber ein Medienset kann verschiedene unterstützende Dateien wie Projekte oder Titelbilder enthalten.

5. **Zentrale Identifikationsnummer (ID)**

   - Jedes Medienset hat eine eindeutige ID (basierend auf einer ULID), die in der `Metadaten.yaml` gespeichert wird. Diese ID bleibt konstant, auch wenn der Titel oder das Jahr des Mediensets geändert wird.

6. **Zukunftssicherheit durch Typen**

   - Die aktuelle Version fokussiert sich auf Familienfilm-basierte Mediensets. In Zukunft sind auch Foto-, Dokument- und Audio-basierte Mediensets vorgesehen, welche jedoch auf der gleichen Struktur basieren. Alle Medientypen teilen sich die grundlegenden Pflichtfelder (ID, Titel, Typ, Jahr, Mediatheksdatum). Zusätzliche Pflichtfelder sind abhängig vom jeweiligen Medienset-Untertyp.

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
├── Video-Internet-4K.m4v
├── Video-Internet-HD.m4v
├── Video-Internet-SD.m4v
├── Video-Medienserver.mov
├── Titelbild.png
├── Projekt.tar
└── Metadaten.yaml
```

### Hinweise zu Verzeichnis- und Dateinamen

- **Keine Umlaute und Sonderzeichen:** Verzeichnis- und Dateinamen dürfen nur ASCII-Zeichen enthalten. Umlaute werden durch `ae`, `oe`, `ue` ersetzt, und Sonderzeichen werden weggelassen.
- **Eindeutige Titel pro Jahr:** Innerhalb eines Jahres muss jeder Medienset-Titel eindeutig sein.

### Versionierung und Filmfassungen

**Aktuelle Version:**  
Die aktuelle Version eines Mediensets befindet sich direkt im Wurzelverzeichnis.

**Ältere Versionen:**  
Ältere Versionen werden in Unterverzeichnissen mit dem Namen `Version X` organisiert, wobei `X` für die Versionsnummer steht.

**Filmfassungen:**  
Filmfassungen für bestimmte Zielgruppen oder technisch aufbereitete Versionen werden in separaten Unterverzeichnissen organisiert, z. B. `Filmfassung_fuer_Vereinsmitglieder` oder `Filmfassung_fuer_Familie`. Die aktuelle Fassung ist immer direkt im Wurzelverzeichnis des jeweiligen Filmfassungs-Unterverzeichnisses.

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
│   └── Version 1/
│       └── (ältere Dateien)
├── Video-Internet-4K.m4v
├── Video-Internet-HD.m4v
├── Video-Internet-SD.m4v
├── Video-Medienserver.mov
├── Titelbild.png
├── Projekt.tar
└── Metadaten.yaml
```

## 3. Inhalt des Verzeichnisses pro Medienset-Typ

In dieser Spezifikation wird nur der **Familienfilm**-Medienset-Typ detailliert beschrieben.

### 3.1 Familienfilm-Medienset Struktur

Die folgende Struktur beschreibt den Inhalt eines Familienfilm-basierten Mediensets:

```
[YYYY]_[Titel]/
├── Video-Internet-4K.m4v
├── Video-Internet-HD.m4v
├── Video-Internet-SD.m4v
├── Video-Medienserver.mov
├── Titelbild.png
├── Projekt.tar
└── Metadaten.yaml
```

### Dateinamenkonventionen für Familienfilm-Mediensets

Innerhalb des Familienfilm-Medienset-Verzeichnisses sind die Dateien nach ihrem Zweck benannt, was die Identifikation und Verarbeitung der Dateien erleichtert.

- **Video-Internet-4K.m4v**: Hochauflösende Version für das Internet (4K)
- **Video-Internet-HD.m4v**: HD-Version für das Internet
- **Video-Internet-SD.m4v**: SD-Version für das Internet
- **Video-Medienserver.mov**: Datei für den Medienserver (Dateityp flexibel, z. B. `.mov`, `.mp4`)
- **Titelbild.png**: Titelbild des Mediensets
- **Projekt.tar**: Archivierte Projektdatei (Dateityp flexibel, z. B. `.tar`, `.zip`)
- **Metadaten.yaml**: Metadaten-Datei, die alle relevanten Informationen zum Medienset enthält

**Hinweis zu Dateitypen:** Abgesehen von der `Metadaten.yaml` sind die Dateitypen nicht vorgegeben. Die Medienserver-Datei kann z. B. auch eine `.mp4` oder `.m4v` sein. Die Projektdatei kann beliebige Formate haben und sollte bei mehreren Dateien archiviert werden.

## 4. Metadaten-Datei (`Metadaten.yaml`)

Die `Metadaten.yaml`-Datei enthält alle relevanten Informationen zu einem Medienset. Diese Datei ermöglicht eine strukturierte Speicherung und einfache Weiterverarbeitung der Metadaten.

### 4.1 Allgemeine Struktur der `Metadaten.yaml`

Die `Metadaten.yaml`-Datei ist für alle Medienset-Typen relevant. Es gibt verschiedene optionale Datumsfelder, um den Lebenszyklus des Mediensets abzubilden:

- **Aufnahmedatum**: Das Datum, an dem die Medien aufgenommen wurden (z. B. das Filmdatum eines Familienfilms).
- **Bearbeitungsdatum**: Das Datum, an dem das Medienset zuletzt angepasst wurde, z. B. bei einer Korrektur oder einer neuen Version eines Films.
- **Mediatheksdatum**: Das Datum, an dem das Medienset in die Mediathek aufgenommen wurde. Dieses Feld ist obligatorisch und wird automatisch gesetzt.
- **Version**: Die Versionsnummer des Mediensets (Ganzzahl). Wenn nicht angegeben, wird Version 1 angenommen.
- **Versionskommentar**: Ein Kommentar zur Version, z. B. Änderungen oder Anpassungen in dieser Version.

Zusätzlich gibt es das Attribut **"Notiz"**, das für interne Bemerkungen vorgesehen ist.

**Hinweis zu Umlauten in Metadaten:** Die Beschränkung auf ASCII-Zeichen betrifft nur Dateinamen und Verzeichnisnamen. Der Inhalt der `Metadaten.yaml` ist Unicode und darf Umlaute und Sonderzeichen enthalten.

### Beispiel für eine `Metadaten.yaml`

**Für Untertyp “Ereignis”**:

```yaml
Spezifikationsversion: "0.8"
Id: "01JA0X08NCSKRSB6VF4C51MEB6"
Titel: "Wanderung auf den Napf mit Übernachtung"
Typ: "Familienfilm"
Untertyp: "Ereignis"
Jahr: "2024"
Mediatheksdatum: "2024-10-21"
Aufnahmedatum: "2024-10-10"
Version: 1
Versionskommentar: ""
Beschreibung: "Als ganze Familie auf dem Gipfel des Napfs mit einer prächtigen Rundumsicht in die Alpen und das Mittelland einschließlich Übernachtung im Berghotel."
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
Spezifikationsversion: "0.8"
Id: "01JAF1DFEWXZX9N9N5PRZ97KC3"
Titel: "Rückblick auf das Jahr 2023"
Typ: "Familienfilm"
Untertyp: "Rückblick"
Jahr: "2023"
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

## 5. Versionierungskonzept

### Aktuelle Version

Die aktuelle Version eines Mediensets befindet sich immer direkt im Wurzelverzeichnis des Mediensets oder der jeweiligen Filmfassung. Dies erleichtert den direkten Zugriff, ohne manuell in Unterverzeichnisse navigieren zu müssen. Die Versionsnummer ist im Attribut **"Version"** der `Metadaten.yaml` angegeben.

### Ältere Versionen

Ältere Versionen, die behalten werden sollen, können in Unterverzeichnissen mit dem Namen `Version X` organisiert werden, wobei `X` die Versionsnummer ist. Nicht mehr benötigte oder fehlerhafte Versionen können überschrieben oder gelöscht werden.

### Filmfassungen

Filmfassungen für bestimmte Zielgruppen werden in separaten Unterverzeichnissen organisiert. Auch hier gilt, dass die aktuelle Version direkt im Unterverzeichnis der Filmfassung liegt.

## 6. Tabellenübersicht der Metadatenfelder

### 6.1 Gemeinsame Eigenschaften für alle Mediensets

| Feldname              | Beschreibung                                                                                                                     | Pflichtfeld |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------- | ----------- |
| Spezifikationsversion | Die Version der Medienset-Spezifikation, die für das Medienset verwendet wurde (z. B. "0.8").                                    | Ja          |
| Id                    | Eindeutige Identifikationsnummer für das Medienset (ULID).                                                                       | Ja          |
| Titel                 | Der Titel des Mediensets.                                                                                                        | Ja          |
| Typ                   | Haupttyp des Mediensets, z. B. "Familienfilm", "Audio", "Fotoalbum", "Dokument" etc.                                             | Ja          |
| Jahr                  | Das Jahr des Mediensets, abgeleitet von Aufnahmedatum oder Zeitraum.                                                             | Ja          |
| Mediatheksdatum       | Das Datum, an dem das Medienset in die Mediathek aufgenommen wurde (Format: "YYYY-MM-DD").                                       | Ja          |
| Beschreibung          | Eine detaillierte Beschreibung des Mediensets.                                                                                   | Nein        |
| Notiz                 | Interne Bemerkungen zum Medienset.                                                                                               | Nein        |
| Schlüsselwörter       | Eine Liste von Schlüsselwörtern zur Kategorisierung des Mediensets.                                                              | Nein        |
| Album                 | Der Name des Albums oder der Sammlung, zu dem das Medienset gehört.                                                              | Nein        |
| Version               | Die Versionsnummer des Mediensets (Ganzzahl). Wenn nicht angegeben, wird Version 1 angenommen.                                    | Nein        |
| Versionskommentar     | Ein Kommentar zur Version, z. B. Änderungen oder Anpassungen in dieser Version.                                                  | Nein        |

### 6.2 Gemeinsame Eigenschaften für alle Familienfilme

| Feldname          | Beschreibung                                                               | Pflichtfeld |
| ----------------- | -------------------------------------------------------------------------- | ----------- |
| Videoschnitt      | Eine Liste der Personen, die für den Videoschnitt verantwortlich sind.     | Nein        |
| Kameraführung     | Eine oder mehrere Personen, die für die Kameraführung verantwortlich sind. | Nein        |
| Studio            | Informationen über das Studio oder den Ort der Produktion.                 | Nein        |
| Untertyp          | Spezifischer Untertyp des Mediensets, z. B. "Ereignis" oder "Rückblick".   | Nein        |
| Dauer_in_Sekunden | Die Gesamtdauer des Films in Sekunden.                                     | Nein        |

### 6.3 Spezifische Pflichtfelder für den Untertyp “Ereignis”

| Feldname     | Beschreibung                                                                                          | Pflichtfeld |
| ------------ | ----------------------------------------------------------------------------------------------------- | ----------- |
| Aufnahmedatum | Das genaue Datum der Aufnahme im Format "YYYY-MM-DD".                                                | Ja          |

### 6.4 Spezifische Pflichtfelder für den Untertyp “Rückblick”

| Feldname | Beschreibung                                                                                                | Pflichtfeld |
| -------- | ----------------------------------------------------------------------------------------------------------- | ----------- |
| Zeitraum | Der Zeitraum des Rückblicks, flexibel im Format (z. B. "2023", "2022-2023", "Januar 2023 bis Dezember 2023"). | Ja          |
