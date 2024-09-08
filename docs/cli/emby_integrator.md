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
* `convert-image-to-adobe-rgb`: Erstelle Adobe RGB-JPG-Datei
* `convert-images-to-adobe-rgb`: Konvertiere eine Liste von PNG-Bildern in...
* `get-images-for-artwork`: Rufe geeignete Bilder für Artwork aus...
* `list-mediaserver-files`: Liste die Mediaserver-Dateien aus einem...

## `emby-integrator compress-masterfile`

Komprimiere eine Master-Datei.

Diese Methode startet die Kompression der angegebenen Datei und bietet die Möglichkeit, 
nach Abschluss der Komprimierung die Originaldatei zu löschen.

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

Erstelle Adobe RGB-JPG-Datei

**Usage**:

```console
$ emby-integrator convert-image-to-adobe-rgb [OPTIONS] IMAGE_FILE
```

**Arguments**:

* `IMAGE_FILE`: [required]

**Options**:

* `--help`: Show this message and exit.

## `emby-integrator convert-images-to-adobe-rgb`

Konvertiere eine Liste von PNG-Bildern in Adobe RGB, falls eine passende Videodatei existiert.
:param media_dir: Verzeichnis, das sowohl die PNG-Bilder als auch die Videodateien enthält

**Usage**:

```console
$ emby-integrator convert-images-to-adobe-rgb [OPTIONS] MEDIA_DIR
```

**Arguments**:

* `MEDIA_DIR`: [required]

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

## `emby-integrator list-mediaserver-files`

Liste die Mediaserver-Dateien aus einem Verzeichnis auf und gruppiere sie nach Mediensets.

**Usage**:

```console
$ emby-integrator list-mediaserver-files [OPTIONS] SOURCE_DIR
```

**Arguments**:

* `SOURCE_DIR`: [required]

**Options**:

* `--help`: Show this message and exit.
