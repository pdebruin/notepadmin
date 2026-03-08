# Notepadmin — Requirements

## Overview

A minimal, cross-platform plain-text editor — a marriage of classic Windows Notepad and gedit. It should feel native on Windows and also run on Linux (Ubuntu), with the simplicity and speed of old-school Notepad.

## Platforms

- **Windows** (primary) — should look and feel like Windows Notepad
- **Linux Ubuntu** (secondary)

## User Interface

### Window Layout

- A single window with:
  1. A **menu bar** at the top
  2. A single **text area** filling the rest of the window
- No toolbar, status bar, or side panels
- The window must be **freely resizable** by the user (drag edges/corners)
- The text area must **grow and shrink** with the window — it should always fill all available space below the menu bar

### Menu Bar

#### File

| Item   | Description                          | Shortcut   |
|--------|--------------------------------------|------------|
| New    | Clear the editor / start a new file  | `Ctrl+N` |
| Open   | Open a plain-text file from disk     | `Ctrl+O` |
| Save   | Save the current file to disk        | `Ctrl+S` |
| Save As| Save to a new filename               | `Ctrl+Shift+S` |
| Close  | Close the current file               | `Ctrl+W` |
| Exit   | Quit the application                 | `Alt+F4` |

#### Edit

| Item       | Description                            | Shortcut       |
|------------|----------------------------------------|----------------|
| Undo       | Undo the last edit                     | `Ctrl+Z`       |
| Redo       | Redo the last undone edit              | `Ctrl+Y`       |
| Cut        | Cut selected text to clipboard         | `Ctrl+X`       |
| Copy       | Copy selected text to clipboard        | `Ctrl+C`       |
| Paste      | Paste text from clipboard (plain text only — strip any formatting) | `Ctrl+V` |
| Delete     | Delete selected text                   | `Del`          |
| Select All | Select all text in the editor          | `Ctrl+A`       |
| Find       | Open a find bar to search for text     | `Ctrl+F`       |

#### View

| Item        | Description                                      | Shortcut         |
|-------------|--------------------------------------------------|------------------|
| Word Wrap   | Toggle word wrap on/off                          |                  |
| Zoom In     | Increase text size                               | `Ctrl++`         |
| Zoom Out    | Decrease text size                               | `Ctrl+-`         |

#### Help

| Item   | Description                                          | Shortcut |
|--------|------------------------------------------------------|----------|
| About  | Show app name, version, license, and source link     |          |

## Title Bar

- Display the current filename and app name (e.g., `myfile.txt — Notepadmin`)
- If no file is open, display `Untitled — Notepadmin`
- Indicate unsaved changes with a leading asterisk (e.g., `*myfile.txt — Notepadmin`)

## Command-Line Usage

- Accept an optional filename as a command-line argument (e.g., `notepadmin myfile.txt`)
- If a filename is provided:
  - If the file exists, open it in the editor
  - If the file does not exist, start with an empty editor and use the given filename as the default for Save
- If no filename is provided, start with a new empty document (current behavior)

## Text Editing

- Plain text only — no rich-text formatting (no bold, italic, underline, colors, etc.)
- **Clipboard operations are plain-text only** — when pasting, always strip any rich-text formatting and insert as plain text
- Support basic Unicode characters
- Standard text-editing behavior (caret, selection, scroll, etc.)
- Vertical and horizontal **scrollbars** should appear when content overflows the visible area
- **Font**: Consolas (fixed/monospace) as the primary font, with system Unicode font fallbacks for non-Latin scripts (CJK, Arabic, Hebrew, etc.) — not user-configurable

## File Handling

- **Encoding**: Default to UTF-8
- **Line endings**: Use platform-native line endings (CRLF on Windows, LF on Linux); preserve the existing line-ending style when opening a file

## Unsaved Changes

- Track whether the document has been modified since last save
- On **New**, **Open**, **Close**, or **Exit** with unsaved changes: show a confirmation dialog asking the user to Save, Don't Save, or Cancel

## Accessibility

- **Text zoom**: Users can enlarge/reduce text size using `Ctrl+Scroll` (mouse wheel)
- Additional accessibility features may be added later

## Technology Constraints

- **Cross-platform**: Must run on Windows and Linux (Ubuntu) from a single codebase
- **Garbage-collected runtime**: Use a language with automatic memory management (e.g., C#, Java, Kotlin, Python, Go, TypeScript/Electron)
- **Well-known language**: Prefer widely adopted languages with strong ecosystem and community for long-term maintainability
- **Technology choice is open**: Specific framework/language to be decided at implementation time

- Lightweight and fast to launch
- Small memory footprint
- Native look and feel on each supported platform
