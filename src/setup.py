import os
from setuptools import setup, find_packages

with open(os.path.join(os.path.dirname(__file__), '..', 'README.md'), 'r') as fh:
    long_description = fh.read()

setup(
    name="kurmann_videoschnitt",
    version="0.42.0",
    description="Video editing automation tools for Kurmann Videoschnitt",
    long_description=long_description,
    long_description_content_type="text/markdown",
    author="Patrick Kurmann",
    author_email="patrick.kurmann@example.com",
    url="https://github.com/kurmann/videoschnitt",
    packages=find_packages(),
    entry_points={
        'console_scripts': [
            'import-new-media=new_media_importer.import_new_media:main',
            'integrate-new-media=original_media_integrator.integrate_new_media:main',
            'compress-prores=apple_compressor_manager.compress_prores:main',
        ],
    },
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
    ],
    python_requires='>=3.6',
)