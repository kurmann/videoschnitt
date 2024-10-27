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

* `convert-image`: Konvertiere ein einzelnes Bild in das...
* `generate-nfo-xml`: Generiert die NFO-Metadatendatei und gibt...
* `group-files`: Gruppiert Dateien mit gleichen Basenamen...
* `integrate-homemovie`: Integriert einen Familienfilm in die...
* `rename-artwork`: Benennt alle JPG, JPEG und PNG-Dateien in...
* `reset-permissions`: Setzt die Berechtigungen eines...
* `scan-media`: Scannt ein Verzeichnis nach Bilddateien...
* `write-nfo-file`: Generiert die NFO-Metadatendatei und...

## `emby-integrator convert-image`

Konvertiere ein einzelnes Bild in das Adobe RGB-Farbprofil.

**Usage**:

```console
$ emby-integrator convert-image [OPTIONS] IMAGE_PATH
```

**Arguments**:

* `IMAGE_PATH`: [required]

**Options**:

* `-n, --no-confirm`: Lösche das Originalbild ohne Rückfrage.
* `--help`: Show this message and exit.

## `emby-integrator generate-nfo-xml`

Generiert die NFO-Metadatendatei und gibt das XML aus.

Diese Methode extrahiert die relevanten Metadaten aus der angegebenen Videodatei, erstellt eine
benutzerdefinierte NFO-Metadatendatei und gibt das resultierende XML in der Konsole aus.

Args:
    file_path (str): Pfad zur Videodatei.

Returns:
    None: Gibt das generierte XML in der Konsole aus.

Beispiel:
    $ emby-integrator generate-nfo-xml /path/to/video.mov

    Ausgabe:
    <?xml version="1.0" encoding="utf-8"?>
    <nfo>
        ...
    </nfo>

**Usage**:

```console
$ emby-integrator generate-nfo-xml [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: [required]

**Options**:

* `--help`: Show this message and exit.

## `emby-integrator group-files`

Gruppiert Dateien mit gleichen Basenamen in Unterverzeichnisse.

**Usage**:

```console
$ emby-integrator group-files [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: Pfad zum Verzeichnis, das gruppiert werden soll  [required]

**Options**:

* `-i, --ignore-suffix TEXT`: Liste von Suffixen, die beim Gruppieren ignoriert werden sollen (case-insensitive)  [default: -poster, -artwork, -fanart]
* `--help`: Show this message and exit.

## `emby-integrator integrate-homemovie`

Integriert einen Familienfilm in die Emby-Mediathek. Verschiebt die Videodatei und optional das Titelbild in das passende Verzeichnis und erstellt die erforderlichen Metadaten.

**Usage**:

```console
$ emby-integrator integrate-homemovie [OPTIONS] VIDEO_FILE MEDIATHEK_DIR
```

**Arguments**:

* `VIDEO_FILE`: Der Pfad zur Videodatei, die in die Mediathek integriert werden soll.  [required]
* `MEDIATHEK_DIR`: Das Hauptverzeichnis der Emby-Mediathek, in das der Familienfilm integriert werden soll.  [required]

**Options**:

* `-i, --title-image FILE`: Der Pfad zum optionalen Titelbild.
* `-v, --version TEXT`: Versionierungsoption: 'overwrite' oder 'new'.
* `--no-prompt`: Unterdrückt die Nachfrage bei der Integration.
* `--help`: Show this message and exit.

## `emby-integrator rename-artwork`

Benennt alle JPG, JPEG und PNG-Dateien in einem Verzeichnis (inkl. Unterverzeichnisse) um,
indem das Suffix '-poster' hinzugefügt oder ersetzt wird.

**Usage**:

```console
$ emby-integrator rename-artwork [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: Pfad zum Verzeichnis mit den Artwork-Dateien  [required]

**Options**:

* `--help`: Show this message and exit.

## `emby-integrator reset-permissions`

Setzt die Berechtigungen eines Verzeichnisses und optional aller Unterverzeichnisse und Dateien zurück.
Verzeichnisse erhalten 755, Dateien 644.

**Usage**:

```console
$ emby-integrator reset-permissions [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: Pfad zum Verzeichnis, dessen Berechtigungen zurückgesetzt werden sollen  [required]

**Options**:

* `--recursive / --no-recursive`: Rekursiv alle Unterverzeichnisse bearbeiten  [default: recursive]
* `--help`: Show this message and exit.

## `emby-integrator scan-media`

Scannt ein Verzeichnis nach Bilddateien (.png, .jpg, .jpeg) und QuickTime-Dateien (.mov),
gruppiert sie als Mediaserver-Set basierend auf den Bilddateien und listet unvollständige Gruppen auf.

## Argumente:
- **media_dir** (*Path*): Pfad zum Verzeichnis, das gescannt werden soll.
- **json_output** (*bool*): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

## Beispielaufrufe:
```bash
emby-integrator scan-media /Pfad/zum/Verzeichnis
```

Ausgabe:
```plaintext
Mediaserver-Set: 2024-09-08 Ann-Sophie rennt Testvideo
    Image: 2024-09-08 Ann-Sophie rennt Testvideo.png
    Video: 2024-09-08 Ann-Sophie rennt Testvideo-1080p-Internet 4K60-Medienserver.mov

Unvollständige Videodateien (ohne Bilder):
    - 2024-09-25 Event ohne Bild-2.mov
```

Mit JSON-Option:
```bash
emby-integrator scan-media /Pfad/zum/Verzeichnis --json
```

Ausgabe im JSON-Format:
```json
{
    "complete_sets": {
        "2024-09-08 Ann-Sophie rennt Testvideo": {
            "image": "2024-09-08 Ann-Sophie rennt Testvideo.png",
            "video": "2024-09-08 Ann-Sophie rennt Testvideo-1080p-Internet 4K60-Medienserver.mov"
        }
    },
    "incomplete_sets": [
        {
            "image": "2024-09-08 Ann-Sophie rennt Testvideo.png",
            "videos": ["2024-09-08 Ann-Sophie rennt Testvideo-720p-Hochwertig.mov"]
        },
        {
            "image": "2024-09-26 Another Event.png",
            "videos": []
        }
    ],
    "unmatched_videos": [
        "2024-09-25 Event ohne Bild-2.mov"
    ]
}

**Usage**:

```console
$ emby-integrator scan-media [OPTIONS] MEDIA_DIR
```

**Arguments**:

* `MEDIA_DIR`: [required]

**Options**:

* `--json-output / --no-json-output`: [default: no-json-output]
* `--help`: Show this message and exit.

## `emby-integrator write-nfo-file`

Generiert die NFO-Metadatendatei und schreibt sie in eine Datei.

Diese Methode extrahiert die relevanten Metadaten aus der angegebenen Videodatei, erstellt eine
benutzerdefinierte NFO-Metadatendatei und schreibt das resultierende XML in eine `.nfo` Datei neben der Videodatei.

Args:
    file_path (str): Pfad zur Videodatei.

Returns:
    None: Schreibt die generierte NFO-Datei in das gleiche Verzeichnis wie die Videodatei.

Beispiel:
    $ emby-integrator write-nfo-file /path/to/video.mov

    Ausgabe:
    NFO-Datei wurde erfolgreich erstellt: /path/to/video.nfo

**Usage**:

```console
$ emby-integrator write-nfo-file [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: [required]

**Options**:

* `--help`: Show this message and exit.
