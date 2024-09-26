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
* `cleanup-prores`: Bereinigt ProRes-Dateien mit einem...
* `compress-prores-file`: Komprimiert eine einzelne ProRes-Datei...
* `compress-prores-files`: Komprimiert ProRes-Dateien in einem...
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

## `apple-compressor-manager cleanup-prores`

Bereinigt ProRes-Dateien mit einem HEVC-A-Pendant.

## Argumente:
- **hevc_a_dir** (*str*): Pfad zum HEVC-A-Verzeichnis.
- **prores_dir** (*str, optional*): Pfad zum ProRes-Verzeichnis. Wenn nicht angegeben, wird `hevc_a_dir` verwendet.
- **verbose** (*bool*): Aktiviert detaillierte Ausgaben, wenn gesetzt.

## Beispielaufruf:
```bash
apple-compressor-manager cleanup-prores /Pfad/zum/HEVC-A-Verzeichnis /Pfad/zum/ProRes-Verzeichnis --verbose
```

**Usage**:

```console
$ apple-compressor-manager cleanup-prores [OPTIONS] HEVC_A_DIR [PRORES_DIR]
```

**Arguments**:

* `HEVC_A_DIR`: Pfad zum HEVC-A-Verzeichnis  [required]
* `[PRORES_DIR]`: Pfad zum ProRes-Verzeichnis

**Options**:

* `--verbose`: Aktiviere detaillierte Ausgaben.
* `--help`: Show this message and exit.

## `apple-compressor-manager compress-prores-file`

Komprimiert eine einzelne ProRes-Datei unter Verwendung eines Compressor-Profils.

## Argumente:
- **input_file** (*str*): Pfad zur ProRes-Datei.
- **compressor_profile** (*str*): Name des Compressor-Profils.
- **output** (*str, optional*): Verzeichnis für die Ausgabedatei. Standardmäßig das Verzeichnis der Eingabedatei.
- **delete_prores** (*bool*): Löscht die ProRes-Datei nach erfolgreicher Komprimierung, wenn gesetzt.
- **confirm_delete** (*bool*): Bestätigt das Löschen der Originaldatei, wenn gesetzt.
- **check_interval** (*int*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus.

## Beispielaufruf:
```bash
apple-compressor-manager compress-prores-file /Pfad/zur/Datei.mov "MeinCompressorProfil" --output /Pfad/zum/Output-Verzeichnis --delete-prores --confirm-delete --check-interval 60
```

**Usage**:

```console
$ apple-compressor-manager compress-prores-file [OPTIONS] INPUT_FILE COMPRESSOR_PROFILE
```

**Arguments**:

* `INPUT_FILE`: Pfad zur ProRes-Datei  [required]
* `COMPRESSOR_PROFILE`: Name des Compressor-Profils  [required]

**Options**:

* `--output TEXT`: Das Verzeichnis, in dem die Ausgabedatei gespeichert werden soll
* `--delete-prores`: Lösche die ProRes-Datei nach erfolgreicher Komprimierung
* `--confirm-delete`: Bestätige das Löschen der Originaldatei
* `--check-interval INTEGER`: Intervall in Sekunden für die Überprüfung des Komprimierungsstatus  [default: 30]
* `--help`: Show this message and exit.

## `apple-compressor-manager compress-prores-files`

Komprimiert ProRes-Dateien in einem Verzeichnis unter Verwendung eines Compressor-Profils.

## Argumente:
- **input_dir** (*str*): Pfad zum Quellverzeichnis der ProRes-Dateien.
- **compressor_profile** (*str*): Name des Compressor-Profils.
- **output** (*str, optional*): Verzeichnis für die Ausgabedateien. Standardmäßig dasselbe wie `input_dir`.
- **delete_prores** (*bool*): Löscht ProRes-Dateien nach erfolgreicher Komprimierung, wenn gesetzt.
- **confirm_delete** (*bool*): Bestätigt das Löschen der Originaldateien, wenn gesetzt.
- **check_interval** (*int*): Intervall in Sekunden für die Überprüfung des Komprimierungsstatus.

## Beispielaufruf:
```bash
apple-compressor-manager compress-prores-files /Pfad/zum/Input-Verzeichnis "MeinCompressorProfil" --output /Pfad/zum/Output-Verzeichnis --delete-prores --confirm-delete --check-interval 60
```

**Usage**:

```console
$ apple-compressor-manager compress-prores-files [OPTIONS] INPUT_DIR COMPRESSOR_PROFILE
```

**Arguments**:

* `INPUT_DIR`: Pfad zum Quellverzeichnis der ProRes-Dateien  [required]
* `COMPRESSOR_PROFILE`: Name des Compressor-Profils  [required]

**Options**:

* `--output TEXT`: Das Verzeichnis, in dem die Ausgabedateien gespeichert werden sollen
* `--delete-prores`: Lösche ProRes-Dateien nach erfolgreicher Komprimierung
* `--confirm-delete`: Bestätige das Löschen der Originaldateien
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
- MeinCompressorProfil
- StandardProfil
- HEVC-A-Profil
```

**Usage**:

```console
$ apple-compressor-manager list-profiles [OPTIONS]
```

**Options**:

* `--help`: Show this message and exit.
