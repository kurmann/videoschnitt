# src/emby_integrator/commands/__init__.py

import typer

def register_commands(app: typer.Typer):
    from .list_metadata import register as register_list_metadata
    from .generate_nfo_xml import register as register_generate_nfo_xml
    from .write_nfo_file import register as register_write_nfo_file
    from .scan_media import register as register_scan_media
    from .convert_image import register as register_convert_image  # Geändert von convert_single_image
    
    register_list_metadata(app)
    register_generate_nfo_xml(app)
    register_write_nfo_file(app)
    register_scan_media(app)
    register_convert_image(app)  # Geändert von register_convert_single_image