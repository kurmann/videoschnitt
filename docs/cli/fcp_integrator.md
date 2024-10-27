# `fcp-integrator`

Führt den kompletten Workflow aus: Integration in iCloud Drive und Emby Mediathek.
Löscht die Originaldateien nach erfolgreicher Integration in Emby Mediathek.

**Usage**:

```console
$ fcp-integrator [OPTIONS] SEARCH_DIR ICLOUD_DIR EMBY_DIR
```

**Arguments**:

* `SEARCH_DIR`: Das Hauptverzeichnis mit Mediendateien.  [required]
* `ICLOUD_DIR`: Das Zielverzeichnis in iCloud Drive.  [required]
* `EMBY_DIR`: Das Zielverzeichnis in der Emby Mediathek.  [required]

**Options**:

* `-ad, --additional-dir DIRECTORY`: Zusätzliches Verzeichnis.
* `--overwrite-existing`: Überschreibt bestehende Dateien ohne Rückfrage, wenn diese existieren.
* `--force`: Unterdrückt alle Rückfragen und führt den Workflow automatisch aus.
* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.
