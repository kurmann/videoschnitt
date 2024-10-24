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

* `auto-create-homemovies`: Sucht im angegebenen Verzeichnis nach...
* `create-homemovie`: Erstellt ein Medienset-Verzeichnis und...
* `list-mediasets`: Listet alle Mediensets im angegebenen...
* `validate-directory`: Überprüft, ob ein Verzeichnis ein gültiges...
* `validate-mediaset`: Führt sowohl die Metadaten- als auch die...
* `validate-metadata`: Validiert die Metadaten.yaml-Datei gegen...

## `mediaset-manager auto-create-homemovies`

Sucht im angegebenen Verzeichnis nach Mediendateien und erstellt automatisch Mediensets basierend auf den Metadaten.

**Usage**:

```console
$ mediaset-manager auto-create-homemovies [OPTIONS] SEARCH_DIR
```

**Arguments**:

* `SEARCH_DIR`: Das Verzeichnis, in dem nach Mediendateien gesucht werden soll.  [required]

**Options**:

* `-amd, --additional-media-dir DIRECTORY`: Zusätzliches Verzeichnis zur Suche nach Mediendateien.
* `--no-prompt`: Unterdrückt die Nachfrage beim Verschieben der Dateien.
* `--help`: Show this message and exit.

## `mediaset-manager create-homemovie`

Erstellt ein Medienset-Verzeichnis und eine Metadaten.yaml-Datei basierend auf einer Videodatei.
Dateien werden gesucht, klassifiziert und in das Medienset-Verzeichnis verschoben.
Es erfolgt eine Bestätigung vor dem Verschieben, es sei denn, 'no_prompt' wurde angegeben.
Bei bestehenden Dateien wird nachgefragt, ob diese überschrieben werden sollen.

**Usage**:

```console
$ mediaset-manager create-homemovie [OPTIONS] METADATA_SOURCE
```

**Arguments**:

* `METADATA_SOURCE`: [required]

**Options**:

* `--additional-media-dir PATH`
* `--titel TEXT`
* `--jahr INTEGER`
* `--untertyp TEXT`
* `--aufnahmedatum TEXT`
* `--zeitraum TEXT`
* `--beschreibung TEXT`
* `--notiz TEXT`
* `--schluesselwoerter TEXT`
* `--album TEXT`
* `--videoschnitt TEXT`
* `--kamerafuehrung TEXT`
* `--dauer-in-sekunden INTEGER`
* `--studio TEXT`
* `--filmfassung-name TEXT`
* `--filmfassung-beschreibung TEXT`
* `--no-prompt / --no-no-prompt`: [default: no-no-prompt]
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
