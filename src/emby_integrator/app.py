# src/emby_integrator/app.py

import typer
from emby_integrator.commands.rename_artwork import rename_artwork
from emby_integrator.commands.convert_image import convert_image_command
from emby_integrator.commands.write_nfo_file import write_nfo_file_command
from emby_integrator.commands.scan_media import scan_media_command
from emby_integrator.commands.generate_nfo_xml import generate_nfo_xml_command
from emby_integrator.commands.reset_permissions import reset_permissions_command
from emby_integrator.commands.group_files import group_files
from emby_integrator.commands.homemovie_integrator import integrate_homemovies
from emby_integrator.commands.list_mediafiles import list_mediafiles

app = typer.Typer(help="Emby Integrator")

# Registriere alle Befehle
app.command("rename-artwork")(rename_artwork)
app.command("convert-image")(convert_image_command)
app.command("write-nfo-file")(write_nfo_file_command)
app.command("scan-media")(scan_media_command)
app.command("generate-nfo-xml")(generate_nfo_xml_command)
app.command("write-nfo-file")(write_nfo_file_command)
app.command("reset-permissions")(reset_permissions_command)
app.command()(group_files)
app.command()(integrate_homemovies)
app.command()(list_mediafiles)

if __name__ == '__main__':
    app()