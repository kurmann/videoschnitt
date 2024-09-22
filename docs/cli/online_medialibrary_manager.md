# `online-medialibrary-manager`

Online Medialibrary Manager für Familienvideos

**Usage**:

```console
$ online-medialibrary-manager [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `create-html`: Generiert eine statische HTML-Seite für...
* `create-og-image`: Erzeugt ein OpenGraph-Bild aus dem...

## `online-medialibrary-manager create-html`

Generiert eine statische HTML-Seite für das Familienvideo und erstellt ein OpenGraph-Bild.

**Usage**:

```console
$ online-medialibrary-manager create-html [OPTIONS] METADATA_SOURCE HIGH_RES_FILE MID_RES_FILE ARTWORK_IMAGE
```

**Arguments**:

* `METADATA_SOURCE`: Pfad zur Videodatei, aus der die Metadaten extrahiert werden sollen  [required]
* `HIGH_RES_FILE`: Pfad zur hochauflösenden Videodatei (4K HEVC)  [required]
* `MID_RES_FILE`: Pfad zur mittelauflösenden Videodatei (HD)  [required]
* `ARTWORK_IMAGE`: Pfad zum Vorschaubild  [required]

**Options**:

* `--output-file TEXT`: Name der Ausgabedatei für das HTML (Standard: index.html)  [default: index.html]
* `--download-file TEXT`: Optionaler Pfad zur Download-Datei (z.B. ZIP-Datei)
* `--base-url TEXT`: Basis-URL für die OG-Metadaten (z.B. https://example.com/videos)
* `--help`: Show this message and exit.

## `online-medialibrary-manager create-og-image`

Erzeugt ein OpenGraph-Bild aus dem gegebenen Vorschaubild.

**Usage**:

```console
$ online-medialibrary-manager create-og-image [OPTIONS] ARTWORK_IMAGE
```

**Arguments**:

* `ARTWORK_IMAGE`: Pfad zum Vorschaubild  [required]

**Options**:

* `--output-image TEXT`: Name der Ausgabedatei für das OpenGraph-Bild (Standard: og-image.jpg)  [default: og-image.jpg]
* `--help`: Show this message and exit.
