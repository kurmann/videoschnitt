## Package-Dokumentation mit MkDocs und mkdocstrings

### Struktur der Dokumentation

In größeren Projekten empfiehlt es sich, die Dokumentation in zwei Bereiche zu unterteilen:

- **Manuelle Dokumentation:** Hier werden konzeptuelle und architektonische Inhalte abgelegt, z.B. in einem `manual/`-Verzeichnis. Diese Dokumente sind in Markdown geschrieben und bieten eine Einführung in das Projekt, erklären Designentscheidungen oder beschreiben übergeordnete Prozesse.

- **Generierte Dokumentation:** Die CLI- und API-Dokumentation wird automatisch aus dem Code erzeugt, z.B. in einem `reference/`-Verzeichnis. Dies umfasst die Dokumentation der einzelnen Module und Sub-Packages sowie die Click-CLIs, die du über `mkdocstrings` integrierst.

### Einrichtung von MkDocs mit mkdocstrings

MkDocs ist ein statisches Site-Generator-Tool, das auf Markdown-Dateien basiert. Mit dem Plugin `mkdocstrings` kannst du Python-Docstrings und CLI-Hilfetexte automatisch in die Dokumentation einbinden.

#### Installation

Zuerst installierst du MkDocs und das mkdocstrings-Plugin:

```bash
pip install mkdocs mkdocstrings
```

#### Konfiguration der `mkdocs.yml`

Die `mkdocs.yml` ist die zentrale Konfigurationsdatei für MkDocs. Hier legst du fest, wie die Dokumentation strukturiert ist und welche Plugins verwendet werden.

Ein Beispiel für eine typische Konfigurationsdatei:

```yaml
site_name: Kurmann Videoschnitt Dokumentation
theme:
  name: material

plugins:
  - search
  - mkdocstrings:
      handlers:
        python:
          options:
            docstring_style: google  # Kann auf den verwendeten Docstring-Stil angepasst werden

nav:
  - Home: index.md
  - Konzept und Architektur:
      - Einführung: manual/index.md
      - Architekturübersicht: manual/architecture.md
      - Konzept: manual/concept.md
  - CLI-Dokumentation:
      - Apple Compressor Manager: reference/apple_compressor_manager.md
      - Neumedien Import: reference/neumedien_import.md
```

#### Generierte CLI-Dokumentation mit mkdocstrings

Mit mkdocstrings kannst du die Click-basierte CLI-Dokumentation automatisch in deine Markdown-Dateien einbinden. Das Plugin liest die Python-Module aus, extrahiert die Docstrings und CLI-Informationen und formatiert sie entsprechend.

Ein Beispiel, wie du eine CLI-Dokumentation einbindest:

<pre>
# Apple Compressor Manager CLI

```{autodoc}
module: integrate_new_media.apple_compressor_manager.cli
```
</pre>

In diesem Beispiel wird die CLI-Dokumentation für das Modul `integrate_new_media.apple_compressor_manager.cli` automatisch generiert und als Teil der Markdown-Dokumentation eingebunden.

#### Beispiel für die generierte CLI-Dokumentation

Die Ausgabe von mkdocstrings kann eine vollständige Dokumentation der CLI-Befehle umfassen, einschließlich:

- Beschreibung des Hauptbefehls und der Subcommands.
- Auflistung der Argumente und Optionen mit erklärenden Texten.
- Beispiele für die Nutzung der CLI.

Ein generiertes Markdown-Dokument könnte beispielsweise so aussehen:

<pre>
# Apple Compressor Manager CLI

## Befehle

### `compress-media`

Komprimiert ProRes-Dateien im angegebenen Verzeichnis.

**Argumente:**
- `input_directory` (Pfad): Das Quellverzeichnis für die ProRes-Dateien.
- `output_directory` (Pfad, optional): Das Zielverzeichnis für die komprimierten Dateien. Wenn nicht angegeben, wird das Quellverzeichnis verwendet.

**Optionen:**
- `--level` (Choice: `low`, `medium`, `high`, Standard: `medium`): Bestimmt die Kompressionsstufe.

Beispiel:

```bash
apple-compressor-manager compress-media /path/to/input /path/to/output --level high
```
</pre>

#### Generierung und Bereitstellung der Dokumentation

Nachdem du die Konfiguration und die Markdown-Dateien eingerichtet hast, kannst du die Dokumentation generieren und lokal anzeigen lassen:

```bash
mkdocs serve
```

Dieser Befehl startet einen lokalen Entwicklungsserver, der die Dokumentation unter `http://127.0.0.1:8000` verfügbar macht.

Wenn du bereit bist, die Dokumentation zu veröffentlichen, kannst du sie mit folgendem Befehl bauen:

```bash
mkdocs build
```

Die generierten HTML-Dateien befinden sich dann im Verzeichnis `site/` und können direkt auf GitHub Pages oder einem anderen Hosting-Service bereitgestellt werden.

### Tipps für die Organisation der Dokumentation

- **Trenne manuelle Dokumente von der generierten Referenzdokumentation:** Lege konzeptuelle und architektonische Inhalte in einem eigenen Verzeichnis (`manual/`) ab und die automatisch generierten Referenzen in einem separaten Verzeichnis (`reference/`).
- **Verlinke zwischen den Bereichen:** Falls es Sinn ergibt, kannst du von der manuellen Dokumentation auf die generierte CLI- oder API-Dokumentation verlinken, um eine durchgängige Benutzererfahrung zu bieten.
- **Nutze die `nav`-Struktur in `mkdocs.yml`:** Organisiere deine Inhalte logisch, sodass Nutzer einfach zwischen den verschiedenen Bereichen der Dokumentation navigieren können.
