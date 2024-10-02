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

* `convert-images-to-adobe-rgb`: Konvertiere eine Liste von PNG-Bildern in...
* `convert-single-image`: Konvertiere ein einzelnes Bild in das...
* `generate-nfo-xml`: Generiert die NFO-Metadatendatei und gibt...
* `list-metadata-command`: Extrahiere die Metadaten aus einer Datei...
* `scan-media-command`: Scannt ein Verzeichnis nach Bilddateien...
* `write-nfo-file`: Generiert die NFO-Metadatendatei und...

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

## `emby-integrator convert-single-image`

Konvertiere ein einzelnes Bild in das Adobe RGB-Farbprofil.

Diese Methode konvertiert das angegebene Bild in das Adobe RGB-Farbprofil und speichert es als JPG.
Standardmäßig wird das Originalbild nach erfolgreicher Konvertierung gelöscht. Der Benutzer kann dies
durch Bestätigung steuern oder das Löschen ohne Rückfrage erzwingen.

Args:
    image_path (str): Pfad zur Bilddatei, die konvertiert werden soll.
    no_confirm (bool): Optional. Wenn gesetzt, wird das Originalbild ohne Rückfrage gelöscht. Standard ist `False`.

Returns:
    None

Beispiel:
    Konvertiere ein Bild mit Bestätigung zum Löschen des Originals:
        $ emby-integrator convert-single-image /path/to/image.png

    Konvertiere ein Bild und lösche das Original ohne Rückfrage:
        $ emby-integrator convert-single-image /path/to/image.png --no-confirm

**Usage**:

```console
$ emby-integrator convert-single-image [OPTIONS] IMAGE_PATH
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

## `emby-integrator list-metadata-command`

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
$ emby-integrator list-metadata-command [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: [required]

**Options**:

* `--json-output / --no-json-output`: [default: no-json-output]
* `--help`: Show this message and exit.

## `emby-integrator scan-media-command`

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
$ emby-integrator scan-media-command [OPTIONS] MEDIA_DIR
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
