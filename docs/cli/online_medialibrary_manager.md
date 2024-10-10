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

* `create-html-command`: Generiert eine statische HTML-Seite für...
* `create-og-image`: Erzeugt ein OpenGraph-Bild aus dem...

## `online-medialibrary-manager create-html-command`

Generiert eine statische HTML-Seite für das Familienvideo und erstellt ein OpenGraph-Bild.

Args:
    metadata_source (str): Pfad zur Videodatei, aus der die Metadaten extrahiert werden sollen.
    high_res_file (str): Pfad zur hochauflösenden Videodatei (4K HEVC).
    mid_res_file (str): Pfad zur mittelauflösenden Videodatei (HD).
    artwork_image (str): Pfad zum Vorschaubild.
    output_file (str): Name der Ausgabedatei für das HTML (Standard: 'index.html').
    download_file (str): Optionaler Pfad zur Download-Datei (z.B. ZIP-Datei).
    base_url (str): Basis-URL für die OG-Metadaten (z.B. https://example.com/videos).

**Usage**:

```console
$ online-medialibrary-manager create-html-command [OPTIONS] METADATA_SOURCE HIGH_RES_FILE MID_RES_FILE ARTWORK_IMAGE
```

**Arguments**:

* `METADATA_SOURCE`: [required]
* `HIGH_RES_FILE`: [required]
* `MID_RES_FILE`: [required]
* `ARTWORK_IMAGE`: [required]

**Options**:

* `--output-file TEXT`: [default: index.html]
* `--download-file TEXT`
* `--base-url TEXT`
* `--help`: Show this message and exit.

## `online-medialibrary-manager create-og-image`

Erzeugt ein OpenGraph-Bild aus dem gegebenen Vorschaubild.
Nur JPG/JPEG-Dateien werden akzeptiert.
Wenn kein Ausgabepfad angegeben wird, wird das gleiche Verzeichnis verwendet,
und das Bild erhält das Suffix '-OG'.

Args:
    artwork_image (str): Pfad zum Eingabebild.
    output_image (str, optional): Pfad zur Ausgabedatei. Wenn nicht angegeben,
                                  wird der Dateiname des Eingabebilds verwendet, 
                                  mit dem Suffix '-OG'.

Returns:
    str: Der Pfad zum erstellten OpenGraph-Bild.

**Usage**:

```console
$ online-medialibrary-manager create-og-image [OPTIONS] ARTWORK_IMAGE
```

**Arguments**:

* `ARTWORK_IMAGE`: [required]

**Options**:

* `--output-image TEXT`
* `--help`: Show this message and exit.
