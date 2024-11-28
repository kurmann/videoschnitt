# `dji-import`

Importiert Medien von einer DJI Action Kamera und organisiert sie in einem strukturierten Verzeichnis.

- Quelle: Dateien aus `source_dir` werden analysiert.
- Ziel: Dateien werden nach ISO-Datum in `target_dir` organisiert.
- `.LRF`-Dateien werden nach erfolgreicher Integration gelöscht.

:param source_dir: Quellverzeichnis mit den DJI-Dateien.
:param target_dir: Zielverzeichnis für organisierte Dateien.

**Usage**:

```console
$ dji-import [OPTIONS] SOURCE_DIR TARGET_DIR
```

**Arguments**:

* `SOURCE_DIR`: [required]
* `TARGET_DIR`: [required]

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.
