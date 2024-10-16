# src/video_compressor/app.py

import typer
from video_compressor.commands.convert_with_handbrake import convert_videos_with_handbrake_command
# Weitere Imports...

app = typer.Typer()

# Registriere die Commands
app.command("convert-to-hevc")(convert_videos_with_handbrake_command)
# Weitere Commands...

if __name__ == "__main__":
    app()