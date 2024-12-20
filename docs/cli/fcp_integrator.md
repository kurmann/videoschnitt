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

* `convert-images`: Konvertiert alle PNG-Bilder in einem...
* `run-workflow`: Führt den Workflow aus: Integration in...

## `fcp-integrator convert-images`

Konvertiert alle PNG-Bilder in einem Verzeichnis in AdobeRGB-JPEGs.

Jeder PNG wird als neues JPEG im Zielverzeichnis erstellt. Wenn kein Zielverzeichnis angegeben ist, werden die JPEGs im Quellverzeichnis abgelegt.

**Usage**:

```console
$ fcp-integrator convert-images [OPTIONS] SOURCE_DIR
```

**Arguments**:

* `SOURCE_DIR`: Das Quellverzeichnis mit PNG-Bildern.  [required]

**Options**:

* `-t, --target-dir DIRECTORY`: Das Zielverzeichnis für die konvertierten JPEG-Bilder. Wenn nicht angegeben, werden die JPEGs im Quellverzeichnis erstellt.
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
