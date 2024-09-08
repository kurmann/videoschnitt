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
* `get-mediaserver-files`: Gruppiere Mediendateien nach Mediensets...

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

## `emby-integrator get-mediaserver-files`

Gruppiere Mediendateien nach Mediensets und überprüfe, ob passende Titelbilder existieren.

Args:
    source_dir (str): Das Verzeichnis, in dem nach Mediendateien und Bildern gesucht wird.

Returns:
    dict: Ein Dictionary, das Mediensets als Schlüssel enthält und die zugehörigen 
        Videos und optionalen Titelbilder als Wert hat.

        Beispiel:
        {
            "Medienset_Name": {
                "videos": ["video1.mp4", "video2.mov"],
                "image": "titelbild.jpg"  # oder None, wenn kein Bild vorhanden ist
            }
        }

Raises:
    FileNotFoundError: Wenn das angegebene Verzeichnis nicht existiert.
    ValueError: Wenn keine Mediendateien gefunden werden.

**Usage**:

```console
$ emby-integrator get-mediaserver-files [OPTIONS] SOURCE_DIR
```

**Arguments**:

* `SOURCE_DIR`: [required]

**Options**:

* `--help`: Show this message and exit.
