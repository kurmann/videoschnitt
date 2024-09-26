# src/apple_compressor_manager/commands/__init__.py

from .add_tag_command import add_tag_command
from .compress_file_command import compress_file_command
from .list_profiles_command import list_profiles_command

__all__ = [
    "add_tag_command",
    "compress_file_command",
    "list_profiles_command",
]