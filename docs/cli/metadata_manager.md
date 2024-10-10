# `metadata-manager`

Metadata Manager CLI für Kurmann Videoschnitt

**Usage**:

```console
$ metadata-manager [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `export-metadata`: Exportiert die Metadaten einer Datei in...
* `get-album`: Gibt den Album-Tag einer Mediendatei zurück.
* `get-bitrate`: Gibt die Bitrate einer Videodatei zurück.
* `get-creation-datetime`: Gibt das Erstellungsdatum einer...
* `get-recording-date`: Extrahiert das Aufnahmedatum aus Dateiname...
* `get-video-codec`: Gibt den Videocodec einer Datei zurück.
* `is-hevc-a`: Überprüft, ob eine Videodatei HEVC-A ist...
* `show-metadata`: Zeigt die Metadaten einer Datei an.
* `show-metadata-with-exiftool`: Zeigt die Metadaten einer Datei an,...
* `show-metadata-with-ffmpeg`: Zeigt die Metadaten einer Datei an,...
* `validate-file`: Validiert die Mediendatei, indem überprüft...

## `metadata-manager export-metadata`

Exportiert die Metadaten einer Datei in eine JSON- oder Textdatei.

## Argumente:
- **file_path** (*Path*): Pfad zur Mediendatei.
- **output_path** (*Path*): Pfad zur Ausgabedatei (unterstützt .json, .txt, .md).
- **include_source** (*bool*): Wenn gesetzt, werden die Quellen der Metadaten ebenfalls in die Datei geschrieben.

## Beispielaufrufe:
```bash
metadata-manager export-metadata /Pfad/zur/Datei.mov /Pfad/zur/Ausgabedatei.json
```

Ausgabe:
```plaintext
Metadaten erfolgreich als JSON exportiert nach: /Pfad/zur/Ausgabedatei.json
```

Mit Text-Export:
```bash
metadata-manager export-metadata /Pfad/zur/Datei.mov /Pfad/zur/Ausgabedatei.txt
```

Ausgabe:
```plaintext
Metadaten erfolgreich als Text exportiert nach: /Pfad/zur/Ausgabedatei.txt
```

**Usage**:

```console
$ metadata-manager export-metadata [OPTIONS] FILE_PATH OUTPUT_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, aus der die Metadaten exportiert werden sollen  [required]
* `OUTPUT_PATH`: Pfad zur Ausgabedatei (JSON oder TXT)  [required]

**Options**:

* `-s, --include-source`: Gibt die Quelle jeder Eigenschaft mit aus
* `--help`: Show this message and exit.

## `metadata-manager get-album`

Gibt den Album-Tag einer Mediendatei zurück.

## Argumente:
- **file_path** (*Path*): Pfad zur Mediendatei, deren Album-Tag abgerufen werden soll.

## Beispielaufruf:
```bash
metadata-manager get-album /Pfad/zur/Datei.mov
```

Ausgabe:
```plaintext
Album: MeinAlbum
```

**Usage**:

```console
$ metadata-manager get-album [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, deren Album-Tag abgerufen werden soll  [required]

**Options**:

* `--help`: Show this message and exit.

## `metadata-manager get-bitrate`

Gibt die Bitrate einer Videodatei zurück.

## Argumente:
- **file_path** (*Path*): Pfad zur Videodatei, deren Bitrate abgerufen werden soll.

## Beispielaufruf:
```bash
metadata-manager get-bitrate /Pfad/zur/Datei.mov
```

Ausgabe:
```plaintext
Bitrate: 1069.47 Mbps
```

**Usage**:

```console
$ metadata-manager get-bitrate [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Videodatei, deren Bitrate abgerufen werden soll  [required]

**Options**:

* `--help`: Show this message and exit.

## `metadata-manager get-creation-datetime`

Gibt das Erstellungsdatum einer Mediendatei zurück.

## Argumente:
- **file_path** (*Path*): Pfad zur Mediendatei, deren Erstellungsdatum abgerufen werden soll.

## Beispielaufruf:
```bash
metadata-manager get-creation-datetime /Pfad/zur/Datei.mov
```

Ausgabe:
```plaintext
Erstellungsdatum: 2024-09-23T19:16:33+02:00
```

**Usage**:

```console
$ metadata-manager get-creation-datetime [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, deren Erstellungsdatum abgerufen werden soll  [required]

**Options**:

* `--help`: Show this message and exit.

## `metadata-manager get-recording-date`

Extrahiert das Aufnahmedatum aus Dateiname oder Titel.

Gibt einen Fehler aus, wenn kein Datum gefunden wird. Standardmäßig wird zuerst aus dem Dateinamen extrahiert (Rang 1)
und bei Misserfolg aus dem Titel (Rang 2), wenn ein Dateipfad angegeben ist.

**Usage**:

```console
$ metadata-manager get-recording-date [OPTIONS] FILE_NAME
```

**Arguments**:

* `FILE_NAME`: Dateiname, aus dem das Aufnahmedatum extrahiert werden soll  [required]

**Options**:

* `-f, --file-path TEXT`: Pfad zur Datei, um das Datum aus dem Titel zu extrahieren
* `-n, --filename-only`: Extrahiere das Aufnahmedatum nur aus dem Dateinamen
* `--help`: Show this message and exit.

## `metadata-manager get-video-codec`

Gibt den Videocodec einer Datei zurück.

## Argumente:
- **file_path** (*Path*): Pfad zur Videodatei, deren Videocodec abgerufen werden soll.

## Beispielaufruf:
```bash
metadata-manager get-video-codec /Pfad/zur/Datei.mov
```

Ausgabe:
```plaintext
Videocodec: prores
```

**Usage**:

```console
$ metadata-manager get-video-codec [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Videodatei, deren Codec abgerufen werden soll  [required]

**Options**:

* `--help`: Show this message and exit.

## `metadata-manager is-hevc-a`

Überprüft, ob eine Videodatei HEVC-A ist (Bitrate > 80 Mbit/s).

## Argumente:
- **file_path** (*Path*): Pfad zur Videodatei, die überprüft werden soll.

## Beispielaufruf:
```bash
metadata-manager is-hevc-a /Pfad/zur/Datei.mov
```

Ausgabe:
```plaintext
Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).
```

**Usage**:

```console
$ metadata-manager is-hevc-a [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Videodatei, die überprüft werden soll  [required]

**Options**:

* `--help`: Show this message and exit.

## `metadata-manager show-metadata`

Zeigt die Metadaten einer Datei an.

## Argumente:
- **file_path** (*Path*): Pfad zur Mediendatei.
- **json_output** (*bool*): Wenn gesetzt, werden die Metadaten im JSON-Format ausgegeben.
- **include_source** (*bool*): Wenn gesetzt, werden die Quellen der Metadaten ebenfalls angezeigt.

## Beispielaufrufe:
```bash
metadata-manager show-metadata /Pfad/zur/Datei.mov
```

Ausgabe:
```plaintext
FileName: Datei.mov
Bitrate: 1069.47 Mbps
VideoCodec: prores
```

Mit JSON-Option:
```bash
metadata-manager show-metadata /Pfad/zur/Datei.mov --json
```

Ausgabe im JSON-Format:
```json
{
    "FileName": "Datei.mov",
    "Bitrate": "1069.47 Mbps",
    "VideoCodec": "prores"
}
```

**Usage**:

```console
$ metadata-manager show-metadata [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen  [required]

**Options**:

* `-j, --json`: Gebe die Metadaten im JSON-Format aus
* `-s, --include-source`: Gibt die Quelle jeder Eigenschaft mit aus
* `--help`: Show this message and exit.

## `metadata-manager show-metadata-with-exiftool`

Zeigt die Metadaten einer Datei an, ermittelt mit ExifTool.

**Usage**:

```console
$ metadata-manager show-metadata-with-exiftool [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen  [required]

**Options**:

* `-j, --json`: Gebe die Metadaten im JSON-Format aus
* `--help`: Show this message and exit.

## `metadata-manager show-metadata-with-ffmpeg`

Zeigt die Metadaten einer Datei an, ermittelt mit FFmpeg/FFprobe.

**Usage**:

```console
$ metadata-manager show-metadata-with-ffmpeg [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen  [required]

**Options**:

* `-j, --json`: Gebe die Metadaten im JSON-Format aus
* `--help`: Show this message and exit.

## `metadata-manager validate-file`

Validiert die Mediendatei, indem überprüft wird, ob die Metadaten erfolgreich geladen werden können.

## Argumente:
- **file_path** (*Path*): Pfad zur Mediendatei, die validiert werden soll.
- **include_source** (*bool*): Wenn gesetzt, wird die Quelle der Metadaten ebenfalls überprüft.

## Beispielaufruf:
```bash
metadata-manager validate-file /Pfad/zur/Datei.mov
```

Ausgabe:
```plaintext
Die Datei wurde erfolgreich validiert. Metadaten konnten geladen werden.
```

**Usage**:

```console
$ metadata-manager validate-file [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, die validiert werden soll  [required]

**Options**:

* `-s, --include-source`: Überprüft die Quelle der Metadaten
* `--help`: Show this message and exit.
