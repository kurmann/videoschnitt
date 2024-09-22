# `original-media-integrator`

CLI-Kommando zum Importieren und Komprimieren von Medien.

Dieser Befehl ruft die import_and_compress_media Funktion auf, die neue Dateien komprimiert
und organisiert.

**Usage**:

```console
$ original-media-integrator [OPTIONS]
```

**Options**:

* `-s, --source-dir TEXT`: Pfad zum Quellverzeichnis  [env var: original_media_source_dir]
* `-d, --destination-dir TEXT`: Pfad zum Zielverzeichnis  [env var: original_media_destination_dir]
* `-c, --compression-dir TEXT`: Optionales Komprimierungsverzeichnis  [env var: original_media_compression_dir]
* `-k, --keep-original-prores`: Behalte die Original-ProRes-Dateien nach der Komprimierung.  [env var: original_media_keep_original_prores]
* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.
