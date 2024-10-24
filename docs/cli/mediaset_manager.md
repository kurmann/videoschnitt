# `mediaset-manager`

**Usage**:

```console
$ mediaset-manager [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `create-homemovie`: Erstellt ein Medienset-Verzeichnis und...
* `list-mediasets`: Listet alle Mediensets im angegebenen...
* `validate-directory`: Überprüft, ob ein Verzeichnis ein gültiges...
* `validate-mediaset`: Führt sowohl die Metadaten- als auch die...
* `validate-metadata`: Validiert die Metadaten.yaml-Datei gegen...

## `mediaset-manager create-homemovie`

Erstellt ein Medienset-Verzeichnis und eine Metadaten.yaml-Datei basierend auf einer Videodatei.
Dateien werden gesucht, klassifiziert und in das Medienset-Verzeichnis verschoben.
Es erfolgt eine Bestätigung vor dem Verschieben.
Bei bestehenden Dateien wird nachgefragt, ob diese überschrieben werden sollen.

Prozess:
1. Ermittlung des Titels:
   Der Titel wird aus den Metadaten der Metadatenquelle (Referenzdatei) extrahiert.
   Falls der Titel im Format "YYYY-MM-DD Titel" vorliegt, wird das Datum extrahiert und als 'aufnahmedatum' verwendet.
   Der extrahierte Titel kann durch Angabe von '--titel' überschrieben werden.

2. Sammlung passender Dateien:
   Sucht in Verzeichnis 1 (Verzeichnis der Metadatenquelle) und optional im zusätzlichen Verzeichnis nach Dateien,
   deren Dateinamen mit dem vollständigen Titel (inklusive Datum) beginnen.
   Nur Dateien, die mit dem vollständigen Titel beginnen, werden berücksichtigt.

3. Klassifizierung der Dateien:
   Videodateien werden anhand von Auflösung ('Image Height') und Bitrate ('Avg Bitrate') klassifiziert.
   Medienserver-Datei: Bitrate > 50 Mbps.
   Internet-Dateien:
     - SD: Vertikale Auflösung ≤ 540 Pixel.
     - HD: Vertikale Auflösung = 1080 Pixel.
     - 4K: Vertikale Auflösung ≥ 2048 Pixel.
   Titelbild:
   Bevorzugt wird eine PNG-Datei, die mit dem vollständigen Titel beginnt.
   Falls keine PNG-Datei gefunden wird, wird eine JPG/JPEG-Datei verwendet.

4. Erstellen des Medienset-Verzeichnisses:
   Das Verzeichnis wird nach dem Muster '{Jahr}_{Titel}' erstellt.
   Der Titel wird dabei dateisystemkonform formatiert.

5. Verschieben und Umbenennen der Dateien:
   Die klassifizierten Dateien werden in das Medienset-Verzeichnis verschoben.
   Die Dateien werden gemäß den erwarteten Dateinamen umbenannt.

6. Erstellen der Metadaten.yaml:
   Die Metadaten werden gesammelt und in der 'Metadaten.yaml' im Medienset-Verzeichnis gespeichert.
   Alle Optionen können verwendet werden, um die extrahierten Metadaten zu überschreiben.

Hinweise:
- Vertikale Auflösung und Bitrate werden mit 'exiftool' aus den Dateien ausgelesen.
- Stellen Sie sicher, dass 'exiftool' installiert und im Systempfad verfügbar ist.

**Usage**:

```console
$ mediaset-manager create-homemovie [OPTIONS]
```

**Options**:

* `-ms, --metadata-source PATH`: Pfad zur Videodatei zur Extraktion von Metadaten (Referenzdatei).  [required]
* `-amd, --additional-media-dir PATH`: Zusätzliches Verzeichnis zur Suche nach Mediendateien.
* `--titel TEXT`: Titel des Mediensets.
* `--jahr INTEGER`: Jahr des Mediensets.
* `--untertyp TEXT`: Untertyp des Mediensets (Ereignis/Rückblick).
* `--aufnahmedatum TEXT`: Aufnahmedatum (YYYY-MM-DD) für Untertyp 'Ereignis'.
* `--zeitraum TEXT`: Zeitraum für Untertyp 'Rückblick'.
* `--beschreibung TEXT`: Beschreibung des Mediensets.
* `--notiz TEXT`: Interne Bemerkungen zum Medienset.
* `--schluesselwoerter TEXT`: Schlüsselwörter zur Kategorisierung, getrennt durch Komma oder Semikolon.
* `--album TEXT`: Name des Albums oder der Sammlung.
* `--videoschnitt TEXT`: Personen für den Videoschnitt, getrennt durch Komma oder Semikolon.
* `--kamerafuehrung TEXT`: Personen für die Kameraführung, getrennt durch Komma oder Semikolon.
* `--dauer-in-sekunden INTEGER`: Gesamtdauer des Films in Sekunden.
* `--studio TEXT`: Studio oder Ort der Produktion.
* `--filmfassung-name TEXT`: Name der Filmfassung.
* `--filmfassung-beschreibung TEXT`: Beschreibung der Filmfassung.
* `--help`: Show this message and exit.

## `mediaset-manager list-mediasets`

Listet alle Mediensets im angegebenen Verzeichnis auf und klassifiziert die Dateien.
Optional durchsucht ein zusätzliches Verzeichnis nach Mediendateien.

**Usage**:

```console
$ mediaset-manager list-mediasets [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: Hauptverzeichnis mit den Mediendateien  [required]

**Options**:

* `-a, --additional-dir TEXT`: Optionales zusätzliches Verzeichnis mit Mediendateien
* `--json`: Gibt die Ausgabe als JSON zurück
* `--help`: Show this message and exit.

## `mediaset-manager validate-directory`

Überprüft, ob ein Verzeichnis ein gültiges Medienset ist.

**Usage**:

```console
$ mediaset-manager validate-directory [OPTIONS] DIRECTORY_PATH
```

**Arguments**:

* `DIRECTORY_PATH`: Pfad zum Medienset-Verzeichnis  [required]

**Options**:

* `--help`: Show this message and exit.

## `mediaset-manager validate-mediaset`

Führt sowohl die Metadaten- als auch die Verzeichnisvalidierung durch.

**Usage**:

```console
$ mediaset-manager validate-mediaset [OPTIONS] DIRECTORY_PATH
```

**Arguments**:

* `DIRECTORY_PATH`: Pfad zum Medienset-Verzeichnis  [required]

**Options**:

* `--help`: Show this message and exit.

## `mediaset-manager validate-metadata`

Validiert die Metadaten.yaml-Datei gegen das YAML-Schema.

**Usage**:

```console
$ mediaset-manager validate-metadata [OPTIONS] METADATA_PATH
```

**Arguments**:

* `METADATA_PATH`: Pfad zur Metadaten.yaml Datei  [required]

**Options**:

* `--help`: Show this message and exit.
