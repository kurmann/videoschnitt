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
* `get-bitrate`: Gibt die Bitrate einer Videodatei zurück.
* `get-creation-datetime`: Gibt das Erstellungsdatum einer...
* `get-video-codec`: Gibt den Videocodec einer Datei zurück.
* `is-hevc-a`: Überprüft, ob eine Videodatei HEVC-A ist...
* `show-metadata`: Zeigt die Metadaten einer Datei an.
* `validate-file`: Validiert die Mediendatei, indem überprüft...

## `metadata-manager export-metadata`

Exportiert die Metadaten einer Datei in eine JSON- oder Textdatei.


**Beispiele:**
    metadata-manager export-metadata /path/to/video.mov /path/to/output.json
    metadata-manager export-metadata /path/to/video.mov /path/to/output.txt


**Beispielausgabe (JSON):**
{
    "FileName": "video.mov",
    "Directory": "/path/to/",
    "FileSize": "10 GB",
    "FileModifyDate": "2024:09:19 17:03:53+02:00",
    "FileType": "MOV",
    "MIMEType": "video/quicktime",
    "CreateDate": "2024:09:19 14:29:04",
    "Duration": "0:14:47",
    "AudioFormat": "mp4a",
    "ImageWidth": "3840",
    "ImageHeight": "2160",
    "CompressorID": "hvc1",
    "CompressorName": "HEVC",
    "BitDepth": "24",
    "VideoFrameRate": "60",
    "Title": "2024-09-15 Paula MTB-Finale Huttwil",
    "Album": "Familie Kurmann",
    "Description": "Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.",
    "Copyright": "© Patrick Kurmann 2024",
    "Author": "Patrick Kurmann",
    "Keywords": "17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann",
    "AvgBitrate": "90.7 Mbps",
    "Producer": "Patrick Kurmann",
    "Studio": "Kurmann Studios"
}


**Beispielausgabe (TXT):**
FileName: video.mov
Directory: /path/to/
FileSize: 10 GB
FileModifyDate: 2024:09:19 17:03:53+02:00
FileType: MOV
MIMEType: video/quicktime
CreateDate: 2024:09:19 14:29:04
Duration: 0:14:47
AudioFormat: mp4a
ImageWidth: 3840
ImageHeight: 2160
CompressorID: hvc1
CompressorName: HEVC
BitDepth: 24
VideoFrameRate: 60
Title: 2024-09-15 Paula MTB-Finale Huttwil
Album: Familie Kurmann
Description: Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
Copyright: © Patrick Kurmann 2024
Author: Patrick Kurmann
Keywords: 17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann
AvgBitrate: 90.7 Mbps
Producer: Patrick Kurmann
Studio: Kurmann Studios

**Usage**:

```console
$ metadata-manager export-metadata [OPTIONS] FILE_PATH OUTPUT_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, aus der die Metadaten exportiert werden sollen  [required]
* `OUTPUT_PATH`: Pfad zur Ausgabedatei (JSON oder TXT)  [required]

**Options**:

* `--help`: Show this message and exit.

## `metadata-manager get-bitrate`

Gibt die Bitrate einer Videodatei zurück.


**Beispiel:**
    metadata-manager get-bitrate /path/to/video.mov


**Beispielausgabe (Bitrate gefunden):**
    Bitrate: 90.7 Mbps


**Beispielausgabe (Bitrate nicht gefunden):**
    Bitrate konnte nicht ermittelt werden.

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


**Beispiel:**
    metadata-manager get-creation-datetime /path/to/video.mov


**Beispielausgabe (Datum gefunden):**
    Erstellungsdatum: 2024-09-19 14:29:04


**Beispielausgabe (Datum nicht gefunden, Fallback):**
    Erstellungsdatum konnte nicht ermittelt werden.

**Usage**:

```console
$ metadata-manager get-creation-datetime [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, deren Erstellungsdatum abgerufen werden soll  [required]

**Options**:

* `--help`: Show this message and exit.

## `metadata-manager get-video-codec`

Gibt den Videocodec einer Datei zurück.


**Beispiel:**
    metadata-manager get-video-codec /path/to/video.mov


**Beispielausgabe (Codec gefunden):**
    Videocodec: HEVC


**Beispielausgabe (Codec nicht gefunden):**
    Videocodec konnte nicht ermittelt werden.

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


**Beispiel:**
    metadata-manager is-hevc-a /path/to/video.mov


**Beispielausgabe (HEVC-A):**
    Die Datei ist HEVC-A (Bitrate > 80 Mbit/s).


**Beispielausgabe (Nicht HEVC-A):**
    Die Datei ist nicht HEVC-A (Bitrate <= 80 Mbit/s).

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


**Beispiele:**
    metadata-manager show-metadata /path/to/video.mov
    metadata-manager show-metadata /path/to/video.mov --json


**Beispielausgabe (Text):**
    FileName: video.mov
    Directory: /path/to/
    FileSize: 10 GB
    FileModifyDate: 2024:09:19 17:03:53+02:00
    FileType: MOV
    MIMEType: video/quicktime
    CreateDate: 2024:09:19 14:29:04
    Duration: 0:14:47
    AudioFormat: mp4a
    ImageWidth: 3840
    ImageHeight: 2160
    CompressorID: hvc1
    CompressorName: HEVC
    BitDepth: 24
    VideoFrameRate: 60
    Title: 2024-09-15 Paula MTB-Finale Huttwil
    Album: Familie Kurmann
    Description: Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
    Copyright: © Patrick Kurmann 2024
    Author: Patrick Kurmann
    Keywords: 17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann
    AvgBitrate: 90.7 Mbps
    Producer: Patrick Kurmann
    Studio: Kurmann Studios


**Beispielausgabe (JSON):**
{
    "FileName": "video.mov",
    "Directory": "/path/to/",
    "FileSize": "10 GB",
    "FileModifyDate": "2024:09:19 17:03:53+02:00",
    "FileType": "MOV",
    "MIMEType": "video/quicktime",
    "CreateDate": "2024:09:19 14:29:04",
    "Duration": "0:14:47",
    "AudioFormat": "mp4a",
    "ImageWidth": "3840",
    "ImageHeight": "2160",
    "CompressorID": "hvc1",
    "CompressorName": "HEVC",
    "BitDepth": "24",
    "VideoFrameRate": "60",
    "Title": "2024-09-15 Paula MTB-Finale Huttwil",
    "Album": "Familie Kurmann",
    "Description": "Start von Paula Gorycka  am XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.",
    "Copyright": "© Patrick Kurmann 2024",
    "Author": "Patrick Kurmann",
    "Keywords": "17.09.24,fotos,aufgenommen von patrick kurmann,paula mtb-finale huttwil,aufgenommen von silvan kurmann",
    "AvgBitrate": "90.7 Mbps",
    "Producer": "Patrick Kurmann",
    "Studio": "Kurmann Studios"
}

**Usage**:

```console
$ metadata-manager show-metadata [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, aus der die Metadaten angezeigt werden sollen  [required]

**Options**:

* `-j, --json`: Gebe die Metadaten im JSON-Format aus
* `--help`: Show this message and exit.

## `metadata-manager validate-file`

Validiert die Mediendatei, indem überprüft wird, ob die Metadaten erfolgreich geladen werden können.


**Beispiel:**
    metadata-manager validate-file /path/to/video.mov


**Beispielausgabe (Erfolgreich):**
    Die Datei wurde erfolgreich validiert. Metadaten konnten geladen werden.


**Beispielausgabe (Fehlgeschlagen):**
    Die Datei '/path/to/video.mov' wurde nicht gefunden.

**Usage**:

```console
$ metadata-manager validate-file [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Mediendatei, die validiert werden soll  [required]

**Options**:

* `--help`: Show this message and exit.
