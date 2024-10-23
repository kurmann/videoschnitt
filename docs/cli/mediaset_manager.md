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
* `create-homemovie-metadata-file`: Erstellt die Metadaten.yaml-Datei im...
* `list-mediasets`: Listet alle Mediensets im angegebenen...
* `validate-directory`: Überprüft, ob ein Verzeichnis ein gültiges...
* `validate-mediaset`: Führt sowohl die Metadaten- als auch die...
* `validate-metadata`: Validiert die Metadaten.yaml-Datei gegen...

## `mediaset-manager create-homemovie`

Erstellt ein Medienset-Verzeichnis und eine Metadaten.yaml-Datei.
Dateien werden verschoben und umbenannt. Es erfolgt eine Bestätigung vor dem Verschieben.
Bei bestehenden Dateien wird nachgefragt, ob diese überschrieben werden sollen.

**Usage**:

```console
$ mediaset-manager create-homemovie [OPTIONS]
```

**Options**:

* `--jahr INTEGER`: Jahr des Mediensets
* `--titel TEXT`: Titel des Mediensets
* `-m, --media-server-file PATH`: Pfad zur Medienserver-Videodatei
* `-4k, --internet-4k-file PATH`: Pfad zur 4K-Internet-Videodatei
* `-hd, --internet-hd-file PATH`: Pfad zur HD-Internet-Videodatei
* `-sd, --internet-sd-file PATH`: Pfad zur SD-Internet-Videodatei
* `-p, --projekt-datei PATH`: Pfad zur Projektdatei
* `-t, --titelbild PATH`: Pfad zum Titelbild (Titelbild.png)
* `--metadata-source PATH`: Datei zur Extraktion von Metadaten
* `--typ TEXT`: Typ des Mediensets
* `--untertyp TEXT`: Untertyp des Mediensets (Ereignis/Rückblick)
* `--aufnahmedatum TEXT`: Aufnahmedatum (YYYY-MM-DD) für Untertyp 'Ereignis'
* `--zeitraum TEXT`: Zeitraum für Untertyp 'Rückblick'
* `--beschreibung TEXT`: Beschreibung des Mediensets
* `--notiz TEXT`: Interne Bemerkungen zum Medienset
* `--schluesselwoerter TEXT`: Schlüsselwörter zur Kategorisierung, durch Komma getrennt
* `--album TEXT`: Name des Albums oder der Sammlung
* `--videoschnitt TEXT`: Personen für den Videoschnitt, durch Komma getrennt
* `--kamerafuehrung TEXT`: Personen für die Kameraführung, durch Komma getrennt
* `--dauer-in-sekunden INTEGER`: Gesamtdauer des Films in Sekunden
* `--studio TEXT`: Studio oder Ort der Produktion
* `--filmfassung-name TEXT`: Name der Filmfassung
* `--filmfassung-beschreibung TEXT`: Beschreibung der Filmfassung
* `--help`: Show this message and exit.

## `mediaset-manager create-homemovie-metadata-file`

Erstellt die Metadaten.yaml-Datei im angegebenen Verzeichnis oder im aktuellen Verzeichnis.

**Usage**:

```console
$ mediaset-manager create-homemovie-metadata-file [OPTIONS]
```

**Options**:

* `--titel TEXT`: Titel des Mediensets
* `--jahr INTEGER`: Jahr des Mediensets
* `--typ TEXT`: Typ des Mediensets
* `--untertyp TEXT`: Untertyp des Mediensets (Ereignis/Rückblick)
* `--aufnahmedatum TEXT`: Aufnahmedatum (YYYY-MM-DD) für Untertyp 'Ereignis'
* `--zeitraum TEXT`: Zeitraum für Untertyp 'Rückblick'
* `--beschreibung TEXT`: Beschreibung des Mediensets
* `--notiz TEXT`: Interne Bemerkungen zum Medienset
* `--schluesselwoerter TEXT`: Schlüsselwörter zur Kategorisierung, durch Komma getrennt
* `--album TEXT`: Name des Albums oder der Sammlung
* `--videoschnitt TEXT`: Personen für den Videoschnitt, durch Komma getrennt
* `--kamerafuehrung TEXT`: Personen für die Kameraführung, durch Komma getrennt
* `--dauer-in-sekunden INTEGER`: Gesamtdauer des Films in Sekunden
* `--studio TEXT`: Studio oder Ort der Produktion
* `--filmfassung-name TEXT`: Name der Filmfassung
* `--filmfassung-beschreibung TEXT`: Beschreibung der Filmfassung
* `--metadata-source PATH`: Datei zur Extraktion von Metadaten
* `-o, --output PATH`: Ausgabepfad inklusive Dateiname (z.B., /path/to/Metadaten.yaml)
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
