# `emby-integrator`

Emby Integrator

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
* `convert-images-to-adobe-rgb`: Konvertiere eine Liste von PNG-Bildern in...
* `generate-nfo-xml`: Generiert die NFO-Metadatendatei und gibt...
* `get-recording-date`: Gibt das Aufnahmedatum aus dem Dateinamen...
* `list-mediaserver-files`: Liste die Mediaserver-Dateien aus einem...
* `list-metadata`: Extrahiere die Metadaten aus einer Datei...
* `write-nfo-file`: Generiert die NFO-Metadatendatei und...

## `emby-integrator compress-masterfile`

Komprimiere eine Master-Datei.

Diese Methode startet die Kompression der angegebenen Datei und bietet die Möglichkeit, 
nach Abschluss der Komprimierung die Originaldatei zu löschen.

Args:
    input_file (str): Der Pfad zur Master-Datei, die komprimiert werden soll.
    delete_master_file (bool): Optional. Gibt an, ob die Master-Datei nach der Komprimierung gelöscht werden soll. Standard ist `False`.

Returns:
    None

Beispiel:
    $ emby-integrator compress-masterfile /path/to/masterfile.mov --delete-master-file

**Usage**:

```console
$ emby-integrator compress-masterfile [OPTIONS] INPUT_FILE
```

**Arguments**:

* `INPUT_FILE`: [required]

**Options**:

* `--delete-master-file / --no-delete-master-file`: Lösche die Master-Datei nach der Komprimierung.  [default: no-delete-master-file]
* `--help`: Show this message and exit.

## `emby-integrator convert-images-to-adobe-rgb`

Konvertiere eine Liste von PNG-Bildern in Adobe RGB, falls eine passende Videodatei existiert.

Diese Methode durchsucht das angegebene Verzeichnis nach PNG-Bildern und konvertiert sie in das 
Adobe RGB-Farbprofil, falls eine passende Videodatei im gleichen Verzeichnis existiert.

Args:
    media_dir (str): Der Pfad zu dem Verzeichnis, das nach PNG-Bildern und passenden Videodateien durchsucht wird.

Returns:
    None

Beispiel:
    $ emby-integrator convert-images-to-adobe-rgb /path/to/mediadirectory

**Usage**:

```console
$ emby-integrator convert-images-to-adobe-rgb [OPTIONS] MEDIA_DIR
```

**Arguments**:

* `MEDIA_DIR`: [required]

**Options**:

* `--help`: Show this message and exit.

## `emby-integrator generate-nfo-xml`

Generiert die NFO-Metadatendatei und gibt das XML aus.

Args:
    file_path (str): Pfad zur Videodatei.

**Usage**:

```console
$ emby-integrator generate-nfo-xml [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: [required]

**Options**:

* `--help`: Show this message and exit.

## `emby-integrator get-recording-date`

Gibt das Aufnahmedatum aus dem Dateinamen in einem deutschen Datumsformat mit Wochentag aus.

Args:
    file_path (str): Pfad zur Datei, deren Aufnahmedatum im Dateinamen enthalten sein soll.

**Usage**:

```console
$ emby-integrator get-recording-date [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: [required]

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

## `emby-integrator list-metadata`

Extrahiere die Metadaten aus einer Datei und gebe sie aus.

Diese Methode extrahiert relevante Metadaten wie Dateiname, Größe, Erstellungsdatum, Dauer, Videoformat 
und andere Informationen aus der Datei mithilfe von ExifTool. Falls das Flag `--json-output` gesetzt wird, 
wird die Ausgabe im JSON-Format zurückgegeben.

Args:
    file_path (str): Der Pfad zur Datei, aus der die Metadaten extrahiert werden sollen.
    json_output (bool): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

Returns:
    None: Gibt die extrahierten Metadaten in einer menschenlesbaren Form oder als JSON zurück, je nach dem Wert von `json_output`.

Beispiel:
    $ emby-integrator get-metadata /path/to/video.mov

    Ausgabe:
    FileName: video.mov
    Directory: /path/to
    FileSize: 123456 bytes
    FileModificationDateTime: 2024-08-10 10:30:00
    ...

Raises:
    FileNotFoundError: Wenn die angegebene Datei nicht existiert.
    ValueError: Wenn keine Metadaten extrahiert werden konnten.

**Usage**:

```console
$ emby-integrator list-metadata [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: [required]

**Options**:

* `--json-output / --no-json-output`: Gebe die Ausgabe im JSON-Format aus  [default: no-json-output]
* `--help`: Show this message and exit.

## `emby-integrator write-nfo-file`

Generiert die NFO-Metadatendatei und schreibt sie in eine Datei.

Args:
    file_path (str): Pfad zur Videodatei.

**Usage**:

```console
$ emby-integrator write-nfo-file [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: [required]

**Options**:

* `--help`: Show this message and exit.
