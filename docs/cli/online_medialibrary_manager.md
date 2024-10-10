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

* `create-artwork`: Erzeugt ein Titelbild aus der angegebenen...
* `create-html`: Generiert eine statische HTML-Seite für...
* `create-og-image`: Erzeugt ein OpenGraph-Bild aus dem...

## `online-medialibrary-manager create-artwork`

Erzeugt ein Titelbild aus der angegebenen Eingabedatei. Wenn kein Zielpfad angegeben ist,
wird das Titelbild im gleichen Verzeichnis erstellt und erhält das Suffix '-Titelbild'.

**Usage**:

```console
$ online-medialibrary-manager create-artwork [OPTIONS] INPUT_IMAGE
```

**Arguments**:

* `INPUT_IMAGE`: Pfad zur Eingabedatei (PNG oder JPG/JPEG)  [required]

**Options**:

* `--output-image TEXT`: Optionaler Pfad zur Ausgabedatei
* `--help`: Show this message and exit.

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

* `--output-file TEXT`: Name der Ausgabedatei für das HTML (Standard: 'index.html')  [default: index.html]
* `--download-file TEXT`: Optionaler Pfad zur Download-Datei (z.B. ZIP-Datei)
* `--base-url TEXT`: Basis-URL für die OG-Metadaten (z.B. https://example.com/videos)
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
