# src/emby_integrator/commands/scan_media.py

import json
import typer
from pathlib import Path
from emby_integrator.media_scanner import scan_media_directory

def scan_media_command(media_dir: Path, json_output: bool = False):
    """
    Scannt ein Verzeichnis nach Bilddateien (.png, .jpg, .jpeg) und QuickTime-Dateien (.mov),
    gruppiert sie als Mediaserver-Set basierend auf den Bilddateien und listet unvollständige Gruppen auf.

    ## Argumente:
    - **media_dir** (*Path*): Pfad zum Verzeichnis, das gescannt werden soll.
    - **json_output** (*bool*): Optional. Wenn gesetzt, wird die Ausgabe im JSON-Format dargestellt. Standard ist `False`.

    ## Beispielaufrufe:
    ```bash
    emby-integrator scan-media /Pfad/zum/Verzeichnis
    ```

    Ausgabe:
    ```plaintext
    Mediaserver-Set: 2024-09-08 Ann-Sophie rennt Testvideo
        Image: 2024-09-08 Ann-Sophie rennt Testvideo.png
        Video: 2024-09-08 Ann-Sophie rennt Testvideo-1080p-Internet 4K60-Medienserver.mov

    Unvollständige Videodateien (ohne Bilder):
        - 2024-09-25 Event ohne Bild-2.mov
    ```

    Mit JSON-Option:
    ```bash
    emby-integrator scan-media /Pfad/zum/Verzeichnis --json
    ```

    Ausgabe im JSON-Format:
    ```json
    {
        "complete_sets": {
            "2024-09-08 Ann-Sophie rennt Testvideo": {
                "image": "2024-09-08 Ann-Sophie rennt Testvideo.png",
                "video": "2024-09-08 Ann-Sophie rennt Testvideo-1080p-Internet 4K60-Medienserver.mov"
            }
        },
        "incomplete_sets": [
            {
                "image": "2024-09-08 Ann-Sophie rennt Testvideo.png",
                "videos": ["2024-09-08 Ann-Sophie rennt Testvideo-720p-Hochwertig.mov"]
            },
            {
                "image": "2024-09-26 Another Event.png",
                "videos": []
            }
        ],
        "unmatched_videos": [
            "2024-09-25 Event ohne Bild-2.mov"
        ]
    }
    """
    try:
        complete_sets, incomplete_sets, unmatched_videos = scan_media_directory(str(media_dir))

        if json_output:
            # Struktur für JSON-Ausgabe
            output = {
                "complete_sets": complete_sets,
                "incomplete_sets": incomplete_sets,
                "unmatched_videos": unmatched_videos
            }
            print(json.dumps(output, indent=4, ensure_ascii=False))
        else:
            # Menschenlesbare Ausgabe
            if not complete_sets and not incomplete_sets and not unmatched_videos:
                typer.secho("Keine geeigneten Medien im angegebenen Verzeichnis gefunden.", fg=typer.colors.YELLOW)
                return

            if complete_sets:
                for group_name, files in complete_sets.items():
                    typer.secho(f"Mediaserver-Set: {group_name}", fg=typer.colors.BLUE, bold=True)
                    if 'image' in files:
                        print(f"    Image: {files['image']}")
                    if 'video' in files:
                        print(f"    Video: {files['video']}")
                    print()  # Leerzeile zwischen den Gruppen

            if incomplete_sets:
                typer.secho("Unvollständige Mediengruppen:", fg=typer.colors.RED, bold=True)
                for group in incomplete_sets:
                    image = group.get("image", "Unbekanntes Bild")
                    videos = group.get("videos", [])
                    print(f"    Image: {image}")
                    if videos:
                        for video in videos:
                            print(f"        Video: {video}")
                print()

            if unmatched_videos:
                typer.secho("Unvollständige Videodateien (ohne Bilder):", fg=typer.colors.RED, bold=True)
                for video in unmatched_videos:
                    print(f"    - {video}")

    except Exception as e:
        typer.secho(f"Ein Fehler ist aufgetreten: {e}", fg=typer.colors.RED)

def register(app: typer.Typer):
    app.command(name="scan-media")(scan_media_command)