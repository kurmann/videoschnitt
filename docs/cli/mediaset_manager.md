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
* `integrate-mediaset`: Integriert ein einzelnes Medienset in die...

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

## `mediaset-manager integrate-mediaset`

Integriert ein einzelnes Medienset in die Mediathek. Verschiebt es in das passende Jahresverzeichnis.

**Usage**:

```console
$ mediaset-manager integrate-mediaset [OPTIONS] MEDIENSET_DIR MEDIATHEK_DIR
```

**Arguments**:

* `MEDIENSET_DIR`: Das Medienset-Verzeichnis, das in die Mediathek integriert werden soll.  [required]
* `MEDIATHEK_DIR`: Das Hauptverzeichnis der Mediathek, in das das Medienset integriert werden soll.  [required]

**Options**:

* `-v, --version TEXT`: Versionierungsoption: 'overwrite' oder 'new'.
* `--no-prompt`: Unterdrückt die Nachfrage bei der Integration.
* `--help`: Show this message and exit.
