# `apple-compressor-manager`

Apple Compressor Manager

**Usage**:

```console
$ apple-compressor-manager [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `add-tag`: Fügt der Datei das Tag 'An Apple...
* `compress-file`: Komprimiert eine oder mehrere Dateien...
* `list-profiles`: Listet alle verfügbaren Compressor-Profile...

## `apple-compressor-manager add-tag`

Fügt der Datei das Tag 'An Apple Kompressor übergeben' hinzu.

## Argumente:
- **file_path** (*str*): Pfad zur Datei, die getaggt werden soll.

## Beispielaufruf:
```bash
apple-compressor-manager add-tag /Pfad/zur/Datei.mov
```

**Usage**:

```console
$ apple-compressor-manager add-tag [OPTIONS] FILE_PATH
```

**Arguments**:

* `FILE_PATH`: Pfad zur Datei, die getaggt werden soll  [required]

**Options**:

* `--help`: Show this message and exit.

## `apple-compressor-manager compress-file`

Komprimiert eine oder mehrere Dateien unter Verwendung von Compressor-Profilen.

## Argumente:
- **input_file** (*str*): Pfad zur Datei, die komprimiert werden soll.
- **compressor_profiles** (*str*): Durch Kommas getrennte Liste von Compressor-Profilnamen.
- **output** (*str, optional*): Verzeichnis für die Ausgabedatei. Standardmäßig das Verzeichnis der Eingabedatei mit angehängtem Profilnamen.
- **check_interval** (*int*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus.

## Beispielaufruf:
```bash
apple-compressor-manager compress-file /Pfad/zur/Datei.m4v "4K60-Medienserver,1080p-Internet" --output /Pfad/zum/Output-Verzeichnis --check-interval 60
```

**Usage**:

```console
$ apple-compressor-manager compress-file [OPTIONS] INPUT_FILE COMPRESSOR_PROFILES
```

**Arguments**:

* `INPUT_FILE`: Pfad zur Datei, die komprimiert werden soll  [required]
* `COMPRESSOR_PROFILES`: Durch Kommas getrennte Liste von Compressor-Profilnamen  [required]

**Options**:

* `--output TEXT`: Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll
* `--check-interval INTEGER`: Intervall in Sekunden für die Überprüfung des Komprimierungsstatus  [default: 30]
* `--help`: Show this message and exit.

## `apple-compressor-manager list-profiles`

Listet alle verfügbaren Compressor-Profile auf.

## Beispielaufruf:
```bash
apple-compressor-manager list-profiles
```

## Ausgabe:
```plaintext
Verfügbare Compressor-Profile:
- HEVC-A
- 1080p-Internet
- 4K60-Medienserver
- 4K30-Medienserver
- 4K-Internet
```

**Usage**:

```console
$ apple-compressor-manager list-profiles [OPTIONS]
```

**Options**:

* `--help`: Show this message and exit.
