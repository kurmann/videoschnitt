# `fcp-integrator`

Final Cut Pro Integrator

**Usage**:

```console
$ fcp-integrator [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `convert-images`: Konvertiert alle unterstützten Bilddateien...
* `run-workflow`: Führt den Workflow aus: Integration in...

## `fcp-integrator convert-images`

Konvertiert alle unterstützten Bilddateien in einem Verzeichnis in AdobeRGB-JPEGs.

Jeder unterstützte Bild wird als neues JPEG im Zielverzeichnis erstellt. Nach erfolgreicher Konvertierung wird die Originaldatei entweder in das angegebene Archivverzeichnis verschoben oder gelöscht.

**Usage**:

```console
$ fcp-integrator convert-images [OPTIONS] SOURCE_DIR
```

**Arguments**:

* `SOURCE_DIR`: Das Quellverzeichnis mit unterstützten Bilddateien (PNG, JPG, JPEG, TIF, TIFF).  [required]

**Options**:

* `-t, --target-dir DIRECTORY`: Das Zielverzeichnis für die konvertierten JPEG-Bilder. Wenn nicht angegeben, werden die JPEGs im Quellverzeichnis erstellt.
* `-a, --archive-directory DIRECTORY`: Das Archivverzeichnis, in das die Originaldateien nach erfolgreicher Konvertierung verschoben werden. Wenn nicht angegeben, werden die Originaldateien gelöscht.
* `--help`: Show this message and exit.

## `fcp-integrator run-workflow`

Führt den Workflow aus: Integration in Emby Mediathek und Cleanup von zugehörigen Dateien.
Löscht die Originaldateien nach erfolgreicher Integration in Emby Mediathek, wenn --delete-source-files gesetzt ist.

**Usage**:

```console
$ fcp-integrator run-workflow [OPTIONS] SEARCH_DIR EMBY_DIR
```

**Arguments**:

* `SEARCH_DIR`: Das Hauptverzeichnis mit Mediendateien.  [required]
* `EMBY_DIR`: Das Zielverzeichnis in der Emby Mediathek.  [required]

**Options**:

* `-ad, --additional-dir DIRECTORY`: Zusätzliches Verzeichnis.
* `--overwrite-existing`: Überschreibt bestehende Dateien ohne Rückfrage, wenn diese existieren.
* `--force`: Unterdrückt alle Rückfragen und führt den Workflow automatisch aus.
* `--delete-source-files`: Löscht die Quelldateien nach erfolgreicher Integration.
* `--help`: Show this message and exit.
