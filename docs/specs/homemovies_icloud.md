## **iCloud Ablagespezifikation für Familienfilme**

**Version 1.0 vom 26. Oktober 2024**  
**Autor: Patrick Kurmann**

### Inhaltsverzeichnis

1. Überblick  
2. Verzeichnisstruktur  
3. Dateinamenkonventionen  
4. Metadaten-Integration  
5. Automatisierter Workflow  
6. Vorteile  
7. Beispiele  
8. Fazit  
9. Erweiterungen und Empfehlungen

### 1. Überblick

Diese Spezifikation beschreibt die Organisation und Ablage von Familienfilmen in der iCloud-Mediathek. Ziel ist es, eine strukturierte und kompatible Ablage zu gewährleisten, die das einfache Teilen von Videos mit Freunden und Familienmitgliedern ermöglicht. Die Struktur orientiert sich an der Emby Mediathek, jedoch ohne die Verwendung von NFO-Dateien und separaten Titelbildern. Die iCloud-Mediathek dient als Backup und als Möglichkeit, Videos in voller Qualität außerhalb des Familienkreises zu teilen. Der Emby-Media-Server fungiert als Single Point of Truth, und die iCloud-Ablage ist eine Ableitung davon.

---

### 2. Verzeichnisstruktur

Die Verzeichnisstruktur in iCloud folgt einem klaren und einfachen Aufbau, der die Verwaltung und das Teilen der Medien erleichtert.

#### Struktur:

```
/Familienfilme/
└── [Album]/
    └── [Jahr]/
        └── [Aufnahmedatum]_[Titel].m4v
```

**Beispiel:**

```
/Familienfilme/
└── Kurmann Glück Highlights/
    └── 2024/
        └── 2024-09-15_Paula_MTB-Finale_Huttwil.m4v
```

**Hinweise:**
- **Familienfilme:** Das übergeordnete Verzeichnis für alle Familienfilme.
- **Album:** Ein Unterverzeichnis innerhalb von "Familienfilme", das als spezifische Kategorie oder Sammlung dient (z.B. "Kurmann Glück Highlights"). Jedes Album kann unterschiedliche Berechtigungen und Freigaben haben, ähnlich wie in der Emby Mediathek.
- **Jahr:** Ein Unterverzeichnis innerhalb eines Albums, das die Filme nach dem Aufnahmedatum gruppiert.
- **Datei:** Die Videodatei ist entweder im QuickTime-Format (`.mov`) oder im MPEG-4-Format (`.m4v`) gemäß Apple-Vorgaben. Es wird nur eine Videodatei pro Eintrag verwendet, ohne separate komprimierte Versionen.

### 3. Dateinamenkonventionen

Die Konsistenz der Dateinamen ist entscheidend für die einfache Verwaltung und das Teilen der Medien.

#### Aufbau des Dateinamens:

```
[Aufnahmedatum]_[Titel].ext
```

- **Aufnahmedatum:** ISO-Format YYYY-MM-DD.
- **Titel:** Klarer und prägnanter Titel des Films.
- **Erweiterung:** `.m4v` für MPEG-4 Dateien oder `.mov` für QuickTime Dateien.

#### Beispiele:

- **QuickTime Datei:**

  ```
  2024-09-15_Paula_MTB-Finale_Huttwil.mov
  ```

- **MPEG-4 Datei:**

  ```
  2024-09-15_Paula_MTB-Finale_Huttwil.m4v
  ```

**Hinweise:**

- **Eindeutigkeit:** Jeder Dateiname innerhalb eines Albums und eines Jahres muss eindeutig sein.
- **Fassung:** Zusätzliche Fassungen können durch Hinzufügen eines Suffixes in eckigen Klammern ergänzt werden, z.B. `[Fassung für Verein]`.

### 4. Metadaten-Integration

In der iCloud-Mediathek werden keine separaten NFO-Dateien oder Posterbilder verwendet. Stattdessen werden Metadaten direkt in die Videodatei eingebettet.

- **Titelbild:** Das Titelbild wird mittels geeigneter Tools (z.B. `ffmpeg`) in die Videodatei eingebettet.
- **Metadaten:** Offizielle MPEG-4-Tags werden verwendet, um Metadaten wie Titel, Datum, Beschreibung und beteiligte Personen zu speichern. Zusätzliche Informationen wie Sammlungen und Tags werden im Beschreibungsfeld ergänzt.
- **Farbprofil:** Das Titelbild wird im Adobe RGB Farbraum eingebettet, um Kompatibilitätsprobleme aufgrund des Farbprofils zu vermeiden.
- **Beschreibungstext:** Die Beschreibung enthält neben Sammlungen und Tags auch die beteiligten Personen in folgendem strukturierten Format:

  ```
  Text: So. 15.09.24: Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
  
  Sammlung: Familienausflüge, Familie, Ausflüge
  Tags: Apple Log, Sport
  Videoschnitt: Patrick Kurmann
  Filmaufnahmen: Patrick Kurmann, Silvan Kurmann
  Beteiligte: Paula Gorycka (Rennfahrerin), Andreas Kurmann sr (Team-Chef)
  ```

- **Kompatibilität:** Die eingebetteten Metadaten sind mit jeder gängigen Medienanwendung kompatibel, wodurch die Dateien unabhängig von Emby verwendet werden können.
- **Einschränkungen:** Bestimmte spezifische Metadaten wie detaillierte Sammlungen und Tags, die in den NFO-Dateien verwendet werden, fehlen, da sie nicht in den standardmäßigen MPEG-4-Tags enthalten sind. Diese Informationen werden jedoch im Beschreibungsfeld ergänzt.

**Zusätzlicher Vorteil:**

Die Einbettung spezifischer Metadaten im `description`-Tag ermöglicht es, diese Informationen ohne einen Metadaten-Editor leicht zugänglich zu machen. Beispielsweise können im Finder (macOS) oder unter den Eigenschaften (Windows) die Titel und Beschreibungen inklusive des Aufnahmedatums angezeigt werden. Zudem werden diese Metadaten wahrscheinlich indexiert, sodass eine Suche nach Begriffen wie "Paula Gorycka" auch dann Ergebnisse liefert, wenn der Name nicht im Dateititel vorkommt. Dies vereinfacht die Auffindbarkeit der Videos erheblich.

### 5. Automatisierter Workflow

Ein automatisierter Prozess sorgt dafür, dass die Dateien korrekt in die iCloud-Mediathek abgelegt werden.

#### Schritte:

1. **Eingangsparameter:**
   - **Titelbild:** PNG-Datei
   - **Videodatei:** Originalvideo im `.mov`- oder `.m4v`-Format

2. **Automatisierte Verarbeitung:**
   - **Metadaten einbetten:** Ein automatischer Algorithmus bettet das Titelbild und die Metadaten direkt in die bestehende Videodatei ein.
   - **Beschreibung erweitern:** Die Beschreibung enthält Sammlungen, Tags und die Liste der beteiligten Personen mit ihren Rollen.
   - **Datei umbenennen:** Die Datei wird gemäß den Dateinamenkonventionen benannt.

3. **Ablage im macOS-Dateisystem:**
   - **Strukturierte Verzeichnisse:** Die verarbeiteten Dateien werden in die entsprechende Sammlung und Jahresverzeichnisse verschoben.
   - **Automatische Synchronisation:** Durch die Integration mit dem macOS-Dateisystem werden die Dateien automatisch mit der iCloud synchronisiert, ohne dass der Algorithmus direkt mit iCloud interagiert.

4. **Teilen-Funktion:**
   - **Einzelne Dateien:** Können individuell in voller Auflösung an Freunde und Familie geteilt werden.
   - **Verzeichnisse:** Können mit Familienmitgliedern geteilt werden, sodass neue Dateien automatisch verfügbar sind.

**Hinweis:** Der automatisierte Algorithmus interagiert mit dem macOS-Dateisystem, um die Dateien korrekt abzulegen. Eine direkte Interaktion mit iCloud ist nicht erforderlich, da macOS die Synchronisation übernimmt.

### 6. Vorteile

- **Maximale Kompatibilität:** Durch die Nutzung offizieller MPEG-4-Tags können die Dateien in jeder gängigen Medienanwendung geöffnet und angezeigt werden.
- **Einfaches Teilen:** Ermöglicht das unkomplizierte Teilen einzelner Videodateien oder ganzer Verzeichnisse mit Freunden und Familienmitgliedern.
- **Hohe Qualität:** Originalvideos bleiben im `.mov`- oder `.m4v`-Format erhalten, ohne separate komprimierte Versionen.
- **Zentrale Verwaltung:** Die einheitliche Struktur erleichtert die Verwaltung und Auffindbarkeit der Medien in der iCloud.
- **Flexibilität:** Da die Metadaten in die Videodatei eingebettet sind, sind die Dateien unabhängig von spezifischen Media-Servern nutzbar.
- **Backup-Funktion:** Dient als zuverlässiges Backup für die Emby Mediathek und stellt sicher, dass die Videos auch außerhalb des Familienkreises zugänglich sind.
- **Verbesserte Suche:** Eingebettete Metadaten im `description`-Tag ermöglichen eine effiziente Suche und Auffindbarkeit der Videos in Dateimanagern und Suchsystemen.
- **Ansprechende Darstellung:** Das eingebettete Titelbild wird in der iOS Files-App schön dargestellt, was eine ansprechende Übersicht der Videos ermöglicht, auch ohne Zugang zum Emby Media Server.

### 7. Beispiele

#### 7.1 Projektmetadaten in Final Cut Pro

- **Titel:** `2024-09-15 Paula MTB-Finale Huttwil`
- **Beschreibung:**
  ```
  Text: So. 15.09.24: Dieses Video zeigt den XCO-Finale der ÖKK Bike Revolution 2024 in Huttwil.
  
  Sammlung: Familienausflüge, Familie, Ausflüge
  Tags: Apple Log, Sport
  Videoschnitt: Patrick Kurmann
  Filmaufnahmen: Patrick Kurmann, Silvan Kurmann
  Beteiligte: Paula Gorycka (Rennfahrerin), Andreas Kurmann sr (Team-Chef)
  ```
- **Album:** `Kurmann Glück Highlights`
- **Titelbild:** `titelbild.png` (Adobe RGB, PNG)

#### 7.2 Generierte Datei in iCloud

```
/Familienfilme/
└── Kurmann Glück Highlights/
    └── 2024/
        └── 2024-09-15_Paula_MTB-Finale_Huttwil.m4v
```

### 8. Fazit

Diese Spezifikation bietet eine präzise und effiziente Methode zur Organisation und Ablage von Familienfilmen in der iCloud-Mediathek. Durch die konsistente Verzeichnisstruktur und die Einbettung von Metadaten direkt in die Videodateien wird das Teilen und die Nutzung der Medien erleichtert. Der automatisierte Workflow sorgt für eine reibungslose Verarbeitung und Ablage der Dateien, während die maximale Kompatibilität der Metadaten eine flexible Nutzung in verschiedenen Anwendungen ermöglicht. Die einheitliche Struktur ermöglicht zudem eine einfache Verwaltung und Auffindbarkeit der Medien sowohl innerhalb als auch außerhalb des Familienkreises. Die iCloud-Mediathek dient als zuverlässiges Backup und als vielseitige Plattform, um Videos in voller Qualität mit Freunden und Familie zu teilen, insbesondere wenn der Emby Media Server nicht erreichbar ist.

### 9. Erweiterungen und Empfehlungen

#### 9.1 Erweiterter Synchronisationsalgorithmus (Optional)

Es besteht die Möglichkeit, einen zusätzlichen Synchronisationsalgorithmus zu implementieren, der die Metadaten vom Emby Media Server periodisch oder bei Änderungen automatisch in die Videodateien auf der iCloud einbettet. Dieser Algorithmus würde sicherstellen, dass die iCloud-Ablage stets aktuell und konsistent mit dem Emby Media Server bleibt, wobei der Emby-Media-Server als Single Point of Truth fungiert.

**Funktionen des Synchronisationsalgorithmus:**

- **Periodische Überprüfung:** Regelmäßige Abfrage des Emby Media Servers auf Metadatenänderungen.
- **Automatisches Aktualisieren:** Einbettung der aktualisierten Metadaten in die entsprechenden Videodateien auf der iCloud.
- **Konsistenzsicherung:** Gewährleistung, dass alle Metadaten in der iCloud-Ablage mit denen im Emby Media Server übereinstimmen.

**Vorteile:**

- **Automatisierte Aktualisierung:** Reduziert den manuellen Aufwand zur Pflege der Metadaten in der iCloud.
- **Konsistente Metadaten:** Sicherstellt, dass alle Ablagen stets die gleichen Informationen enthalten.
- **Zukunftssicher:** Ermöglicht eine nahtlose Erweiterung des Systems bei zukünftigen Anforderungen.

**Hinweis:** Dieser Synchronisationsalgorithmus ist optional und nicht Teil der grundlegenden Vorgaben. Er kann bei Bedarf separat entwickelt und implementiert werden.

#### 9.2 Vorteile der eingebetteten Metadaten im `description`-Tag

Die Einbettung spezifischer Metadaten im `description`-Tag bietet mehrere Vorteile:

- **Einfache Zugänglichkeit:** Metadaten wie Titel, Beschreibung und beteiligte Personen sind ohne spezielle Metadaten-Editoren zugänglich. Sie können direkt im Finder (macOS) oder in den Eigenschaften (Windows) eingesehen werden.
- **Verbesserte Suchbarkeit:** Metadaten im Beschreibungsfeld werden wahrscheinlich von Suchsystemen indexiert, was die Auffindbarkeit der Videos erhöht. Beispielsweise erscheint ein Video in den Suchergebnissen, wenn nach einem im Beschreibungsfeld genannten Namen gesucht wird, auch wenn dieser nicht im Dateititel vorkommt.
- **Zentrale Information:** Alle wichtigen Informationen sind in der Videodatei selbst enthalten, was die Verwaltung und Nutzung vereinfacht.

#### 9.3 Ansprechende Darstellung des Titelbildes in der iOS Files-App

Durch das Einbetten des Titelbildes im Adobe RGB Farbraum wird sichergestellt, dass das Vorschaubild in der iOS Files-App ansprechend und korrekt dargestellt wird. Dies bietet folgende Vorteile:

- **Visuelle Übersicht:** Familienmitglieder und Freunde können auf einen Blick erkennen, worum es in den Videos geht, ohne die Datei öffnen zu müssen.
- **Ästhetische Darstellung:** Das eingebettete Titelbild sorgt für eine attraktive und konsistente Darstellung der Videos in der iOS Files-App.
- **Verbesserte Benutzererfahrung:** Eine ansprechende Vorschau erleichtert das Durchsuchen und Finden der gewünschten Videos, insbesondere wenn viele Dateien vorhanden sind.

**Ziel:** Auch ohne Zugang zum Emby Media Server bietet die iCloud-Mediathek eine übersichtliche und ästhetische Darstellung der Videos, was die Nutzung und das Teilen weiter erleichtert.

**Ende der Spezifikation**
