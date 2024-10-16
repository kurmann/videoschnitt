# `mediaset-manager`

**Usage**:

```console
$ mediaset-manager [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `create-metadata-file`: Erstellt eine Metadaten.yaml-Datei für ein...
* `get-recording-date`: Extrahiert das Aufnahmedatum aus Dateiname...
* `get-resolution`: Gibt die Auflösungskategorie einer...
* `list-mediasets`: Listet alle Mediensets im angegebenen...

## `mediaset-manager create-metadata-file`

Erstellt eine Metadaten.yaml-Datei für ein gegebenes Medienset basierend auf einer Metadaten-Quelle.

**Usage**:

```console
$ mediaset-manager create-metadata-file [OPTIONS] METADATA_SOURCE
```

**Arguments**:

* `METADATA_SOURCE`: Pfad zur Metadaten-Quelle (z.B. eine Videodatei)  [required]

**Options**:

* `--help`: Show this message and exit.

## `mediaset-manager get-recording-date`

Extrahiert das Aufnahmedatum aus Dateiname oder Titel.

Gibt einen Fehler aus, wenn kein Datum gefunden wird. Standardmäßig wird zuerst aus dem Dateinamen extrahiert (Rang 1)
und bei Misserfolg aus dem Titel (Rang 2), wenn ein Dateipfad angegeben ist.

**Usage**:

```console
$ mediaset-manager get-recording-date [OPTIONS] FILE_NAME
```

**Arguments**:

* `FILE_NAME`: Dateiname, aus dem das Aufnahmedatum extrahiert werden soll  [required]

**Options**:

* `-f, --file-path TEXT`: Pfad zur Datei, um das Datum aus dem Titel zu extrahieren
* `-n, --filename-only`: Extrahiere das Aufnahmedatum nur aus dem Dateinamen
* `--help`: Show this message and exit.

## `mediaset-manager get-resolution`

Gibt die Auflösungskategorie einer Videodatei zurück (SD, 720p, 1080p, 2K, 4K)
sowie die exakte Auflösung in Pixeln (Höhe × Breite).

**Usage**:

```console
$ mediaset-manager get-resolution [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Videodatei, um die Auflösungskategorie zu bestimmen  [required]

**Options**:

* `--help`: Show this message and exit.

## `mediaset-manager list-mediasets`

Listet alle Mediensets im angegebenen Verzeichnis auf und klassifiziert die Dateien.
Optional durchsucht ein zusätzliches Verzeichnis nach Mediendateien.

**Usage**:

```console
$ mediaset-manager list-mediasets [OPTIONS] DIRECTORY
```

**Arguments**:

* `DIRECTORY`: Hauptverzeichnis mit den Mediendateien  [required]

**Options**:

* `-a, --additional-dir TEXT`: Optionales zusätzliches Verzeichnis mit Mediendateien
* `--json`: Gibt die Ausgabe als JSON zurück
* `--help`: Show this message and exit.
