import os
import subprocess
import typer
from rich.console import Console
from rich.table import Table

app = typer.Typer()
console = Console()

def analyze(input_file: str = typer.Argument(..., help="Pfad zur Eingabedatei")):
    """
    Analysiert eine Videodatei mit MediaInfo und zeigt alle relevanten Informationen an.
    """
    if not os.path.isfile(input_file):
        console.print(f"[bold red]Die Datei '{input_file}' existiert nicht.[/bold red]")
        raise typer.Exit(code=1)

    # Überprüfen, ob MediaInfo installiert ist
    if subprocess.call(["which", "mediainfo"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL) != 0:
        console.print("[bold red]MediaInfo ist nicht installiert. Bitte installieren und erneut versuchen.[/bold red]")
        raise typer.Exit(code=1)

    # MediaInfo-Befehl für alle Details
    mediainfo_cmd = ["mediainfo", input_file]
    mediainfo_output = subprocess.run(mediainfo_cmd, stdout=subprocess.PIPE, text=True).stdout

    # Ausgabe in Abschnitte teilen
    sections = mediainfo_output.split("\n\n")
    general_info, video_info, audio_info, subtitle_info = "", "", [], []

    # Daten analysieren und formatieren
    for section in sections:
        if section.startswith("General"):
            general_info = section
        elif section.startswith("Video"):
            video_info = section
        elif section.startswith("Audio"):
            audio_info.append(section)
        elif section.startswith("Text"):
            subtitle_info.append(section)

    # Allgemeine Informationen
    console.print("[bold blue]Allgemeine Informationen:[/bold blue]")
    console.print(general_info)

    # Video-Informationen als Tabelle
    if video_info:
        video_table = Table(title="Video Stream", style="bright_green")
        video_table.add_column("Format", style="cyan")
        video_table.add_column("Profil", style="green")
        video_table.add_column("HDR", style="magenta")
        video_table.add_column("Bitrate", style="yellow")
        video_table.add_column("Auflösung", style="blue")
        video_table.add_column("Farbtiefe", style="red")
        video_table.add_column("Farbraum", style="cyan")

        lines = video_info.split("\n")
        details = {line.split(":")[0].strip(): line.split(":")[1].strip() for line in lines if ":" in line}
        resolution = f"{details.get('Width', 'N/A')} x {details.get('Height', 'N/A')}"
        video_table.add_row(
            details.get("Format", "N/A"),
            details.get("Format profile", "N/A"),
            details.get("HDR format", "N/A"),
            details.get("Bit rate", "N/A"),
            resolution,
            details.get("Bit depth", "N/A"),
            details.get("Color primaries", "N/A"),
        )
        console.print(video_table)

    # Audio-Informationen als Tabelle
    if audio_info:
        audio_table = Table(title="Audio Streams", style="bright_yellow")
        audio_table.add_column("ID", style="cyan", justify="center")
        audio_table.add_column("Codec", style="green")
        audio_table.add_column("Profil", style="magenta")
        audio_table.add_column("Kanäle", style="yellow")
        audio_table.add_column("Layout", style="blue")
        audio_table.add_column("Bitrate", style="red")
        audio_table.add_column("Sampling-Rate", style="green")
        audio_table.add_column("Sprache", style="cyan")

        for audio in audio_info:
            lines = audio.split("\n")
            details = {line.split(":")[0].strip(): line.split(":")[1].strip() for line in lines if ":" in line}
            audio_table.add_row(
                details.get("ID", "N/A"),
                details.get("Format", "N/A"),
                details.get("Format_Profile", "N/A"),
                details.get("Channel(s)", "N/A"),
                details.get("Channel layout", "N/A"),
                details.get("Bit rate", "N/A"),
                details.get("Sampling rate", "N/A"),
                details.get("Language", "N/A")
            )
        console.print(audio_table)

    # Untertitel-Informationen als Tabelle
    if subtitle_info:
        subtitle_table = Table(title="Untertitel Streams", style="bright_cyan")
        subtitle_table.add_column("ID", style="cyan", justify="center")
        subtitle_table.add_column("Codec", style="green")
        subtitle_table.add_column("Sprache", style="yellow")
        subtitle_table.add_column("Forced", style="red")

        for subtitle in subtitle_info:
            lines = subtitle.split("\n")
            details = {line.split(":")[0].strip(): line.split(":")[1].strip() for line in lines if ":" in line}
            subtitle_table.add_row(
                details.get("ID", "N/A"),
                details.get("Format", "N/A"),
                details.get("Language", "N/A"),
                details.get("Forced", "N/A")
            )
        console.print(subtitle_table)
    else:
        console.print("[bold red]Keine Untertitel gefunden.[/bold red]")

@app.command()
def main():
    typer.run(analyze)


if __name__ == "__main__":
    main()