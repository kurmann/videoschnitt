# CLI-Design

## Inhaltsverzeichnis

# CLI-Design

## Inhaltsverzeichnis

1. [Einleitung](#einleitung)
2. [Benennungen und Konsistenz](#benennungen-und-konsistenz)
3. [Modularer Ansatz](#modularer-ansatz)
4. [CLI-Ausgabe und Rückgabewerte](#cli-ausgabe-und-rückgabewerte)
5. [Fehlerbehandlung](#fehlerbehandlung)
6. [Logging und Transparenz](#logging-und-transparenz)
7. [CLI-Methoden vs. Modul-Methoden](#cli-methoden-vs-modul-methoden)
8. [Konsistenz bei Schreiboperationen](#konsistenz-bei-schreiboperationen)
9. [Best Practices für Dateimanipulation](#best-practices-für-dateimanipulation)
10. [Konsistenz und Wartbarkeit](#konsistenz-und-wartbarkeit)

## 1. Einleitung

Eine **Command-Line Interface** (CLI) ermöglicht es Benutzern, Softwareanwendungen über einfache Textbefehle zu steuern. Dies ist besonders nützlich in der Automatisierung und im Skripting, wo Benutzer spezifische Aufgaben schnell und wiederholbar ausführen können, ohne eine grafische Benutzeroberfläche (GUI) zu benötigen. 

Das Ziel dieses Dokumentes ist es, die wichtigsten Prinzipien und Best Practices für das Design und die Implementierung einer benutzerfreundlichen und robusten CLI in Python zu erläutern. Die CLI-Entwicklung erfolgt unter Verwendung von *Typer*, einer modernen und Python-typischen Bibliothek zur schnellen und einfachen Erstellung von **Befehlszeilenanwendungen**.

### 1.1 Warum Typer?

*Typer* bietet eine einfache und deklarative Möglichkeit, **CLI-Methoden** zu erstellen, indem es sich auf die nativen Funktionen von Python wie **Type Hints** und **Annotations** stützt. Dadurch wird die Verbindung zwischen **Methoden** und **CLI-Befehlen** nahezu nahtlos, was die Entwicklung vereinfacht und gleichzeitig den Code lesbar und wartbar macht.

### 1.2 Zielgruppe und Anwendungsbereich

Die hier vorgestellten **CLI-Design**-Prinzipien richten sich an Entwickler, die eine **strukturierte**, **modulare** und **gut dokumentierte** CLI in Python erstellen möchten. Der Fokus liegt auf der Verwaltung und Verarbeitung von Dateien, besonders in Kontexten, in denen **Lese- und Schreiboperationen** häufig sind, wie z.B. beim **Medienset-Management**.

Diese **Best Practices** können für andere Arten von CLIs angepasst werden, da die zugrunde liegenden Konzepte allgemein anwendbar sind.

## 2. CLI-Methoden

Eine **CLI-Methode** ist die Repräsentation einer Funktionalität, die der Benutzer über die Befehlszeile aufruft. Jede Methode führt eine spezifische Aufgabe aus, z. B. das **Listen** von Dateien, das **Integrieren** von Medien oder das **Extrahieren** von Metadaten. In diesem Kapitel wird beschrieben, wie die CLI-Methoden strukturiert werden sollten, um eine klare und intuitive Bedienung zu ermöglichen.

### 2.1 Methodenbenennung

Die Benennung von Methoden ist entscheidend für die Benutzerfreundlichkeit der CLI. Sie sollte immer klar und beschreibend sein, sodass der Benutzer sofort erkennt, welche Aufgabe eine Methode erfüllt. 

#### Best Practices:
- Verwende prägnante und aussagekräftige **Verben** für Befehle:
  - **list**: Wenn Elemente aufgelistet werden, z. B. `list-mediaserver-files`.
  - **get**: Wenn eine spezifische Information abgerufen wird, z. B. `get-recording-date`.
  - **integrate**: Wenn Elemente in ein Zielsystem oder eine Datenbank überführt werden, z. B. `integrate-mediaserver-file`.

- Vermeide abstrakte oder unklare Begriffe. Der Benutzer sollte ohne Nachschlagen sofort verstehen, was der Befehl tut.
- Verwende **Bindestriche** in Befehlen, um mehrere Wörter zu verbinden, z. B. `compress-masterfile`. Dadurch wird der Befehl lesbarer.

#### Beispiele:
```bash
# Listet alle Mediendateien in einem Verzeichnis auf und gruppiert sie nach Mediensets
$ emby-integrator list-mediaserver-files /path/to/mediadirectory

# Extrahiert das Aufnahmedatum aus dem Dateinamen einer Videodatei
$ emby-integrator get-recording-date /path/to/video.mov
```

### 2.2 Methodensignaturen

Jede CLI-Methode sollte eine präzise und vollständige Signatur haben. Die Signatur umfasst sowohl die erwarteten **Parameter** als auch die **Optionen**, die das Verhalten des Befehls beeinflussen. 

#### Best Practices:
- Verwende **klar benannte Parameter** für Pflichtargumente:
  - Beispiel: `source_dir` als Parameter für das Verzeichnis, das nach Dateien durchsucht wird.
  
- Füge **Optionen** mit sinnvollen Standardwerten hinzu, um das Verhalten der Methode flexibel zu gestalten:
  - Beispiel: `json_output: bool = typer.Option(False, help="Gebe die Ausgabe im JSON-Format aus")`, um das Ausgabeformat zu steuern.

- Nutze **Type Hints**, um die erwarteten Typen von Argumenten klar zu machen. Dies ermöglicht Typer auch, automatisch Dokumentationen und Validierungen zu generieren.

#### Beispiele:
```python
@app.command()
def list_mediaserver_files(
    source_dir: str, 
    json_output: bool = typer.Option(False, help="Gebe die Ausgabe im JSON-Format aus")
):
    """
    Liste die Mediaserver-Dateien aus einem Verzeichnis auf und gruppiere sie nach Mediensets.
    """
    # CLI-Logik
```

### 2.3 CLI-Rückgabewerte

CLI-Methoden sollten normalerweise keine komplexen **Rückgabewerte** an andere Funktionen übergeben. Sie geben die Ergebnisse direkt in der Konsole aus, damit der Benutzer die Informationen sofort erhält.

- Bei **Listen**-Methoden kann die Ausgabe strukturiert oder formatiert sein (z.B. **JSON**).
- Bei Methoden, die nur ein einzelnes **Ergebnis** zurückgeben, sollte die Ausgabe ebenfalls klar und formatiert sein, um sie leicht lesbar zu machen.
  
#### Beispiel: Rückgabe von Listen
```bash
Medienset: 2024-08-27 Ann-Sophie Spielsachen Bett
  Videos:    2024-08-27 Ann-Sophie Spielsachen Bett.mov
  Titelbild: Kein Titelbild gefunden.
----------------------------------------
Medienset: 2024-08-11 Böllerwagen Vollgas (Videoclip)
  Videos:    2024-08-11 Böllerwagen Vollgas (Videoclip).mov
  Titelbild: Kein Titelbild gefunden.
----------------------------------------
```

## 3. Fehlerbehandlung und Ausnahmen

Eine solide **Fehlerbehandlung** in der CLI ist entscheidend, um Benutzerfehler und unerwartete Situationen verständlich und kontrolliert zu handhaben. In diesem Kapitel wird erläutert, wie Fehler und Ausnahmen in der CLI behandelt werden sollten und wie die Ausgabe für den Benutzer klar und hilfreich gestaltet werden kann.

### 3.1 Verwendung von Ausnahmen

In **Modul-Methoden** sollten **Ausnahmen** verwendet werden, um Fehler oder ungültige Zustände anzuzeigen. Typische Ausnahmen sind:
- **FileNotFoundError**: Wenn eine Datei oder ein Verzeichnis nicht gefunden wird.
- **ValueError**: Wenn ein ungültiger Wert übergeben wurde oder ein erwartetes Format nicht erfüllt wird.
- **subprocess.CalledProcessError**: Wenn ein externes Kommando (wie `exiftool`) fehlschlägt.

#### Best Practices:
- Nutze **benannte Ausnahmen**, um den Kontext des Fehlers klar darzustellen.
- Stelle sicher, dass die Ausnahme eine prägnante **Fehlermeldung** enthält, die den Grund des Fehlers erläutert.

#### Beispiel:
```python
def get_recording_date(file_path: str) -> str:
    """
    Extrahiert das Aufnahmedatum aus dem Dateinamen.

    Raises:
        ValueError: Wenn der Dateiname kein gültiges Datum enthält.
    """
    match = re.search(r'\d{4}-\d{2}-\d{2}', file_path)
    if not match:
        raise ValueError(f"Kein gültiges Datum im Dateinamen '{file_path}' gefunden.")
    return match.group()
```

### 3.2 Fehlerausgabe in der CLI

Für **CLI-Methoden** sollten Fehler so behandelt werden, dass sie dem Benutzer auf verständliche Weise mitgeteilt werden. Hier kommt **Typer** ins Spiel, das spezielle Methoden wie `typer.secho` zur Ausgabe von farblich hervorgehobenen Fehlern bietet.

#### Best Practices:
- Verwende `typer.secho` für Fehlerausgaben, um diese in **roter Farbe** hervorzuheben.
- Bei schwerwiegenden Fehlern sollte der **Exit-Code** gesetzt werden, um anderen Skripten mitzuteilen, dass der Befehl fehlschlug.

#### Beispiel:
```python
@app.command()
def get_recording_date_command(file_path: str):
    """
    Extrahiert das Aufnahmedatum aus dem Dateinamen und gibt es aus.
    """
    try:
        date = get_recording_date(file_path)
        typer.echo(f"Aufnahmedatum: {date}")
    except ValueError as e:
        typer.secho(f"Fehler: {e}", fg=typer.colors.RED, err=True)
        raise typer.Exit(code=1)
```

### 3.3 Detaillierte Fehlerausgaben

In einigen Fällen kann es sinnvoll sein, neben einer einfachen Fehlermeldung zusätzliche **Fehlerinformationen** anzuzeigen, z. B. den **Exit-Code** eines externen Prozesses oder den **fehlerhaften Befehl**. Dies hilft bei der Fehleranalyse.

#### Best Practices:
- Gib neben der eigentlichen Fehlermeldung auch zusätzliche Informationen aus, die für die Diagnose hilfreich sein könnten (z. B. den Exit-Code oder die genaue Fehlerausgabe).
- Verwende **zusätzliche Optionen** wie `--verbose`, um erweiterte Fehlerinformationen zu steuern.

#### Beispiel:
```python
@app.command()
def get_metadata_command(file_path: str):
    """
    Extrahiert relevante Metadaten aus einer Datei und gibt sie in JSON-Format aus.
    """
    try:
        metadata = get_metadata(file_path)
        typer.echo(json.dumps(metadata, indent=4))
    except subprocess.CalledProcessError as e:
        typer.secho(f"Fehler beim Extrahieren der Metadaten für '{file_path}'.", fg=typer.colors.RED, err=True)
        typer.secho(f"Exit Code: {e.returncode}", fg=typer.colors.RED, err=True)
        typer.secho(f"Fehlerausgabe: {e.stderr}", fg=typer.colors.RED, err=True)
        typer.secho(f"Vollständiger Befehl: {e.cmd}", fg=typer.colors.RED, err=True)
        raise typer.Exit(code=1)
```

### 3.4 Verwendung von Exit-Codes

**Exit-Codes** sind entscheidend, um anzuzeigen, ob ein CLI-Befehl erfolgreich war oder nicht. In der Regel wird der Exit-Code `0` für einen erfolgreichen Abschluss verwendet, während Nicht-Null-Werte auf Fehler hinweisen.

#### Best Practices:
- Setze einen Exit-Code von `0` für **erfolgreiche** Ausführungen.
- Verwende Exit-Codes wie `1` oder `2`, um spezifische Fehler anzuzeigen, z. B.:
  - **1**: Allgemeiner Fehler oder Benutzerfehler.
  - **2**: Fehler beim Zugriff auf externe Ressourcen (Dateien, APIs).

#### Beispiel:
```python
@app.command()
def list_mediaserver_files_command(source_dir: str):
    """
    Liste die Mediaserver-Dateien auf und gebe sie formatiert aus.
    """
    try:
        media_sets = list_mediaserver_files(source_dir)
        typer.echo(json.dumps(media_sets, indent=4))
    except FileNotFoundError as e:
        typer.secho(f"Fehler: {e}", fg=typer.colors.RED, err=True)
        raise typer.Exit(code=1)
```

## 4. Umgang mit Leseoperationen und Ausgabeformaten

Das Design von **Leseoperationen** in der **CLI** ist entscheidend, um die **Benutzerfreundlichkeit** und **Flexibilität** der Schnittstelle zu maximieren. Während in den **Modulen** die Methoden präzise und spezifisch benannt werden, liegt der Fokus bei der CLI auf einer **klaren, intuitiven Benutzung** durch den Endanwender.

In diesem Kapitel konzentrieren wir uns auf die Gestaltung der CLI und wie Leseoperationen sinnvoll strukturiert und benannt werden, um die Benutzerfreundlichkeit zu gewährleisten.

### 4.1 Konsistentes Benennen von Leseoperationen in der CLI

Die Benennung von Leseoperationen folgt klaren Regeln, um die **Intention** der Methode zu verdeutlichen. Bei CLI-Methoden gilt der Grundsatz, dass Leseoperationen, die eine Sammlung von Daten liefern, oft mit **`list`** beginnen, während spezifische Lesevorgänge, die nur ein Element zurückgeben, mit **`get`** benannt werden.

Wichtig: Diese Regeln gelten für das **CLI-Design**, wo die Benennung auf Klarheit und Verständlichkeit abzielt. In den zugrunde liegenden **Modulen** können die Methoden präziser benannt werden, um ihre genaue Funktion besser widerzuspiegeln. Beispielsweise könnte eine CLI-Methode `get_recording_date` die zugrunde liegende Modulmethode `extract_recording_date` aufrufen, die die tatsächliche Logik zur Extraktion des Datums enthält.

#### Best Practices:
- Verwende das Präfix **`list`** in der CLI für Methoden, die **mehrere** Elemente zurückgeben.
- Verwende das Präfix **`get`** in der CLI für Methoden, die **ein einziges** Element zurückgeben.
- Die Benennung der CLI-Methoden soll **benutzerfreundlich** und **selbsterklärend** sein.
- In den **Modulen** hingegen können Methoden spezifischer benannt werden, z. B. durch Begriffe wie `extract`, `parse` oder `fetch`, um deren Logik deutlicher zu machen.

#### Beispiel (CLI-Design):
```python
@app.command()
def list_mediaserver_files(source_dir: str):
    """
    Liste alle Mediaserver-Dateien im angegebenen Verzeichnis auf.
    """
    media_sets = get_mediaserver_files(source_dir)
    # Ausgabe im gewünschten Format...
```

```python
@app.command()
def get_recording_date(file_path: str):
    """
    Extrahiere das Aufnahmedatum aus dem Dateinamen.
    """
    date = extract_recording_date(file_path)  # Modul-Logik
    typer.echo(f"Aufnahmedatum: {date}")
```

### 4.2 Menschliche Ausgabe vs. maschinenlesbare Formate in der CLI

Eine der zentralen Herausforderungen bei Leseoperationen in der CLI besteht darin, sicherzustellen, dass die **Ausgabe** sowohl für den Menschen als auch für **andere Programme** geeignet ist. Hier kommen zwei wesentliche Ausgabeformate ins Spiel:
- **Menschenlesbare Ausgabe**: Diese Ausgabe ist für den Benutzer optimiert und sollte strukturiert, leicht verständlich und visuell ansprechend sein.
- **Maschinenlesbare Ausgabe**: Oft in Form von **JSON**, diese Ausgabe ist so strukturiert, dass sie von anderen Programmen einfach verarbeitet werden kann.

Diese Regel betrifft hauptsächlich die **CLI-Methoden**. In den **Modul-Methoden** sollte der Rückgabewert typischerweise ein **Datenobjekt** oder **Dictionary** sein, das von der CLI-Methode für die Ausgabe aufbereitet wird.

#### Best Practices:
- In der **CLI** sollte die Ausgabe standardmäßig **menschenlesbar** sein.
- Verwende **Optionen** wie `--json-output`, um maschinenlesbare Formate anzubieten.
- In **Modulen** sollte die Ausgabe als strukturierte Daten (z. B. als **Dictionary**) erfolgen, die dann von der CLI weiter verarbeitet werden können.

#### Beispiel (CLI-Design):
```python
@app.command()
def list_mediaserver_files(source_dir: str, json_output: bool = False):
    """
    Liste die Mediaserver-Dateien aus einem Verzeichnis auf.
    """
    media_sets = get_mediaserver_files(source_dir)
    
    if json_output:
        typer.echo(json.dumps(media_sets, indent=4))
    else:
        for set_name, data in media_sets.items():
            typer.echo(f"Medienset: {set_name}")
            typer.echo(f"  Videos: {', '.join(data['videos'])}")
            typer.echo(f"  Titelbild: {data['image'] or 'Kein Titelbild gefunden'}")
```

### 4.3 Ausgabe von Einzelelementen in der CLI

Für Leseoperationen, die **ein einziges Element** zurückgeben (z. B. ein **Datum** oder einen **Namen**), ist es sinnvoll, die Ausgabe in einem **benutzerfreundlichen Format** zu gestalten. Dabei sollte das Ergebnis klar und prägnant sein.

#### Best Practices:
- Gebe das Ergebnis in der **CLI** in einem **formatierten Stil** aus (z. B. für Datum oder Uhrzeit).
- In **Modulen** wird das Ergebnis als Rückgabewert erwartet, der dann von der CLI-Methode verwendet wird.

#### Beispiel (CLI-Design):
```python
@app.command()
def get_recording_date(file_path: str):
    """
    Extrahiere das Aufnahmedatum aus dem Dateinamen und gebe es formatiert aus.
    """
    date = extract_recording_date(file_path)
    formatted_date = date.strftime("%A, %d. %B %Y")  # Formatiere das Datum auf Deutsch
    typer.echo(f"Aufnahmedatum: {formatted_date}")
```

## 5. Umgang mit Schreiboperationen und Kommandos

Schreiboperationen in einer **CLI** stellen eine besondere Herausforderung dar, da sie oft nicht nur lesende Zugriffe durchführen, sondern auch Änderungen an den Daten oder dem Dateisystem vornehmen. Der Umgang mit Schreiboperationen sollte deshalb klar strukturiert sein, um die **Benutzererwartungen** zu erfüllen und gleichzeitig **Sicherheit** sowie **Rückverfolgbarkeit** zu gewährleisten.

Dieses Kapitel beschreibt, wie Schreiboperationen in der CLI strukturiert und benannt werden können, und gibt Best Practices für das Design von Kommandos und Optionen an.

### 5.1 Benennung von Schreiboperationen

Im Gegensatz zu Leseoperationen, bei denen die Verwendung von Begriffen wie **`get`** oder **`list`** vorherrscht, sollten Schreiboperationen in der CLI explizit benannt werden, um dem Benutzer klar zu machen, dass eine **Veränderung** vorgenommen wird. Typische Begriffe für Schreiboperationen sind:

- **`create`**: Erstelle ein neues Element oder eine Ressource.
- **`update`**: Aktualisiere ein bestehendes Element oder eine Ressource.
- **`delete`**: Lösche ein Element oder eine Ressource.
- **`integrate`**: Füge eine Datei oder ein Element in ein anderes System oder Verzeichnis ein.
- **`compress`**: Komprimiere eine Datei oder einen Ordner.
- **`convert`**: Wandle eine Datei von einem Format in ein anderes um.

Die **Wahl der richtigen Begriffe** ist entscheidend, um dem Benutzer die Wirkung des Befehls klar zu machen. Die Benennung sollte deutlich machen, welche Art von Veränderung vorgenommen wird, und im besten Fall das Ergebnis des Befehls beschreiben.

#### Best Practices:
- Verwende **prägnante** und **aktionsorientierte** Verben, um den Zweck der Operation klar zu machen.
- Stelle sicher, dass der Name des Befehls **selbsterklärend** ist, ohne dass der Benutzer die Hilfe aufrufen muss.
- **Vermeide** zu allgemeine Begriffe wie **`run`**, die keine klare Aktion beschreiben.

#### Beispiel (CLI-Design):
```python
@app.command()
def integrate_mediaserver_file(file_path: str, media_library_root: str, media_set_name: str):
    """
    Integriere eine Mediendatei in die Mediathek.
    
    Dieser Befehl verschiebt die Mediendatei in das entsprechende Verzeichnis des angegebenen Mediasets 
    innerhalb der Medienbibliothek.
    """
    success = integrate_mediaserver_file(file_path, media_library_root, media_set_name)
    
    if success:
        typer.secho("Datei erfolgreich integriert.", fg=typer.colors.GREEN)
    else:
        typer.secho("Fehler bei der Integration der Datei.", fg=typer.colors.RED)
```

### 5.2 Rückgabewerte und Bestätigungen bei Schreiboperationen

Es ist wichtig, dass der Benutzer nach der Ausführung einer Schreiboperation eine **Bestätigung** erhält, dass die Aktion erfolgreich war oder ein Fehler aufgetreten ist. Hier spielt die Wahl der **Ausgabe** eine große Rolle.

In vielen Fällen ist es sinnvoll, dem Benutzer das Ergebnis der Schreiboperation deutlich zu signalisieren. Typer bietet hierfür Farboptionen, mit denen Erfolgs- und Fehlermeldungen visuell hervorgehoben werden können. Die Rückmeldung sollte kurz, prägnant und verständlich sein.

#### Best Practices:
- Verwende **typer.secho()** mit Farben, um **Erfolgs- und Fehlermeldungen** visuell zu unterscheiden.
- Stelle sicher, dass der Rückgabewert der Schreiboperation klar signalisiert, ob die Operation erfolgreich war oder nicht.
- Bei **kritischen Operationen** kann eine **Bestätigung** vom Benutzer eingeholt werden, bevor die Operation ausgeführt wird (z. B. beim Löschen).

#### Beispiel (CLI-Design):
```python
@app.command()
def delete_file(file_path: str, confirm: bool = typer.Option(True, prompt="Bist du sicher, dass du diese Datei löschen möchtest?")):
    """
    Lösche eine Datei nach Bestätigung.
    
    Dieser Befehl löscht die angegebene Datei, nachdem der Benutzer eine Bestätigung gegeben hat.
    """
    if os.path.exists(file_path):
        os.remove(file_path)
        typer.secho(f"Datei {file_path} erfolgreich gelöscht.", fg=typer.colors.GREEN)
    else:
        typer.secho(f"Datei {file_path} nicht gefunden.", fg=typer.colors.RED)
```

### 5.3 Fehlermeldungen und Rückfallmechanismen bei Schreiboperationen

Es ist unvermeidlich, dass bei Schreiboperationen Fehler auftreten können – sei es durch fehlende Berechtigungen, nicht existierende Dateien oder Systemprobleme. Daher ist es besonders wichtig, dass diese Fehler korrekt abgefangen und dem Benutzer klar kommuniziert werden.

Gleichzeitig sollten **Schreiboperationen** möglichst **nicht zu "laut"** sein, wenn sie erfolgreich abgeschlossen werden, sondern nur dann **Fehlermeldungen** oder **Bestätigungen** ausgeben, wenn es notwendig ist.

#### Best Practices:
- Verwende **typer.secho()** für **Fehlermeldungen** und setze farbliche Markierungen (z. B. rot für Fehler).
- Implementiere **Fallback-Mechanismen**, wenn eine Aktion fehlschlägt, wie z. B. die Möglichkeit, den Vorgang nach einer Fehlerbehebung erneut zu versuchen.
- Nutze **try-except-Blöcke**, um Fehler korrekt zu behandeln und den Benutzer über die Ursache zu informieren.

#### Beispiel (CLI-Design):
```python
@app.command()
def compress_masterfile(input_file: str, delete_master_file: bool = False):
    """
    Komprimiere eine Master-Datei und lösche optional die Originaldatei nach erfolgreicher Komprimierung.
    """
    try:
        compress_masterfile(input_file)
        typer.secho(f"Datei {input_file} erfolgreich komprimiert.", fg=typer.colors.GREEN)

        if delete_master_file:
            os.remove(input_file)
            typer.secho(f"Originaldatei {input_file} gelöscht.", fg=typer.colors.YELLOW)
    except Exception as e:
        typer.secho(f"Fehler bei der Komprimierung der Datei {input_file}: {e}", fg=typer.colors.RED)
```

### 5.4 Schreiboperationen mit mehreren Dateien

Bei Schreiboperationen, die **mehrere Dateien** betreffen, ist es oft sinnvoll, den Benutzer über den Fortschritt zu informieren, um die **Transparenz** der Operation zu erhöhen. Dies ist besonders wichtig bei länger andauernden Prozessen wie dem **Komprimieren** oder **Verschieben** mehrerer Dateien.

Die Verwendung von **Fortschrittsbalken** oder **zählbaren Iterationen** kann dem Benutzer helfen, die Dauer der Operation besser einzuschätzen.

#### Best Practices:
- Gib bei Operationen mit mehreren Dateien eine **Fortschrittsanzeige** oder **Statusmeldung** aus.
- Verwende **Zähler** oder **Fortschrittsbalken**, um den aktuellen Stand der Operation anzuzeigen.
- Stelle sicher, dass **Fehler** und **Erfolge** bei jeder Datei korrekt gemeldet werden.

#### Beispiel (CLI-Design):
```python
@app.command()
def compress_files_in_directory(directory: str):
    """
    Komprimiere alle Dateien in einem Verzeichnis und gebe den Fortschritt aus.
    """
    files = [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f))]
    total_files = len(files)

    for idx, file in enumerate(files, start=1):
        try:
            compress_masterfile(os.path.join(directory, file))
            typer.secho(f"[{idx}/{total_files}] Datei {file} erfolgreich komprimiert.", fg=typer.colors.GREEN)
        except Exception as e:
            typer.secho(f"[{idx}/{total_files}] Fehler bei der Komprimierung der Datei {file}: {e}", fg=typer.colors.RED)
```

## 6. Kombination von Schreib- und Leseoperationen

In vielen Fällen sind **Schreib- und Leseoperationen** in einer **CLI** eng miteinander verbunden. Typischerweise geht eine Leseoperation einer Schreiboperation voraus, um eine **validierte** oder **korrekte Grundlage** für die spätere Änderung zu schaffen. Ein gutes Design stellt sicher, dass diese Operationen miteinander harmonieren und dem Benutzer eine **nahtlose** Erfahrung bieten.

### 6.1 Best Practices zur Kombination von Lese- und Schreiboperationen

Beim **Design** von CLIs, die Lese- und Schreiboperationen kombinieren, ist es wichtig, dass diese **getrennt** und **modular** bleiben, um klare Verantwortlichkeiten zu definieren. Dies bedeutet, dass:
- **Leseoperationen** nur darauf fokussiert sein sollten, **Daten zu extrahieren** und dem Benutzer zur Verfügung zu stellen.
- **Schreiboperationen** sich darauf konzentrieren, **Änderungen** an den Daten oder Dateien vorzunehmen.

Durch diese klare Trennung wird es einfacher, **Testbarkeit** und **Wartbarkeit** des Codes sicherzustellen.

#### Best Practices:
- **Lese- und Schreiboperationen** sollten in Modulen und Funktionen klar voneinander getrennt werden.
- Es ist hilfreich, für Schreiboperationen **Bestätigungen** (z. B. in der Form von Optionen) anzufordern, bevor irreversible Änderungen vorgenommen werden.
- Wenn Leseoperationen in Schreiboperationen eingebunden sind, sollte dies immer explizit erfolgen und durch entsprechende Statusmeldungen transparent gemacht werden.

### 6.2 Anwendungsbeispiel: Integrieren von Dateien in eine Mediathek

Ein typischer Anwendungsfall, bei dem Lese- und Schreiboperationen kombiniert werden, ist das **Integrieren** von Mediendateien in eine **Mediathek**. Dabei wird zuerst überprüft, ob die Datei **existiert** und **gelesen** werden kann, bevor sie schließlich **integriert** (d. h. verschoben oder kopiert) wird.

#### Beispiel (CLI-Design):
```python
@app.command()
def integrate_file(
    file_path: str, 
    media_library_root: str, 
    media_set_name: str,
    confirm: bool = typer.Option(True, prompt="Bist du sicher, dass du diese Datei integrieren möchtest?")
):
    """
    Integriere eine Datei in ein Mediaset der Mediathek.

    Dieser Befehl überprüft zunächst, ob die Datei existiert und lesbar ist. Danach wird die Datei
    in das angegebene Mediaset innerhalb der Medienbibliothek verschoben oder kopiert.
    
    Best Practices:
    - Erst wird die Datei gelesen und auf ihre Gültigkeit überprüft.
    - Anschließend erfolgt die Schreiboperation, um die Datei zu integrieren.
    - Der Benutzer wird zur Bestätigung der Schreiboperation aufgefordert.
    """
    # Leseoperation: Datei validieren
    if not os.path.exists(file_path):
        typer.secho(f"Die Datei '{file_path}' existiert nicht.", fg=typer.colors.RED)
        raise typer.Exit(code=1)

    # Optional: Weitere Validierung der Datei (z. B. Dateiformat, Metadaten)
    
    # Schreiboperation: Datei integrieren
    success = integrate_mediaserver_file(file_path, media_library_root, media_set_name)
    
    if success:
        typer.secho("Datei erfolgreich integriert.", fg=typer.colors.GREEN)
    else:
        typer.secho("Fehler bei der Integration der Datei.", fg=typer.colors.RED)
```

### 6.3 Fortschritts- und Statusmeldungen bei kombinierten Operationen

Wenn **Lese- und Schreiboperationen** miteinander kombiniert werden, ist es wichtig, den Benutzer kontinuierlich über den Fortschritt der **Verarbeitung** zu informieren. Besonders bei längeren Prozessen, die mehrere Dateien betreffen, sollten **Zwischenergebnisse** oder **Statusmeldungen** eingeblendet werden, um Transparenz zu gewährleisten.

#### Best Practices:
- Verwende **typer.secho()** für **Statusmeldungen** bei der Bearbeitung jeder Datei.
- Bei längeren Prozessen ist eine **Fortschrittsanzeige** hilfreich.
- Stelle sicher, dass **Fehlermeldungen** und **Erfolgsnachrichten** klar voneinander getrennt sind.

#### Beispiel (CLI-Design):
```python
@app.command()
def integrate_files_in_directory(directory: str, media_library_root: str):
    """
    Integriere alle Mediendateien aus einem Verzeichnis in die Mediathek.

    Dieser Befehl liest die Mediendateien aus dem angegebenen Verzeichnis aus, prüft jede Datei
    auf ihre Gültigkeit und integriert sie dann in die Mediathek.
    """
    files = [f for f in os.listdir(directory) if os.path.isfile(os.path.join(directory, f))]
    total_files = len(files)

    for idx, file in enumerate(files, start=1):
        try:
            typer.secho(f"[{idx}/{total_files}] Bearbeite Datei: {file}", fg=typer.colors.YELLOW)

            # Leseoperation: Überprüfe, ob die Datei valide ist
            if not os.path.exists(file):
                typer.secho(f"Datei {file} nicht gefunden. Überspringe.", fg=typer.colors.RED)
                continue

            # Schreiboperation: Integriere die Datei in die Mediathek
            integrate_mediaserver_file(file, media_library_root, "example-set")
            typer.secho(f"[{idx}/{total_files}] Datei {file} erfolgreich integriert.", fg=typer.colors.GREEN)

        except Exception as e:
            typer.secho(f"Fehler bei der Verarbeitung der Datei {file}: {e}", fg=typer.colors.RED)
```

### 6.4 Umgang mit Fehlern bei kombinierten Operationen

Bei der Kombination von Lese- und Schreiboperationen können verschiedene Fehler auftreten – z. B. kann eine Datei nicht lesbar oder nicht verschiebbar sein. In diesen Fällen sollte sichergestellt werden, dass:
- Der **Fehler** korrekt erkannt und dem Benutzer angezeigt wird.
- Der Prozess weitergeführt wird, wenn möglich (z. B. die nächste Datei wird bearbeitet).
- Der Benutzer die Möglichkeit erhält, nach einer Fehlerbehebung erneut zu versuchen, die fehlerhafte Datei zu integrieren.

#### Best Practices:
- Stelle sicher, dass **try-except-Blöcke** Fehler abfangen und dem Benutzer verständlich signalisieren, was passiert ist.
- Verwende farbliche **typer.secho()**-Ausgaben, um Fehler deutlich sichtbar zu machen.
- Gib dem Benutzer bei kritischen Fehlern die Möglichkeit, den Vorgang manuell zu wiederholen.

#### Beispiel (CLI-Design):
```python
@app.command()
def integrate_with_fallback(file_path: str, media_library_root: str):
    """
    Integriere eine Datei in die Mediathek und biete einen Fallback an, falls die Integration fehlschlägt.
    
    Diese Methode versucht, die Datei in die Mediathek zu integrieren, und bietet bei Fehlern eine 
    Möglichkeit, den Vorgang erneut durchzuführen.
    """
    try:
        success = integrate_mediaserver_file(file_path, media_library_root, "example-set")
        if success:
            typer.secho("Datei erfolgreich integriert.", fg=typer.colors.GREEN)
        else:
            typer.secho("Fehler bei der Integration der Datei.", fg=typer.colors.RED)
    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)
        
        # Fallback: Benutzer erhält Möglichkeit, den Vorgang erneut zu versuchen
        retry = typer.confirm("Möchtest du den Vorgang erneut versuchen?")
        if retry:
            integrate_with_fallback(file_path, media_library_root)
```

### Kapitel 7: Befehlsoptionen und Parameterverwendung

Die richtige Verwendung von **Befehlsoptionen** und **Parametern** ist entscheidend für eine benutzerfreundliche und klare CLI-Struktur. In diesem Kapitel wird der Unterschied zwischen beiden erläutert, und es werden **Best Practices** für deren Implementierung dargestellt.

#### 7.1 Unterschied zwischen Optionen und Parametern
- **Optionen** (*Options*) sind typischerweise zusätzliche Einstellungen, die in der CLI mit einem **Flag** aktiviert werden und optional sind. Sie werden durch einen Doppeldash (`--`) gekennzeichnet, gefolgt von einem Namen. Beispiel:
  
  ```bash
  emby-integrator list-mediaserver-files /path/to/media --json-output
  ```

  Hier gibt das **Flag** `--json-output` an, dass die Ausgabe im **JSON-Format** erfolgen soll.

- **Parameter** sind **Pflichtargumente**, die der Benutzer direkt nach dem Befehl angibt. Sie werden ohne zusätzliche Kennzeichnung übergeben. Beispiel:

  ```bash
  emby-integrator compress-masterfile /path/to/file.mov
  ```

  Hier ist `/path/to/file.mov` der **Pfad** zur Eingabedatei, die komprimiert werden soll.

#### 7.2 Wann Optionen verwenden?
- **Optionen** sollten immer dann verwendet werden, wenn die Funktionalität eines Befehls erweitert oder modifiziert wird, ohne dass dies zwingend notwendig ist. Ein typisches Beispiel ist die Wahl eines **Ausgabeformats** (z.B. Text oder JSON).
  
- Wenn eine **Funktion** auch ohne das optionale Flag sinnvoll ausgeführt werden kann, sollte ein Flag anstelle eines Parameters verwendet werden.

#### 7.3 Wann Parameter verwenden?
- **Parameter** sind für Daten notwendig, ohne die ein Befehl nicht ausgeführt werden kann. Sie werden für **zwingend erforderliche Informationen** wie Dateipfade, Eingabedaten oder andere wesentliche Informationen verwendet.
  
- Parameter sollten einfach und klar strukturiert sein, um die Nutzerfreundlichkeit zu maximieren. Es ist ratsam, keine unnötig langen oder komplexen Parameter zu verlangen.

#### 7.4 Typisierung und Standardwerte
- Für **Optionen** und **Parameter** sollte in der Regel eine **Typisierung** angewendet werden, um die Eingabewerte zu validieren. Beispiel:
  
  ```python
  @app.command()
  def compress_masterfile(input_file: str, delete_master_file: bool = typer.Option(False)):
      # Implementation
  ```

  Hier wird der Parameter `input_file` als `str` erwartet, während `delete_master_file` ein **Boolean** ist, der standardmäßig auf `False` gesetzt wird.

- **Standardwerte** sollten verwendet werden, um die CLI benutzerfreundlich zu gestalten, sodass der Benutzer nicht bei jedem Aufruf alle Optionen explizit angeben muss.

#### 7.5 Optionale und zwingende Parameter
- Es ist eine **Best Practice**, so viele Parameter wie möglich optional zu gestalten und nur das zwingend erforderlich zu machen, was wirklich notwendig ist. Dies erhöht die **Flexibilität** der CLI und ermöglicht es Benutzern, nur die wichtigsten Informationen bereitzustellen.

- Typische **optionale Parameter** sind:
  - **Formatierungsoptionen** (`--json-output`)
  - **Flags für Lösch- oder Überschreibaktionen** (`--force`, `--delete-master-file`)
  
  Zwingende Parameter sollten nur verwendet werden, wenn ihre Abwesenheit den Befehl **unbrauchbar** macht.

#### 7.6 Strukturierte Befehlsoptionen
- Bei komplexeren CLIs, die eine Vielzahl von Optionen und Parametern unterstützen, ist es wichtig, dass die Optionen gut dokumentiert und benutzerfreundlich sind. Eine gute **CLI-Dokumentation** listet alle möglichen Optionen auf und erklärt deren Funktionalität. Hier kommt *Typer* ins Spiel, das automatisch generierte Hilfe- und Dokumentationsseiten für Befehle und Optionen bereitstellt.

Durch die gezielte Verwendung von **Befehlsoptionen** und **Parametern** kann eine flexible und benutzerfreundliche CLI gestaltet werden, die den Anforderungen von einfachen und komplexen Benutzerinteraktionen gerecht wird.

### Kapitel 8: Fehlerbehandlung und Ausnahmen in der CLI

Die richtige Behandlung von **Fehlern** und **Ausnahmen** ist ein essenzieller Bestandteil jeder gut gestalteten CLI. In diesem Kapitel werden die Best Practices für das Management von Fehlern in der CLI sowie in den aufgerufenen Modulen erläutert.

#### 8.1 CLI-spezifische Fehlerbehandlung

In der **CLI** ist es wichtig, dem Benutzer verständliche und gut strukturierte Fehlermeldungen anzuzeigen. Hier sind einige Richtlinien:

- **Typer** stellt die Funktion `typer.Exit` zur Verfügung, um die CLI mit einem bestimmten **Exit-Code** zu verlassen. Dies sollte verwendet werden, wenn die CLI aufgrund eines Fehlers nicht ordnungsgemäß beendet werden kann.
  
  Beispiel:

  ```python
  if not os.path.exists(input_file):
      typer.echo(f"Fehler: Die Datei '{input_file}' wurde nicht gefunden.", err=True)
      raise typer.Exit(code=1)
  ```

- **Farbcodierung** von Fehlermeldungen mit **Typer's `secho`** Funktion kann die Lesbarkeit verbessern. Wichtige Fehlermeldungen sollten in **Rot** hervorgehoben werden.

  Beispiel:

  ```python
  typer.secho("Fehler: Der Pfad wurde nicht gefunden.", fg=typer.colors.RED, err=True)
  ```

#### 8.2 Modul-spezifische Fehlerbehandlung

Innerhalb der **Module** sollten Fehler als **Exceptions** behandelt werden, die anschließend in der CLI entsprechend gefangen und interpretiert werden. Dadurch bleibt die Modul-Logik sauber und unabhängig von der CLI-Ausgabe.

- Es ist sinnvoll, benutzerdefinierte **Exceptions** zu erstellen, um spezifische Fehlerzustände klar zu kommunizieren. Zum Beispiel:

  ```python
  class FileNotFoundError(Exception):
      """Wird ausgelöst, wenn eine Datei nicht gefunden wird."""
  ```

- In den CLI-Befehlen können diese **Exceptions** dann gefangen und übersetzt werden, sodass sie dem Benutzer als leicht verständliche Fehlermeldungen angezeigt werden:

  ```python
  try:
      metadata = get_metadata(file_path)
  except FileNotFoundError as e:
      typer.secho(f"Fehler: {str(e)}", fg=typer.colors.RED, err=True)
      raise typer.Exit(code=1)
  ```

#### 8.3 Rückgabewerte und Exit-Codes

In einer gut gestalteten CLI ist es wichtig, dass **Exit-Codes** verwendet werden, um dem Aufrufer den Status der Ausführung mitzuteilen. Die gängigen Codes sind:

- **0**: Erfolgreiche Ausführung
- **1**: Allgemeiner Fehler
- **2**: Benutzerfehler oder ungültige Eingabe

Die **Typer**-Funktion `raise typer.Exit(code=1)` sorgt dafür, dass der Exit-Code korrekt gesetzt wird.

#### 8.4 Verwendung von Logging in Modulen

- Für die Fehler- und Statusmeldungen innerhalb der Module sollte **Logging** verwendet werden. Dadurch bleibt die Möglichkeit offen, die Ausgabe später flexibel zu gestalten. Für die CLI-spezifische Darstellung können die **Fehlermeldungen** dann durch die CLI nach außen sichtbar gemacht werden.
  
  Beispiel für Logging in einem Modul:

  ```python
  import logging

  def get_metadata(file_path: str):
      if not os.path.exists(file_path):
          logging.error(f"Datei {file_path} wurde nicht gefunden.")
          raise FileNotFoundError(f"Datei '{file_path}' nicht gefunden.")
  ```

#### 8.5 Beispiele für eine vollständige Fehlerbehandlung

Ein Beispiel für eine saubere **Fehlerbehandlung** in einer CLI, die sowohl **Typer** als auch **Logging** kombiniert:

```python
@app.command()
def get_metadata_command(file_path: str):
    """Extrahiert Metadaten aus der angegebenen Datei."""
    try:
        metadata = get_metadata(file_path)
        typer.echo(json.dumps(metadata, indent=4))
    except FileNotFoundError as e:
        typer.secho(f"Fehler: {str(e)}", fg=typer.colors.RED, err=True)
        raise typer.Exit(code=1)
    except Exception as e:
        logging.exception("Ein unerwarteter Fehler ist aufgetreten.")
        typer.secho(f"Ein unerwarteter Fehler ist aufgetreten: {str(e)}", fg=typer.colors.RED, err=True)
        raise typer.Exit(code=2)
```

In diesem Beispiel wird eine **benutzerdefinierte Exception** (`FileNotFoundError`) verwendet, und für unerwartete Fehler wird eine **generische Exception** gefangen und geloggt.

#### 8.6 Zusammenfassung der Fehlerbehandlung

- **CLI-spezifische Fehlerbehandlung** sollte durch verständliche Fehlermeldungen und farbliche Hervorhebungen unterstützt werden.
- **Modul-spezifische Fehler** sollten über **Exceptions** gehandhabt und in der CLI entsprechend interpretiert werden.
- **Exit-Codes** sind wichtig, um den Status der CLI-Anwendung korrekt zu signalisieren.
- Die Verwendung von **Logging** ermöglicht es, Fehler und Statusmeldungen flexibel zu gestalten und später anzupassen.

### Kapitel 9: Testen und Validierung der CLI-Methoden

Eine robuste **CLI**-Anwendung sollte gründlich getestet und validiert werden, um sicherzustellen, dass alle Funktionen wie erwartet funktionieren. In diesem Kapitel werden die besten Vorgehensweisen und Techniken zum Testen der **CLI-Methoden** und der zugrunde liegenden **Module** besprochen.

#### 9.1 Testen der CLI mit pytest und typer.testing

Das Testen von **CLI-Anwendungen** kann effektiv mit **pytest** und dem **typer.testing**-Modul durchgeführt werden. **Typer** bietet hierfür eine spezielle **Test-API**, die es ermöglicht, CLI-Aufrufe direkt in den Tests zu simulieren.

##### 9.1.1 Einrichtung der Testumgebung

Bevor Tests geschrieben werden können, muss die **pytest**-Testumgebung eingerichtet werden. Dies umfasst in der Regel die Installation von **pytest** und weiteren Abhängigkeiten:

```bash
pip install pytest typer
```

Sobald die Umgebung eingerichtet ist, kann eine Testdatei für die CLI erstellt werden, z. B. `test_cli.py`.

##### 9.1.2 Testen von CLI-Befehlen mit typer.testing

Das **typer.testing**-Modul stellt einen **CliRunner** zur Verfügung, der es ermöglicht, CLI-Befehle zu simulieren und ihre Ausgabe zu überprüfen.

Ein einfaches Beispiel für das Testen eines CLI-Befehls, der Metadaten extrahiert:

```python
import pytest
from typer.testing import CliRunner
from my_cli_app import app

runner = CliRunner()

def test_get_metadata():
    result = runner.invoke(app, ["get-metadata", "testfile.mov"])
    assert result.exit_code == 0
    assert "FileName" in result.output
```

In diesem Test wird der **`CliRunner`** verwendet, um den **`get-metadata`**-Befehl auszuführen und sicherzustellen, dass der Rückgabewert (Exit-Code) **0** ist und die Ausgabe die erwarteten Schlüssel enthält.

##### 9.1.3 Testen von Fehlermeldungen

Ein weiterer wichtiger Aspekt beim Testen der CLI ist die Validierung von **Fehlermeldungen**. Dies kann beispielsweise in einem Test für das Szenario „Datei nicht gefunden“ geschehen:

```python
def test_get_metadata_file_not_found():
    result = runner.invoke(app, ["get-metadata", "nonexistentfile.mov"])
    assert result.exit_code == 1
    assert "Fehler" in result.output
```

Hier wird überprüft, ob die CLI den richtigen Exit-Code zurückgibt und eine entsprechende Fehlermeldung anzeigt.

#### 9.2 Testen der Module

Während die CLI getestet wird, sollten auch die **Module** unabhängig davon getestet werden, um sicherzustellen, dass die Business-Logik korrekt funktioniert. Hierbei werden die Modul-Methoden direkt aufgerufen und überprüft.

##### 9.2.1 Beispiel für das Testen einer Modul-Methode

Ein einfacher Test für eine Methode, die Metadaten aus einer Datei extrahiert, könnte so aussehen:

```python
from my_module import get_metadata

def test_get_metadata():
    metadata = get_metadata("testfile.mov")
    assert metadata["FileName"] == "testfile.mov"
    assert "CreateDate" in metadata
```

In diesem Test wird überprüft, ob die Methode **`get_metadata`** die korrekten Informationen extrahiert und zurückgibt.

#### 9.3 Testen von Eingabewerten und Validierung

Eine wichtige Komponente des Testens ist die Überprüfung von **Eingabewerten**. Methoden und CLI-Befehle sollten so implementiert sein, dass ungültige Eingaben korrekt abgefangen und valide Eingaben entsprechend verarbeitet werden.

Beispiel für die Validierung von Eingaben in einer CLI-Methode:

```python
def test_invalid_file_format():
    result = runner.invoke(app, ["get-metadata", "testfile.txt"])
    assert result.exit_code == 1
    assert "ungültiges Dateiformat" in result.output
```

Hier wird sichergestellt, dass die CLI eine Fehlermeldung für eine Datei mit einem ungültigen Format zurückgibt.

#### 9.4 Testen von Ausgaben und JSON-Ergebnissen

CLI-Befehle, die **JSON-Daten** zurückgeben, sollten ebenfalls gründlich getestet werden. Hierbei wird die Struktur der JSON-Ausgabe überprüft.

Ein Beispieltest für einen CLI-Befehl, der eine JSON-Ausgabe zurückgibt:

```python
def test_list_mediasets_json():
    result = runner.invoke(app, ["list-mediasets", "--json-output"])
    assert result.exit_code == 0
    json_output = json.loads(result.output)
    assert isinstance(json_output, dict)
    assert "Medienset_Name" in json_output
```

In diesem Test wird die CLI ausgeführt, um eine **JSON-Ausgabe** zu erzeugen. Anschließend wird überprüft, ob die Struktur der Ausgabe korrekt ist und die erwarteten Schlüssel enthält.

#### 9.5 Verwenden von Fixtures in pytest

Um den Testaufwand zu verringern und Wiederverwendbarkeit zu fördern, bietet **pytest** **Fixtures** an. Diese können dazu verwendet werden, Testdaten oder Testdateien vorzubereiten.

Ein Beispiel für die Verwendung eines **Fixtures**, um eine Testdatei bereitzustellen:

```python
import pytest

@pytest.fixture
def testfile(tmp_path):
    file = tmp_path / "testfile.mov"
    file.write_text("Testinhalte")
    return file
```

Mit diesem Fixture können Tests auf eine temporäre Datei zugreifen, die während der Tests erstellt und anschließend automatisch entfernt wird.

#### 9.6 Integrationstests

Zusätzlich zu den **Unit-Tests**, die einzelne Module und Funktionen testen, sollten auch **Integrationstests** durchgeführt werden, um sicherzustellen, dass die CLI als Ganzes korrekt funktioniert. Diese Tests simulieren End-to-End-Abläufe und überprüfen, ob alle Komponenten wie erwartet zusammenarbeiten.

Ein Beispiel für einen Integrationstest, der sicherstellt, dass eine Videodatei korrekt verarbeitet wird:

```python
def test_full_compression_workflow(tmp_path):
    # Vorbereitung: Testdatei erstellen
    test_video = tmp_path / "testvideo.mov"
    test_video.write_text("Testinhalte")

    # CLI-Befehl ausführen
    result = runner.invoke(app, ["compress-masterfile", str(test_video)])
    assert result.exit_code == 0
    assert "Komprimierung abgeschlossen" in result.output
```

#### 9.7 Best Practices für das Testen der CLI

- **Isoliertes Testen**: Teste jede Methode oder CLI-Funktion isoliert, um sicherzustellen, dass sie unabhängig funktioniert.
- **Echte Testdaten verwenden**: Wo möglich, sollten reale Dateiformate und Inhalte verwendet werden, um sicherzustellen, dass die Funktionen auch in der Praxis funktionieren.
- **Fehlerfälle testen**: Teste immer auch unerwartete Szenarien und Fehlerfälle, um sicherzustellen, dass die Anwendung robust und fehlertolerant ist.
- **Automatisierung der Tests**: Verwende **CI/CD-Tools** (Continuous Integration/Continuous Deployment), um die Tests regelmäßig und automatisiert auszuführen.

#### 9.8 Zusammenfassung der Teststrategien

- **pytest** und **typer.testing** bieten leistungsfähige Tools für das Testen von CLI-Anwendungen.
- Sowohl die **CLI** als auch die **Module** sollten unabhängig voneinander getestet werden.
- **Fehlerbehandlung** und **Validierung** von Eingaben sind essenzielle Bestandteile der Teststrategie.
- Integrationstests stellen sicher, dass alle Komponenten korrekt zusammenspielen.
- Die regelmäßige **Automatisierung der Tests** stellt sicher, dass die Anwendung stabil und zuverlässig bleibt.

### Kapitel 10: Konsistenz und Wartbarkeit im CLI-Design

Die **Konsistenz** und **Wartbarkeit** sind entscheidende Faktoren für den langfristigen Erfolg jeder Softwareanwendung, einschließlich **CLI**-Anwendungen. Ein gut durchdachtes und konsistentes Design erleichtert die Nutzung für Anwender und die Wartung für Entwickler. In diesem Kapitel besprechen wir, wie diese beiden Ziele durch bewährte Praktiken und strukturelle Entscheidungen im **CLI-Design** erreicht werden können.

#### 10.1 Konsistenz bei Benennungen und Strukturen

Eine der wichtigsten Säulen der Konsistenz in einer **CLI** ist die Einheitlichkeit bei der Benennung von Befehlen, Optionen und Argumenten. Dadurch wird die **Lernkurve** für die Nutzer reduziert, und sie können Befehle und deren Optionen intuitiv anwenden.

##### 10.1.1 Einheitliche Benennungen

- **Verben für Befehle**: Befehle sollten mit einem Verb beginnen, um klar zu machen, dass eine Aktion ausgeführt wird. Beispiele: `list`, `get`, `create`, `delete`, `update`, `integrate`.
- **Eindeutige und verständliche Benennungen**: Verwende Begriffe, die leicht zu verstehen sind und keine Mehrdeutigkeiten verursachen.
- **Optionen und Argumente**: Halte die Benennung von Optionen und Argumenten konsistent. Beispielsweise sollten Dateipfade immer als `--file` oder `--path` benannt werden.

##### 10.1.2 Konsistenz bei Rückgabewerten

- **Standardisierte Rückgabeformate**: Ein konsistentes Ausgabeformat, wie z.B. die Wahl zwischen einer menschenlesbaren oder **JSON**-basierten Ausgabe, trägt erheblich zur Konsistenz bei.
- **Fehlerbehandlung**: Fehlerausgaben sollten immer im gleichen Format und mit ähnlicher Struktur erfolgen. Dies gilt auch für Fehlercodes, die die CLI zurückgibt.

##### 10.1.3 Konsistentes Layout für CLI-Dokumentation

Auch die **Dokumentation** der CLI-Befehle und deren Optionen sollte konsistent gestaltet sein, um den Nutzern eine einheitliche Hilfe zu bieten. Dies umfasst:
- **Beschreibung der Befehle**: Jede Befehlsbeschreibung sollte klar formuliert und nach dem gleichen Schema aufgebaut sein.
- **Beispiele**: Einheitliche Beispiele für die Nutzung der CLI helfen, die Anwendung zu verstehen.

#### 10.2 Modularität zur Verbesserung der Wartbarkeit

Die **Modularität** der Anwendung spielt eine zentrale Rolle in der Wartbarkeit. Eine auf Module aufgeteilte Struktur erlaubt es, Änderungen an einem Teil der Anwendung vorzunehmen, ohne den gesamten Code zu beeinflussen.

##### 10.2.1 Trennung von Logik und CLI-Methoden

Um die Wartbarkeit zu erhöhen, sollte die eigentliche Logik von der CLI getrennt sein. Die CLI-Methoden sollten nur dazu dienen, die **Module** aufzurufen und deren Ergebnisse entsprechend auszugeben. Dadurch wird es einfacher, die Module unabhängig von der CLI zu testen und zu warten.

Beispiel für eine klare Trennung:

```python
# CLI-Methode
@app.command()
def list_mediaserver_files(source_dir: str):
    media_sets = get_mediaserver_files(source_dir)
    print(json.dumps(media_sets, indent=4))

# Modul-Methode
def get_mediaserver_files(source_dir: str) -> dict:
    # Logik zur Verarbeitung der Dateien
    pass
```

##### 10.2.2 Wiederverwendbare Module

Die Strukturierung in **Module** macht den Code nicht nur übersichtlicher, sondern auch wiederverwendbar. Ein gut strukturiertes Modul kann sowohl in der **CLI** als auch in anderen Teilen der Anwendung verwendet werden. Dies reduziert **Code-Duplikate** und vereinfacht die Wartung.

Beispiel für ein wiederverwendbares Modul:

```python
# Modul: metadata_manager.py
def get_metadata(file_path: str) -> dict:
    # Extrahiert und gibt Metadaten zurück
    pass
```

#### 10.3 Konsistenz bei Fehlermeldungen und Logs

Eine konsistente **Fehlerbehandlung** und einheitliche **Log-Ausgaben** sind ebenfalls wichtige Bestandteile eines gut wartbaren **CLI**-Designs.

##### 10.3.1 Einheitliche Fehlermeldungen

Fehler sollten immer nach einem ähnlichen Schema behandelt und ausgegeben werden. Dies kann mit **Typer** und farblich hervorgehobenen Fehlermeldungen erreicht werden:

```python
def handle_error(message: str):
    typer.secho(f"Fehler: {message}", fg=typer.colors.RED, err=True)
```

Dadurch werden die Fehler nicht nur für den Anwender klarer, sondern auch das Debugging wird erleichtert.

##### 10.3.2 Logging für Wartbarkeit

Das Einbinden von **Logging** in Modul-Methoden sorgt dafür, dass der Zustand der Anwendung zu jedem Zeitpunkt nachvollziehbar ist. Dies ist insbesondere für die Fehlersuche von entscheidender Bedeutung. Die Verwendung des **logging**-Moduls in Python ermöglicht eine flexible Verwaltung der Ausgaben.

Beispiel:

```python
import logging

logging.basicConfig(level=logging.INFO)

def get_metadata(file_path: str) -> dict:
    logging.info(f"Extrahiere Metadaten für {file_path}")
    # Logik zur Extraktion
```

#### 10.4 Versionierung und Dokumentation

Eine klar strukturierte **Versionierung** und fortlaufende **Dokumentation** der **CLI** sind essenziell, um die Wartbarkeit zu gewährleisten. Dies umfasst:
- **Versionshinweise** bei Änderungen oder Erweiterungen der CLI.
- **Automatisierte CLI-Dokumentation** mit Werkzeugen wie **Typer**.
- **Changelog**, um Änderungen zu verfolgen und Rückwärtskompatibilität sicherzustellen.

#### 10.5 Automatisierte Tests und CI/CD-Integration

Automatisierte **Tests** und die Integration der CLI in eine **CI/CD-Pipeline** sind Schlüsselfaktoren für eine langfristige Wartbarkeit. Dadurch können Fehler frühzeitig erkannt und behoben werden, bevor sie in eine Produktionsumgebung gelangen.

#### 10.6 Zusammenfassung

- **Konsistenz** bei der Benennung von Befehlen und Optionen sowie bei der Fehlerbehandlung ist essenziell, um die CLI einfach nutzbar und wartbar zu gestalten.
- **Modularität** und die klare Trennung zwischen Logik und CLI sorgen für eine bessere Wartbarkeit und erleichtern das Testen.
- Einheitliche **Fehlerbehandlung** und **Logs** erleichtern die Fehlersuche und machen die Anwendung robuster.
- Eine kontinuierliche **Versionierung**, **Dokumentation** und die Integration von **automatisierten Tests** fördern eine langfristige Wartbarkeit.
