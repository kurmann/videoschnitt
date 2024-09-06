# `original-media-integrator`

CLI-Kommando zum Importieren und Komprimieren von Medien.

Dieser Befehl ruft die import_and_compress_media Funktion auf, die neue Dateien komprimiert
und organisiert.

**Usage**:

```console
$ original-media-integrator [OPTIONS] SOURCE_DIR DESTINATION_DIR
```

**Arguments**:

* `SOURCE_DIR`: Pfad zum Quellverzeichnis  [required]
* `DESTINATION_DIR`: Pfad zum Zielverzeichnis  [required]

**Options**:

* `--compression-dir TEXT`: Optionales Komprimierungsverzeichnis
* `--keep-original-prores / --no-keep-original-prores`: Behalte die Original-ProRes-Dateien nach der Komprimierung.  [default: no-keep-original-prores]
* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.
