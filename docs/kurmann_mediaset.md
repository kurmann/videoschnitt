# Kurmann-Medienset Spezifikation

Version 0.2 vom 12. Oktober 2024. Autor: *Patrick Kurmann*

## 1. Überblick

Ein **Kurmann-Medienset** ist eine standardisierte Sammlung von Mediendateien, die zu einem bestimmten Ereignis, Thema oder Projekt gehören und in einem eigenen Verzeichnis organisiert sind. Jedes Medienset enthält sowohl die Medien selbst als auch eine strukturierte Metadaten-Datei (`Metadaten.yaml`), welche die wichtigsten Informationen über das Set beschreibt. Die Metadaten-Datei ermöglicht eine effiziente Archivierung, Kategorisierung und zukünftige Verarbeitung der Mediendateien.

**Version 0.2** fokussiert sich ausschließlich auf **Video**-basierte Mediensets. Weitere Medientypen wie **Dokument**, **Foto** und **Audio** sind vorgesehen und werden in zukünftigen Versionen ergänzt. Alle Medientypen werden gemeinsame obligatorische Eigenschaften besitzen: **Id**, **Titel**, **Typ** und **Datum**.

### Eigenschaften eines Kurmann-Mediensets:

1. **Ein Verzeichnis pro Medienset**:

   - Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name sich aus dem Aufnahmedatum und dem Projekttitel zusammensetzt.
   - Beispiel: `2024-10-10_10_Wanderung_auf_den_Napf_mit_Übernachtung/`.

2. **Verschiedene Mediendateien**:

   - Ein Medienset enthält mehrere Mediendateien (z.B. Videos in verschiedenen Auflösungen, Titelbilder, Projektdateien), die nach ihrem Zweck benannt sind (z.B. `Medienserver.mov`, `Internet-4K.m4v`, `Titelbild.png`).

3. **Zentrale Metadaten-Datei**:

   - Jede Sammlung enthält eine Datei namens `Metadaten.yaml`, welche die strukturierten Informationen zum Medienset enthält (z.B. Titel, Datum, Dauer des Videos, beteiligte Personen).

4. **Ein Typ pro Medienset**:

   - Ein Medienset hat einen klar definierten Medientyp, wie Video, Foto, Dokument oder Audio. Mischformen gibt es nicht, aber ein Medienset kann verschiedene unterstützende Dateien wie Projekte oder Titelbilder enthalten.

5. **Zentrale Identifikationsnummer (ID)**:

   - Jedes Medienset hat eine eindeutige ID (basierend auf einer ULID), die in der `Metadaten.yaml` gespeichert wird. Diese ID bleibt konstant, auch wenn der Titel oder das Datum des Mediensets geändert wird.

6. **Zukunftssicherheit durch Typen**:

   - Aktuell spezifiziert Version 0.1 Video-basierte Mediensets. In Zukunft sind auch Foto-, Dokument- und Audio-basierte Mediensets vorgesehen, welche jedoch auf der gleichen Struktur basieren. Alle Medientypen teilen sich die grundlegenden Pflichtfelder (ID, Titel, Datum, Typ).

## 2. Verzeichnisstruktur

Die Verzeichnisstruktur gilt für alle Medienset-Typen. Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name das Aufnahmedatum und den Titel enthält.

### Allgemeine Struktur des Verzeichnisses

Der Name des Verzeichnisses besteht immer aus dem Datum und dem Titel des Mediensets:

```yaml
[YYYY-MM-DD]_[Titel]/
```

## 3. Inhalt des Verzeichnisses pro Medienset-Typ

In dieser Spezifikation wird nur der Video-Medienset-Typ beschrieben.

#### 3.1 Video-Medienset Struktur

Die folgende Struktur beschreibt den Inhalt eines Video-basierten Mediensets:

```yaml
[YYYY-MM-DD]_[Titel]/
├── Internet-4K.m4v
├── Internet-HD.m4v
├── Medienserver.mov
├── Master.mov
├── Titelbild.png
├── Projekt.fcpbundle
└── Metadaten.yaml
```

### Beispiel:

```yaml
2024-10-10_10_Wanderung_auf_den_Napf_mit_Übernachtung/
├── Internet-4K.m4v
├── Internet-HD.m4v
├── Medienserver.mov
├── Master.mov
├── Titelbild.png
├── Projekt.fcpbundle
└── Metadaten.yaml
```

#### 3.2. Dateinamenkonventionen für Video-Mediensets

Innerhalb des Video-Medienset-Verzeichnisses sind die Dateien nach ihrem Zweck benannt, was die Identifikation und Verarbeitung der Dateien erleichtert.

- **Internet-4K.m4v**: Hochauflösende Version für das Internet (4K)
- **Internet-HD.m4v**: HD-Version für das Internet
- **Medienserver.mov**: Datei für den Medienserver
- **Master.mov**: Master-Version des Videos
- **Titelbild.png**: Titelbild des Mediensets
- **Projekt.fcpbundle**: Videoschnitt-Projektdatei (z.B. von Final Cut Pro)
- **Metadaten.yaml**: Metadaten-Datei, die alle relevanten Informationen zum Medienset enthält

## 4. Metadaten-Datei (`Metadaten.yaml`)

Die `Metadaten.yaml`-Datei enthält alle relevanten Informationen zu einem Medienset. Diese Datei ermöglicht eine strukturierte Speicherung und einfache Weiterverarbeitung der Metadaten.

### 4.1. Allgemeine Struktur der `Metadaten.yaml`

Die `Metadaten.yaml`-Datei ist für alle Medienset-Typen relevant. Andere Typen wie Dokument, Fotoalbum oder Audio können zusätzliche oder andere Eigenschaften haben.

Die `Metadaten.yaml`-Datei ist eine YAML-Datei, die die wichtigsten Informationen zu einem Medienset speichert. Die Pflichtfelder sind für alle Medienset-Typen bindend, während die optionalen Felder spezifisch für den Video-Typ sind. Ein Beispiel ist unten dargestellt, wobei alle Arrays inline dargestellt sind.

> **Hinweis**: Wenn Double Quotes (`"`) in den Werten verwendet werden, müssen diese entsprechend escaped werden. Zum Beispiel: `"Das ist ein \"Beispiel\" mit Anführungszeichen."` oder alternativ mit Single Quotes (`'`).

```yaml
Id: "01JA0X08NCSKRSB6VF4C51MEB6"
Titel: "Wanderung auf den Napf mit Übernachtung"
Typ: "Video"
Datum: "2024-10-10"
Beschreibung: "Als ganze Familie auf dem Gipfel des Napfs mit einer prächtigen Rundumsicht in die Alpen und das Mittelland einschliesslich Übernachtung im Berghotel."
Copyright: "© Patrick Kurmann 2024"
Veröffentlichungsdatum: "2024-10-12"
Studio: "Privates Videoschnitt-Studio Lyssach"
Schlüsselwörter: ["Touren", "Familie"]
Album: "Familie Kurmann-Glück"
Videoschnitt: ["Patrick Kurmann"]
Aufnahmen: ["Patrick Kurmann", "Kathrin Glück"]
Dauer_in_Sekunden: 425
Untertyp: "Eigenproduktionen"
```

### 4.2. Beschreibung der Felder

#### 4.2.1. Pflichtfelder (für alle Medienset-Typen)

- **Id**: Eindeutige Identifikationsnummer für das Medienset (wird als ULID vergeben).
- **Titel**: Der Titel des Mediensets.
- **Typ**: Haupttyp des Mediensets (z.B. „Video“).
- **Datum**: Das Datum der Aufnahme, im Format YYYY-MM-DD.

#### 4.2.2. Optionale Felder (spezifisch für Video-Mediensets)

- **Beschreibung**: Eine detaillierte Beschreibung des Mediensets.
- **Copyright**: Informationen über den Copyright-Inhaber.
- **Veröffentlichungsdatum**: Das Datum, an dem das Medienset veröffentlicht wurde, im Format YYYY-MM-DD.
- **Studio**: Informationen über das Studio oder den Ort der Produktion (z.B. Privates Videoschnitt-Studio Lyssach).
- **Schlüsselwörter**: Eine Liste von Schlüsselwörtern zur Kategorisierung des Mediensets. Beispiel: `["Touren", "Familie"]`
- **Album**: Der Name des Albums oder der Sammlung, zu dem das Medienset gehört.
- **Videoschnitt**: Eine Liste der Personen, die für den Videoschnitt verantwortlich sind. Beispiel: `["Patrick Kurmann"]`
- **Aufnahmen**: Eine Liste der Personen, die an den Aufnahmen beteiligt waren. Beispiel: `["Patrick Kurmann", "Kathrin Glück"]`
- **Dauer\_in\_Sekunden**: Die Gesamtdauer des Videos in Sekunden (gerundet).
- **Untertyp**: Ein spezifischer Untertyp des Videos, z.B. „Eigenproduktionen“.

## 5. Generierung der `Metadaten.yaml`

Um die `Metadaten.yaml`-Datei automatisch zu erstellen, wird ein Typer-Command bereitgestellt. In Version 0.1 unterstützt der Command ausschließlich Video-basierte Mediensets. Andere Medientypen werden in zukünftigen Versionen unterstützt.

### 5.1. Nutzung des `create-metadata-file` Commands

Um die `Metadaten.yaml`-Datei für ein Video-basiertes Medienset zu generieren, führe folgenden Befehl in deinem Terminal aus:

```bash
mediaset-manager create-metadata-file '/Pfad/zur/Medienserver.mov'
```

Falls die `Metadaten.yaml`-Datei bereits existiert und überschrieben werden soll, übernimmt die CLI die bestehende **Id**.

```
Bestehende ID übernommen.
Metadaten erfolgreich gespeichert.
```

