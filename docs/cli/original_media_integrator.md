# `original-media-integrator`

Original Media Integrator

**Usage**:

```console
$ original-media-integrator [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `import-by-create-date`: Importiert Mediendateien basierend auf dem...
* `import-by-exif-creation-date`: Importiert Mediendateien basierend auf dem...

## `original-media-integrator import-by-create-date`

Importiert Mediendateien basierend auf dem File Created Date, Dateinamen oder EXIF-Datum ins Zielverzeichnis.

Unterstützte Dateinamenformate (wird bevorzugt verarbeitet, ohne EXIF-Daten auszulesen):
1. 'YYYY-MM-DD_hh-mm-ss.ext' (z.B. '2024-10-29_19-24-54.mov')
2. 'YYYY-MM-DD.ext' (z.B. '2024-10-29.mov')

Wenn die Datei eines dieser Formate hat, wird das Datum direkt aus dem Dateinamen extrahiert.
Andernfalls versucht das Script, das Datum aus EXIF-Daten zu lesen. Fallback: File Created Date.

- Erstellt ein Unterverzeichnis im Zielverzeichnis basierend auf dem ISO-Datum des Erstellungsdatums.
- Beibehaltung der relativen Unterverzeichnisstruktur des Quellverzeichnisses innerhalb des Datumsverzeichnisses.
- Entfernt leere Verzeichnisse im Eingangsverzeichnis nach dem Verschieben der Dateien.

**Usage**:

```console
$ original-media-integrator import-by-create-date [OPTIONS] SOURCE_DIR DESTINATION_DIR
```

**Arguments**:

* `SOURCE_DIR`: Pfad zum Quellverzeichnis  [required]
* `DESTINATION_DIR`: Pfad zum Zielverzeichnis  [required]

**Options**:

* `--help`: Show this message and exit.

## `original-media-integrator import-by-exif-creation-date`

Importiert Mediendateien basierend auf dem EXIF-Erstellungsdatum (CreationDate).

- Beibehaltung der relativen Unterverzeichnisstruktur des Quellverzeichnisses.
- Organisation der Dateien nach Jahr/Monat/Tag basierend auf EXIF-Daten.
- Unterstützt Videodateien (z. B. MOV, MP4) und Bilddateien (z. B. JPG, PNG).

Beispiel:
    python -m original_media_integrator import-by-exif-creation-date /source /destination

**Usage**:

```console
$ original-media-integrator import-by-exif-creation-date [OPTIONS] SOURCE_DIR DESTINATION_DIR
```

**Arguments**:

* `SOURCE_DIR`: Pfad zum Quellverzeichnis  [required]
* `DESTINATION_DIR`: Pfad zum Zielverzeichnis  [required]

**Options**:

* `--base-source-dir PATH`: Wurzelverzeichnis zur Berechnung des relativen Pfads. Standard: source_dir
* `--help`: Show this message and exit.
