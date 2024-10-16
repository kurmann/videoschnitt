# `video-compressor`

Konvertiert alle nicht H.264 oder HEVC Videos in einem Verzeichnis nach HEVC mit HandBrakeCLI.

**Usage**:

```console
$ video-compressor [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: Pfad zum Verzeichnis mit den Videodateien  [required]

**Options**:

* `--preset-file TEXT`: Pfad zur Preset-Datei (JSON)  [default: /Users/patrickkurmann/Library/Containers/fr.handbrake.HandBrake/Data/Library/Application Support/HandBrake/UserPresets.json]
* `--preset TEXT`: Name des HandBrakeCLI Presets  [default: YouTube]
* `--postfix TEXT`: Postfix für die Ausgabedateien  [default: -hevc]
* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.
