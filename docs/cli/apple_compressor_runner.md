# `apple-compressor-runner`

Vergibt Komprimierungsjobs für alle ProRes-Dateien im angegebenen Verzeichnis und für jedes angegebene Profil.

## Argumente:
- **input_dir** (*str*): Pfad zum Eingangsverzeichnis, das nach ProRes-Dateien durchsucht wird.
- **compressor_profiles** (*str*): Durch Kommas getrennte Liste von Compressor-Profilnamen.

**Usage**:

```console
$ apple-compressor-runner [OPTIONS] INPUT_DIR COMPRESSOR_PROFILES
```

**Arguments**:

* `INPUT_DIR`: Pfad zum Eingangsverzeichnis  [required]
* `COMPRESSOR_PROFILES`: Durch Kommas getrennte Liste von Compressor-Profilnamen  [required]

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.
