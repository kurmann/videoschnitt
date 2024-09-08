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

Diese Methode durchsucht das angegebene Verzeichnis nach Videodateien und zugehörigen Titelbildern 
und gruppiert diese nach Mediensets. Falls das Flag `--json-output` gesetzt wird, wird die Ausgabe 
im JSON-Format zurückgegeben, andernfalls wird eine menschenlesbare Ausgabe erstellt, die die 
Informationen bündig darstellt.

Args:
    source_dir (str): Der Pfad zu dem Verzeichnis, das nach Mediendateien und Bildern durchsucht wird.
    json_output (bool): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

Returns:
    None: Gibt die Mediensets in einer menschenlesbaren Form oder als JSON zurück, je nach dem Wert von `json_output`.

Beispiel:
    $ emby-integrator list-mediaserver-files /path/to/mediadirectory

    Ausgabe:
    Medienset: 2024-08-27 Ann-Sophie Spielsachen Bett
    Videos:    2024-08-27 Ann-Sophie Spielsachen Bett.mov
    Titelbild: Kein Titelbild gefunden.
    ----------------------------------------
    Medienset: Ann-Sophie rennt (Testvideo)
    Videos:    Ann-Sophie rennt (Testvideo)-4K60-Medienserver.mov
    Titelbild: Ann-Sophie rennt (Testvideo).jpg
    ----------------------------------------

Raises:
    FileNotFoundError: Wenn das angegebene Verzeichnis nicht existiert.

**Usage**:

```console
$ emby-integrator list-mediaserver-files [OPTIONS] SOURCE_DIR
```

**Arguments**:

* `SOURCE_DIR`: [required]

**Options**:

* `--json-output / --no-json-output`: Gebe die Ausgabe im JSON-Format aus  [default: no-json-output]
* `--help`: Show this message and exit.
