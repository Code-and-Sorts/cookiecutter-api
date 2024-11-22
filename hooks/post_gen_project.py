# !/usr/bin/env python
from pathlib import Path

if __name__ == '__main__':

    if "{{cookiecutter.language}}" != "Python":
        Path('{{cookiecutter.project_name}}-python').unlink()

    if "{{cookiecutter.language}}" != "Typescript":
        Path('{{cookiecutter.project_name}}-typescript').unlink()
