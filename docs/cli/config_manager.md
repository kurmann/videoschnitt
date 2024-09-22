# `config-manager`

Config Manager CLI für Kurmann Videoschnitt

**Usage**:

```console
$ config-manager [OPTIONS] COMMAND [ARGS]...
```

**Options**:

* `--install-completion`: Install completion for the current shell.
* `--show-completion`: Show completion for the current shell, to copy it or customize the installation.
* `--help`: Show this message and exit.

**Commands**:

* `delete`: Löscht einen Konfigurationswert aus der...
* `list`: Listet alle Konfigurationswerte in der...
* `set`: Setzt einen Konfigurationswert in der...

## `config-manager delete`

Löscht einen Konfigurationswert aus der .env-Datei.

**Usage**:

```console
$ config-manager delete [OPTIONS] KEY
```

**Arguments**:

* `KEY`: Der Schlüssel, den du löschen möchtest.  [required]

**Options**:

* `--help`: Show this message and exit.

## `config-manager list`

Listet alle Konfigurationswerte in der .env-Datei auf.

**Usage**:

```console
$ config-manager list [OPTIONS]
```

**Options**:

* `--help`: Show this message and exit.

## `config-manager set`

Setzt einen Konfigurationswert in der .env-Datei.

**Usage**:

```console
$ config-manager set [OPTIONS] KEY VALUE
```

**Arguments**:

* `KEY`: Der Schlüssel, den du setzen möchtest.  [required]
* `VALUE`: Der Wert, der dem Schlüssel zugewiesen werden soll.  [required]

**Options**:

* `--help`: Show this message and exit.
