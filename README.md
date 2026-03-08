# Notepadmin

A minimal plain-text editor inspired by Windows Notepad. Runs on Windows and Linux (Ubuntu).

## Building & Running

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
cd v1
dotnet run --project src/Notepadmin
```

### Linux prerequisites

On minimal or headless Linux installs (e.g. WSL2), you may need X11 libraries:

```bash
sudo apt install libice6 libsm6
```

A full Ubuntu Desktop installation already includes these.

## Distribution

> **TODO:** Decide on distribution strategy (self-contained zip, .deb package, etc.) and document here.

The goal is to use **.NET Native AOT** (`dotnet publish -r <rid> -p:PublishAot=true`) for a single self-contained binary per platform with no .NET runtime dependency.

## Running on WSL2

> **TODO:** Document WSLg requirements and setup instructions for running Notepadmin as a Linux GUI app under WSL2.
