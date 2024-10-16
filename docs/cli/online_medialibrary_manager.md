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

* `create-artwork`: Erzeugt ein Titelbild aus einer Eingabedatei.
* `create-html`: Erstellt eine statische HTML-Seite.
* `create-og-image`: Erstellt ein OpenGraph-Bild.

## `online-medialibrary-manager create-artwork`

Erzeugt ein Titelbild aus einer Eingabedatei.

Diese Methode konvertiert ein Bild in das JPG-Format und wendet das Adobe RGB-Farbprofil an.
Wenn kein Zielpfad angegeben wird, wird das Titelbild im gleichen Verzeichnis erstellt und 
erhält das Suffix '-Titelbild'.

Args:
    input_image (str): Pfad zur Eingabedatei.
    output_image (str, optional): Optionaler Pfad zur Ausgabedatei.

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

Erstellt eine statische HTML-Seite.

Diese Methode verwendet die bereitgestellten Videodateien und Metadaten, um eine HTML-Seite zu generieren,
die die Videos in verschiedenen Auflösungen anzeigt. Zusätzlich wird ein OpenGraph-Bild erstellt, das für
die Vorschau auf sozialen Medien verwendet werden kann.

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

* `--subtitle TEXT`: Optionaler Untertitel für die Seite (z.B. Ukrainisch)
* `--download-file TEXT`: Optionaler Pfad zur Download-Datei (z.B. ZIP-Datei)
* `--base-url TEXT`: Basis-URL für die OG-Metadaten (z.B. https://example.com/videos)
* `--help`: Show this message and exit.

## `online-medialibrary-manager create-og-image`

Erstellt ein OpenGraph-Bild.

Diese Methode skaliert und beschneidet das gegebene Vorschaubild und erstellt ein OpenGraph-Bild für die Verwendung in sozialen Medien. Wenn kein Zielpfad angegeben ist, wird das Bild im gleichen Verzeichnis wie das Eingabebild gespeichert und erhält das Suffix '-OG'.

Args:
    artwork_image (str): Pfad zum Vorschaubild.
    output_image (str): Name der Ausgabedatei für das OpenGraph-Bild (Standard: 'og-image.jpg').

**Usage**:

```console
$ online-medialibrary-manager create-og-image [OPTIONS] ARTWORK_IMAGE
```

**Arguments**:

* `ARTWORK_IMAGE`: [required]

**Options**:

* `--output-image TEXT`
* `--help`: Show this message and exit.
