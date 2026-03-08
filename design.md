# Notepadmin — Detailed Design

Technology: **C# / .NET 9 + Avalonia UI + AvaloniaEdit**

---

## 1. Project Structure

```
notepadmin/
├── Notepadmin.sln                    # Solution file
├── src/
│   └── Notepadmin/
│       ├── Notepadmin.csproj         # Project file (Avalonia, AvaloniaEdit, AOT config)
│       ├── Program.cs                # Entry point — parses CLI args, launches app
│       ├── App.axaml                 # Application definition (theme, styles)
│       ├── App.axaml.cs              # Application startup logic
│       ├── ViewModels/
│       │   └── MainWindowViewModel.cs  # All editor state and commands
│       ├── Views/
│       │   ├── MainWindow.axaml      # Window layout: menu bar + text editor
│       │   └── MainWindow.axaml.cs   # Code-behind: wiring, zoom, shortcuts
│       └── Services/
│           └── DocumentService.cs    # File I/O, encoding, line-ending detection
└── README.md
```

This is intentionally flat — the app is simple and doesn't warrant deep nesting.

---

## 2. Technology Stack

| Component | Choice | Purpose |
|---|---|---|
| Framework | .NET 9 | Runtime, cross-platform support |
| GUI | Avalonia UI 11 | Cross-platform native-feeling window, menus, dialogs |
| Text editor control | AvaloniaEdit | Text area with built-in undo/redo, find, scrollbars |
| Architecture | MVVM (lightweight) | Separation of UI and logic via Avalonia data binding |
| Packaging | `dotnet publish` with Native AOT | Single self-contained binary per platform |

---

## 3. UI Layout (MainWindow.axaml)

The window is a single `DockPanel` with two children:

```
┌──────────────────────────────────────────────┐
│  Menu Bar (DockPanel.Dock="Top")             │
│  ┌─────┬──────┬──────┐                       │
│  │File │ Edit │ View │                       │
│  └─────┴──────┴──────┘                       │
├──────────────────────────────────────────────┤
│                                              │
│  AvaloniaEdit TextEditor                     │
│  (fills remaining space)                     │
│                                              │
│                                              │
│                                              │
│                                              │
└──────────────────────────────────────────────┘
```

The `TextEditor` control automatically handles:
- Scrollbars (vertical + horizontal)
- Caret, selection, keyboard navigation
- Undo/redo history
- Clipboard operations (cut/copy/paste)

### AXAML Sketch

```xml
<Window Title="{Binding WindowTitle}"
        Width="800" Height="600"
        MinWidth="400" MinHeight="300">
  <DockPanel>
    <Menu DockPanel.Dock="Top">
      <!-- File, Edit, View menus bound to ViewModel commands -->
    </Menu>
    <AvaloniaEdit:TextEditor
        Name="Editor"
        FontFamily="Consolas, 'Segoe UI', 'Noto Sans', 'Noto Sans CJK SC', 'Noto Sans Devanagari', sans-serif"
        FontSize="{Binding FontSize}"
        WordWrap="{Binding WordWrapEnabled}"
        ShowLineNumbers="False"
        HorizontalScrollBarVisibility="Auto"
        VerticalScrollBarVisibility="Auto" />
  </DockPanel>
</Window>
```

---

## 4. Component Design

### 4.1 MainWindowViewModel

Owns all editor state and exposes commands for menu actions.

**State (observable properties):**

| Property | Type | Purpose |
|---|---|---|
| `WindowTitle` | `string` | Computed: `[*]filename — Notepadmin` |
| `CurrentFilePath` | `string?` | `null` = Untitled |
| `IsModified` | `bool` | Dirty flag, drives `*` in title |
| `WordWrapEnabled` | `bool` | Toggle for word wrap |
| `FontSize` | `double` | Current zoom level (default: 14) |

**Commands (bound to menu items and shortcuts):**

| Command | Triggers | Behavior |
|---|---|---|
| `NewCommand` | Menu + `Ctrl+N` | Check unsaved → clear editor, reset state |
| `OpenCommand` | Menu + `Ctrl+O` | Check unsaved → show file dialog → load file |
| `SaveCommand` | Menu + `Ctrl+S` | If has path: save. If Untitled: delegate to SaveAs |
| `SaveAsCommand` | Menu + `Ctrl+Shift+S` | Show save dialog → write file |
| `CloseCommand` | Menu + `Ctrl+W` | Check unsaved → clear editor, reset state |
| `ExitCommand` | Menu + `Alt+F4` | Check unsaved → close window |
| `FindCommand` | Menu + `Ctrl+F` | Open AvaloniaEdit's built-in SearchPanel |
| `ZoomInCommand` | Menu + `Ctrl++` | Increase `FontSize` (max 72) |
| `ZoomOutCommand` | Menu + `Ctrl+-` | Decrease `FontSize` (min 8) |
| `ToggleWordWrapCommand` | Menu | Toggle `WordWrapEnabled` |

Undo, Redo, Cut, Copy, Paste, Delete, Select All are **not** ViewModel commands — they are handled directly by the AvaloniaEdit `TextEditor` control via its built-in keyboard shortcuts. The menu items simply invoke the corresponding `TextArea` methods in code-behind (e.g., `Editor.Undo()`, `Editor.Copy()`).

### 4.2 DocumentService

Handles file I/O with encoding and line-ending awareness.

| Method | Signature | Behavior |
|---|---|---|
| `ReadFile` | `(string path) → (string text, LineEnding detected)` | Read as UTF-8, detect line endings (CRLF/LF) |
| `WriteFile` | `(string path, string text, LineEnding ending)` | Write as UTF-8 with specified line endings |
| `DetectLineEnding` | `(string text) → LineEnding` | Scan for `\r\n` vs `\n`, default to platform-native |

```csharp
enum LineEnding { PlatformDefault, CRLF, LF }
```

### 4.3 Unsaved Changes Flow

Every action that would discard the current document (New, Open, Close, Exit) goes through this flow:

```
User triggers action
  → Is document modified?
     → No  → proceed
     → Yes → show dialog: "Save changes to {filename}?"
               [Save]       → run Save/SaveAs, then proceed
               [Don't Save] → proceed without saving
               [Cancel]     → abort, return to editor
```

The dialog uses Avalonia's `MessageBox` or a custom `Window` with three buttons.

### 4.4 Help / About Dialog

A simple modal dialog triggered from Help → About:

```
┌─────────────────────────────────┐
│         About Notepadmin        │
│                                 │
│  Notepadmin v1.0                │
│  A minimal plain-text editor    │
│                                 │
│  License: MIT                   │
│  Source: github.com/...         │
│                                 │
│            [ OK ]               │
└─────────────────────────────────┘
```

Implemented as a small Avalonia `Window` with labels and a close button. The source link opens the default browser via `Process.Start`.

---

## 5. Accessibility

Standard Avalonia controls (menus, text editor, dialogs) are automatically exposed to platform accessibility APIs:
- **Windows**: UI Automation → works with NVDA, Narrator
- **Linux**: AT-SPI → works with Orca

No additional work is needed for basic screen reader support. Voice input is handled at the OS level and arrives as normal text input.

---

## 6. Keyboard Shortcuts

All shortcuts are registered as `KeyBinding` entries on the window:

| Shortcut | Action | Handled by |
|---|---|---|
| `Ctrl+N` | New | ViewModel |
| `Ctrl+O` | Open | ViewModel |
| `Ctrl+S` | Save | ViewModel |
| `Ctrl+Shift+S` | Save As | ViewModel |
| `Ctrl+W` | Close | ViewModel |
| `Ctrl+Z` | Undo | AvaloniaEdit (built-in) |
| `Ctrl+Y` | Redo | AvaloniaEdit (built-in) |
| `Ctrl+X` | Cut | AvaloniaEdit (built-in) |
| `Ctrl+C` | Copy | AvaloniaEdit (built-in) |
| `Ctrl+V` | Paste (plain text) | Code-behind (intercept + strip formatting) |
| `Del` | Delete | AvaloniaEdit (built-in) |
| `Ctrl+A` | Select All | AvaloniaEdit (built-in) |
| `Ctrl+F` | Find | AvaloniaEdit SearchPanel |
| `Ctrl++` | Zoom In | ViewModel |
| `Ctrl+-` | Zoom Out | ViewModel |
| `Ctrl+Scroll` | Zoom In/Out | Code-behind (PointerWheelChanged) |

### Plain-Text Paste

`Ctrl+V` is intercepted in code-behind to ensure only plain text is pasted:

```csharp
async void OnPaste(object sender, RoutedEventArgs e)
{
    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
    var text = await clipboard?.GetTextAsync();
    if (text != null)
        Editor.TextArea.Selection.ReplaceSelectionWithText(text);
    e.Handled = true; // prevent default rich-text paste
}
```

---

## 5. Title Bar Logic

The `WindowTitle` property is computed from `CurrentFilePath` and `IsModified`:

```
IsModified = true,  CurrentFilePath = "/home/user/notes.txt"  → "*notes.txt — Notepadmin"
IsModified = false, CurrentFilePath = "/home/user/notes.txt"  → "notes.txt — Notepadmin"
IsModified = true,  CurrentFilePath = null                    → "*Untitled — Notepadmin"
IsModified = false, CurrentFilePath = null                    → "Untitled — Notepadmin"
```

The `IsModified` flag is set by subscribing to `Editor.Document.Changed` and cleared on save.

---

## 7. Command-Line Argument Handling


In `Program.cs`:

```csharp
static void Main(string[] args)
{
    var filePath = args.Length > 0 ? args[0] : null;
    BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    // filePath is passed to MainWindowViewModel on startup
}
```

- If `filePath` is provided and file exists → load it via `DocumentService.ReadFile`
- If `filePath` is provided and file does not exist → set `CurrentFilePath` (for Save default), leave editor empty
- If no argument → start as Untitled

---

## 8. Build & Packaging

### Project File (Notepadmin.csproj) — Key Settings

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <PublishAot>true</PublishAot>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Avalonia" Version="11.*" />
    <PackageReference Include="Avalonia.Desktop" Version="11.*" />
    <PackageReference Include="Avalonia.Themes.Fluent" Version="11.*" />
    <PackageReference Include="Avalonia.AvaloniaEdit" Version="11.*" />
  </ItemGroup>
</Project>
```

### Publish Commands

```bash
# Windows (from Windows or cross-compile)
dotnet publish -c Release -r win-x64

# Linux (from Linux or cross-compile)
dotnet publish -c Release -r linux-x64
```

Each produces a single self-contained binary in `bin/Release/net9.0/{rid}/publish/`.

### Development

```bash
dotnet new sln -n Notepadmin
dotnet new avalonia.app -n Notepadmin -o src/Notepadmin
dotnet sln add src/Notepadmin
cd src/Notepadmin
dotnet add package Avalonia.AvaloniaEdit
dotnet run   # launches the app for testing
```

---

## 9. Requirement → Implementation Traceability

| Requirement | Implemented by |
|---|---|
| Menu bar with File/Edit/View | `MainWindow.axaml` — `<Menu>` with bindings |
| Text area filling window | `DockPanel` + `TextEditor` control |
| Resizable window | Avalonia default behavior |
| New / Open / Save / SaveAs / Close / Exit | `MainWindowViewModel` commands |
| Undo / Redo / Cut / Copy / Delete / Select All | AvaloniaEdit built-in |
| Paste (plain text only) | Code-behind paste interception |
| Find | AvaloniaEdit `SearchPanel.Install()` |
| Word Wrap toggle | `TextEditor.WordWrap` bound to ViewModel |
| Zoom In / Out | `FontSize` property + commands |
| Ctrl+Scroll zoom | `PointerWheelChanged` handler in code-behind |
| Title bar with filename + dirty indicator | `WindowTitle` computed property |
| Unsaved changes dialog | Confirmation dialog before destructive actions |
| Command-line filename | `Program.cs` args → ViewModel |
| UTF-8 encoding | `DocumentService` reads/writes UTF-8 |
| Line ending detection + preservation | `DocumentService.DetectLineEnding` |
| Consolas font | `FontFamily="Consolas"` in AXAML |
| Scrollbars | AvaloniaEdit built-in |
| Cross-platform single binary | `dotnet publish` with AOT per platform |
