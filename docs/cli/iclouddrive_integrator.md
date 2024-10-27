# `iclouddrive-integrator`

iCloud Integrator

**Usage**:

```console
$ iclouddrive-integrator [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `integrate-homemovie`
* `integrate-homemovies`

## `iclouddrive-integrator integrate-homemovie`

**Usage**:

```console
$ iclouddrive-integrator integrate-homemovie [OPTIONS] VIDEO_FILE ICLOUD_DIR
```

**Arguments**:

* `VIDEO_FILE`: Pfad zur Videodatei, die integriert werden soll.  [required]
* `ICLOUD_DIR`: Das Zielverzeichnis in iCloud.  [required]

**Options**:

* `-i, --title-image FILE`: Pfad zum Titelbild, das integriert werden soll.
* `--overwrite-existing`: Überschreibt bestehende Dateien.
* `--help`: Show this message and exit.

## `iclouddrive-integrator integrate-homemovies`

**Usage**:

```console
$ iclouddrive-integrator integrate-homemovies [OPTIONS] SEARCH_DIR ICLOUD_DIR
```

**Arguments**:

* `SEARCH_DIR`: Das Verzeichnis mit Mediendateien.  [required]
* `ICLOUD_DIR`: Das Zielverzeichnis in iCloud.  [required]

**Options**:

* `-ad, --additional-dir DIRECTORY`: Zusätzliches Verzeichnis.
* `--overwrite-existing`: Überschreibt bestehende Dateien.
* `--help`: Show this message and exit.
