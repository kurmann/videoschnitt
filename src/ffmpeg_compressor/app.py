# src/ffmpeg_compressor/app.py

import typer
from ffmpeg_compressor.commands.convert_to_hevc import convert_videos_to_hevc_command
# Weitere Imports...

app = typer.Typer()

# Registriere die Commands
app.command("convert-to-hevc")(convert_videos_to_hevc_command)
# Weitere Commands...

if __name__ == "__main__":
    app()