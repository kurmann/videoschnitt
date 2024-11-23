# `video-compressor`

**Usage**:

```console
$ video-compressor [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `convert-to-apple`: Konvertiert eine Videodatei in ein...
* `convert-to-hevc`: Konvertiert alle nicht H.264 oder HEVC...

## `video-compressor convert-to-apple`

Konvertiert eine Videodatei in ein Apple-kompatibles Format.
Videostreams werden remuxt, wenn möglich. DTS wird zu E-AC3 umgewandelt.
Nicht-kompatible Untertitel werden extrahiert, kompatible übernommen.

**Usage**:

```console
$ video-compressor convert-to-apple [OPTIONS] INPUT_FILE
```

**Arguments**:

* `INPUT_FILE`: Pfad zur Eingabedatei  [required]

**Options**:

* `--keep-original`: Behalte die Originaldatei.
* `--constant-quality INTEGER`: Constant Quality für Videotoolbox (CQ-Wert).  [default: 89]
* `--help`: Show this message and exit.

## `video-compressor convert-to-hevc`

Konvertiert alle nicht H.264 oder HEVC Videos in einem Verzeichnis nach HEVC mit HandBrakeCLI.
WebM-Dateien mit H.264 oder HEVC werden ohne Neukodierung in einen MP4-Container remuxt.
Mit der Option --force-remux-mp4-h264 werden MP4-Dateien mit H.264 Codec ebenfalls remuxt.

**Usage**:

```console
$ video-compressor convert-to-hevc [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: Pfad zum Verzeichnis mit den Videodateien  [required]

**Options**:

* `--preset-file TEXT`: Pfad zur Preset-Datei (JSON)  [default: /Users/patrickkurmann/Library/Containers/fr.handbrake.HandBrake/Data/Library/Application Support/HandBrake/UserPresets.json]
* `--preset TEXT`: Name des HandBrakeCLI Presets  [default: YouTube]
* `--postfix TEXT`: Postfix für die Ausgabedateien  [default: -hevc]
* `--keep-original`: Originaldateien behalten und nicht löschen
* `--force-remux-mp4-h264`: Erzwingt das Remuxing von MP4-Dateien mit H.264 Codec.
* `--help`: Show this message and exit.
