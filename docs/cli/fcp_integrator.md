# `fcp-integrator`

Führt den Workflow aus: Integration in Emby Mediathek und Cleanup von zugehörigen Dateien.
Löscht die Originaldateien nach erfolgreicher Integration in Emby Mediathek, wenn --delete-source-files gesetzt ist.

**Usage**:

```console
$ fcp-integrator [OPTIONS] SEARCH_DIR EMBY_DIR
```

**Arguments**:

* `SEARCH_DIR`: Das Hauptverzeichnis mit Mediendateien.  [required]
* `EMBY_DIR`: Das Zielverzeichnis in der Emby Mediathek.  [required]

**Options**:

* `-ad, --additional-dir DIRECTORY`: Zusätzliches Verzeichnis.
* `--overwrite-existing`: Überschreibt bestehende Dateien ohne Rückfrage, wenn diese existieren.
* `--force`: Unterdrückt alle Rückfragen und führt den Workflow automatisch aus.
* `--delete-source-files`: Löscht die Quelldateien nach erfolgreicher Integration.
* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.
