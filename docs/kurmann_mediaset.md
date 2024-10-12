# Kurmann-Medienset Spezifikation

## 1. Überblick

Das **Kurmann-Medienset** ist ein standardisiertes Format zur Organisation und Archivierung von Mediendateien. Jedes Medienset besteht aus mehreren Mediendateien sowie einer Metadaten-Datei (`Metadaten.yaml`). Diese Spezifikation stellt sicher, dass alle Mediensets konsistent benannt und strukturiert sind, um eine einfache Verwaltung und zukünftige Erweiterung zu ermöglichen.

**Version 0.1** fokussiert sich ausschließlich auf **Video**-basierte Mediensets. Weitere Medientypen wie **Dokument**, **Foto** und **Audio** sind vorgesehen und werden in zukünftigen Versionen ergänzt. Alle Medientypen werden gemeinsame obligatorische Eigenschaften besitzen: **Id**, **Titel**, **Typ** und **Datum**.

## 2. Verzeichnisstruktur

Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name das Aufnahmedatum und den Projektnamen enthält.

```yaml
[YYYY-MM-DD]_[Projektname]/
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

## 3. Dateinamenkonventionen

Innerhalb des Medienset-Verzeichnisses sind die Dateien nach ihrem Zweck benannt, was die Identifikation und Verarbeitung der Dateien erleichtert.

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

Die `Metadaten.yaml`-Datei ist eine YAML-Datei, die die wichtigsten Informationen zu einem Medienset speichert. Ein Beispiel ist unten dargestellt, wobei alle Arrays inline dargestellt sind.

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

#### Pflichtfelder

- **Id**: Eindeutige Identifikationsnummer für das Medienset (wird als ULID vergeben).
- **Titel**: Der Titel des Mediensets.
- **Typ**: Haupttyp des Mediensets (z.B. „Video“).
- **Datum**: Das Datum der Aufnahme, im Format YYYY-MM-DD.

#### Optionale Felder

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
