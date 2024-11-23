# src/video_compressor/app.py

import typer
from video_compressor.commands.convert_with_handbrake import convert_videos_with_handbrake_command
from video_compressor.commands.convert_to_apple import convert_to_apple
from video_compressor.commands.analyze_with_mediainfo import analyze

app = typer.Typer()

app.command("convert-to-hevc")(convert_videos_with_handbrake_command)
app.command("convert-to-apple")(convert_to_apple)
app.command("analyze")(analyze)

if __name__ == "__main__":
    app()