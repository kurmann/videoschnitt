# `apple-compressor-manager`

Apple Compressor Manager CLI

**Usage**:

```console
$ apple-compressor-manager [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `cleanup-prores`: Bereinigt ProRes-Dateien mit einem...
* `compress-prores-file`: Komprimiert eine einzelne ProRes-Datei.
* `compress-prores-files`: Komprimiert ProRes-Dateien in einem...

## `apple-compressor-manager cleanup-prores`

Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant.

**Usage**:

```console
$ apple-compressor-manager cleanup-prores [OPTIONS] HEVC_A_DIR [PRORES_DIR]
```

**Arguments**:

* `HEVC_A_DIR`: Pfad zum HEVC-A-Verzeichnis  [required]
* `[PRORES_DIR]`: Pfad zum ProRes-Verzeichnis

**Options**:

* `--verbose`: Aktiviere detaillierte Ausgaben.
* `--help`: Show this message and exit.

## `apple-compressor-manager compress-prores-file`

Komprimiert eine einzelne ProRes-Datei.

**Usage**:

```console
$ apple-compressor-manager compress-prores-file [OPTIONS] INPUT_FILE
```

**Arguments**:

* `INPUT_FILE`: Pfad zur ProRes-Datei  [required]

**Options**:

* `--output TEXT`: Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll
* `--delete-prores`: Lösche die ProRes-Datei nach erfolgreicher Komprimierung
* `--help`: Show this message and exit.

## `apple-compressor-manager compress-prores-files`

Komprimiert ProRes-Dateien in einem Verzeichnis.

**Usage**:

```console
$ apple-compressor-manager compress-prores-files [OPTIONS] INPUT_DIR
```

**Arguments**:

* `INPUT_DIR`: Pfad zum Quellverzeichnis der ProRes-Dateien  [required]

**Options**:

* `--output TEXT`: Das Verzeichnis, in dem die Ausgabedateien gespeichert werden sollen
* `--delete-prores`: Lösche ProRes-Dateien nach erfolgreicher Komprimierung
* `--help`: Show this message and exit.
