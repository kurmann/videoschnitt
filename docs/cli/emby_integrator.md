# `emby-integrator`

FileManager CLI für Emby Integrator

**Usage**:

```console
$ emby-integrator [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `compress-masterfile`: Komprimiere eine Master-Datei.
* `convert-image-to-adobe-rgb`: Konvertiere ein Bild in das Adobe...
* `get-images-for-artwork`: Rufe geeignete Bilder für Artwork aus...
* `get-mediaserver-files`: Rufe die Mediaserver-Dateien aus einem...

## `emby-integrator compress-masterfile`

Komprimiere eine Master-Datei.

**Usage**:

```console
$ emby-integrator compress-masterfile [OPTIONS] INPUT_FILE
```

**Arguments**:

* `INPUT_FILE`: [required]

**Options**:

* `--delete-master-file / --no-delete-master-file`: Lösche die Master-Datei nach der Komprimierung.  [default: no-delete-master-file]
* `--help`: Show this message and exit.

## `emby-integrator convert-image-to-adobe-rgb`

Konvertiere ein Bild in das Adobe RGB-Profil und speichere es als JPEG.

**Usage**:

```console
$ emby-integrator convert-image-to-adobe-rgb [OPTIONS] IMAGE_FILE
```

**Arguments**:

* `IMAGE_FILE`: [required]

**Options**:

* `--help`: Show this message and exit.

## `emby-integrator get-images-for-artwork`

Rufe geeignete Bilder für Artwork aus einem Verzeichnis ab.

**Usage**:

```console
$ emby-integrator get-images-for-artwork [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: [required]

**Options**:

* `--help`: Show this message and exit.

## `emby-integrator get-mediaserver-files`

Rufe die Mediaserver-Dateien aus einem Verzeichnis ab.

**Usage**:

```console
$ emby-integrator get-mediaserver-files [OPTIONS] SOURCE_DIR
```

**Arguments**:

* `SOURCE_DIR`: [required]

**Options**:

* `--help`: Show this message and exit.
