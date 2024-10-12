# Kurmann-Medienset Spezifikation

## 1. Überblick

Das Kurmann-Medienset ist ein standardisiertes Format zur Organisation und Archivierung von Mediendateien. Jedes Medienset besteht aus mehreren Medien-Dateien sowie einer Metadaten-Datei (Metadaten.yaml). Diese Spezifikation stellt sicher, dass alle Mediensets konsistent benannt und strukturiert sind, um eine einfache Verwaltung und zukünftige Erweiterung zu ermöglichen.

## 2. Verzeichnisstruktur

Jedes Medienset wird in einem eigenen Verzeichnis gespeichert, dessen Name das Aufnahmedatum und den Projektnamen enthält.

```
[YYYY-MM-DD]_[Projektname]/
├── Internet-4K.m4v
├── Internet-HD.m4v
├── Medienserver.mov
├── Master.mov
├── Titelbild.png
├── Projekt.fcpbundle
└── Medienserver.mov.yaml
```

Beispiel:

```
2024-10-10_Leah_will_Krokodil_zeigen/
├── Internet-4K.m4v
├── Internet-HD.m4v
├── Medienserver.mov
├── Master.mov
├── Titelbild.png
├── Projekt.fcpbundle
└── Medienserver.mov.yaml
```

## 3. Dateinamenkonventionen

Innerhalb des Medienset-Verzeichnisses sind die Dateien nach ihrem Zweck benannt. Dies erleichtert die Identifikation und Verarbeitung der Dateien.

- Internet-4K.m4v: Hochauflösende Version für das Internet (4K)
- Internet-HD.m4v: HD-Version für das Internet
- Medienserver.mov: Datei für den Medienserver
- Master.mov: Master-Version des Videos
- Titelbild.png: Titelbild des Mediensets
- Projekt.fcpbundle: Final Cut Pro Projektdatei
- Medienserver.mov.yaml: Metadaten-Datei, die alle relevanten Informationen zum Medienset enthält

## 4. Metadaten-Datei (Metadaten.yaml)

Die Metadaten.yaml-Datei enthält alle relevanten Informationen zu einem Medienset. Diese Datei ermöglicht eine strukturierte Speicherung und einfache Weiterverarbeitung der Metadaten.

### 4.1. Struktur der Metadaten.yaml

Beispielhafte Metadaten.yaml Datei

```yaml
Id: "01JA07850N9398XWPS7CA472XF"
Titel: "Wanderung auf den Napf"
Aufnahmedatum: "2024-10-10"
Beschreibung: "Als ganze Familie auf dem Gipfel das Napfs mit einer prächtigen Rundumsicht in die Alpen und Mittelland."
Copyright: "© Patrick Kurmann 2024"
Veröffentlichungsdatum: "2024-10-12"
Studio: ""
Schlüsselwörter: ["10.10.24"]
Album: "Familie Kurmann-Glück"
Videoschnitt: ["Patrick Kurmann"]
Aufnahmen: ["Patrick Kurmann"]
Dauer_in_Sekunden: 425 
```

### 4.2 Beschreibung der Felder

Feldname	Beschreibung	Typ	Pflichtfeld
Id	Eindeutige Identifikationsnummer für das Medienset.	String	Ja
Titel	Titel des Mediensets.	String	Ja
Aufnahmedatum	Datum der Aufnahme im Format YYYY-MM-DD.	String	Ja
Beschreibung	Detaillierte Beschreibung des Mediensets.	String	Nein
Copyright	Copyright-Informationen.	String	Nein
Veröffentlichungsdatum	Datum der Veröffentlichung im Format YYYY-MM-DD.	String	Nein
Studio	Studio-Informationen.	String	Nein
Schlüsselwörter	Liste von Schlüsselwörtern zur besseren Kategorisierung und Suche.	Liste von Strings	Nein
Album	Album-Name, dem das Medienset zugeordnet ist.	String	Nein
Videoschnitt	Liste der Personen, die für den Videoschnitt verantwortlich sind.	Liste von Strings	Nein
Aufnahmen	Liste der Personen, die an den Aufnahmen beteiligt sind.	Liste von Strings	Nein
Dauer_in_Sekunden	Gesamtdauer des Videos in Sekunden, gerundet.	Integer	Nein
