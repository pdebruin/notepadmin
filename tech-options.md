# Notepadmin — Technology Options

Evaluation of technology stacks against the project requirements: cross-platform (Windows + Linux Ubuntu), garbage-collected runtime preferred, well-known language, lightweight, fast launch, native look and feel.

---

## Python + PySide6 (Qt) ⭐ Recommended

| | |
|---|---|
| **Language** | Python |
| **GUI toolkit** | Qt 6 (via PySide6) |
| **License** | LGPL (PySide6) |

**Pros**
- Qt provides native look and feel on both Windows and Linux
- `QPlainTextEdit` covers most requirements out of the box (undo/redo, find, zoom, scrollbars, line endings)
- Python is widely known, easy to iterate on, huge ecosystem
- PySide6 is LGPL — no commercial licensing concerns
- Packaging via PyInstaller or Briefcase for standalone executables

**Cons**
- Slower startup than compiled languages
- Larger distribution size due to bundled Qt libraries (~80–150 MB packaged)
- Python can feel sluggish for very large files

---

## Python + GTK (PyGObject)

| | |
|---|---|
| **Language** | Python |
| **GUI toolkit** | GTK 4 (via PyGObject) |
| **License** | LGPL |

**Pros**
- Perfect native look on Linux/GNOME (gedit is GTK)
- Mature text-editing widgets
- Familiar to Linux desktop developers

**Cons**
- GTK on Windows works but feels less native — weaker fit for "Windows primary"
- Harder to package for Windows; GTK runtime dependencies are heavier
- Smaller community for GTK on Windows compared to Qt

> **Real-world example — gedit on Windows:** gedit is ~1.5 MB on Linux because GTK is already part of the OS. On Windows, the Microsoft Store version is **235 MB** (bundles the entire GTK stack) and costs **€8.49** — the fee exists solely to fund the Windows port maintenance. This illustrates the cost of shipping a GTK app on Windows.

---

## C# + Avalonia

| | |
|---|---|
| **Language** | C# (.NET) |
| **GUI toolkit** | Avalonia UI |
| **License** | MIT |

**Pros**
- Excellent performance and polished native feel on both platforms
- Strong typing, great IDE support (Visual Studio, Rider)
- Active community, modern XAML-based UI framework
- Single binary deployment via .NET AOT compilation

**Cons**
- Steeper setup compared to Python
- Avalonia is less mainstream than WPF/WinForms — smaller ecosystem
- .NET runtime must be bundled or installed

---

## Go GUI Options

Go has several GUI frameworks, but the ecosystem is less mature than Python/C# for desktop apps.

### Go + Fyne

| | |
|---|---|
| **GUI toolkit** | Fyne |
| **License** | BSD |
| **Rendering** | Custom (OpenGL) — not native OS widgets |

**Pros**
- Fast startup, compiles to a single static binary with zero dependencies
- Small memory footprint
- Simple language, easy to learn
- Cross-compilation built into the toolchain

**Cons**
- Custom-drawn widgets — looks the same everywhere but native on neither platform
- Text editing widgets are basic; would need significant custom work for undo/redo, find, zoom
- Limited rich text-editing support

### Go + gotk4 (GTK4 bindings)

| | |
|---|---|
| **GUI toolkit** | GTK 4 (via gotk4) |
| **License** | MIT |
| **Rendering** | Native GTK widgets |

**Pros**
- Native look on Linux (uses real GTK widgets)
- Advanced text editing via `GtkTextView`

**Cons**
- GTK on Windows is painful to package and doesn't feel native
- Requires CGO and GTK development libraries

### Go + go-qt (Qt bindings)

| | |
|---|---|
| **GUI toolkit** | Qt (via go-qt / miqt) |
| **License** | Varies (LGPL for Qt) |
| **Rendering** | Native Qt widgets |

**Pros**
- Native look on both platforms, rich text editing widgets
- Mature Qt ecosystem behind it

**Cons**
- Requires CGO — loses Go's simple cross-compilation advantage
- Go bindings lag behind C++/Python Qt — less maintained
- Heavy deployment (must bundle Qt libraries)

### Go + Gio

| | |
|---|---|
| **GUI toolkit** | Gio (immediate mode) |
| **License** | MIT |
| **Rendering** | Custom GPU-accelerated |

**Pros**
- Pure Go, hardware-accelerated, very performant

**Cons**
- Immediate-mode paradigm — steep learning curve, no standard widgets
- Would need to build text editor UI from scratch
- Overkill for a simple text editor

### Go + Wails (Go backend + web frontend)

| | |
|---|---|
| **GUI toolkit** | OS webview + HTML/CSS/JS frontend |
| **License** | MIT |
| **Rendering** | Web-based (embedded browser) |

**Pros**
- Single binary output, leverage web frontend ecosystem
- Good native integration for menus and dialogs

**Cons**
- Essentially Electron-lite — defeats the "lightweight" goal
- Requires WebView2 on Windows
- UI looks like a web app, not a native desktop app

---

## Rust + egui / iced / gtk-rs

| | |
|---|---|
| **Language** | Rust |
| **GUI toolkit** | egui, iced, or gtk-rs |
| **License** | MIT / Apache 2.0 |

**Pros**
- Excellent performance, minimal memory footprint
- Small standalone binaries
- `gtk-rs` provides true native GTK look on Linux; `iced` and `egui` are modern and actively developed
- Strong safety guarantees from the language

**Cons**
- **Not garbage-collected** — falls outside the stated requirement for automatic memory management
- Steeper learning curve (ownership, borrowing, lifetimes)
- GUI ecosystem is still maturing; less polished than Qt or GTK from Python/C#
- Longer development time for the same feature set compared to Python

---

## Summary

| Stack | Native feel | Startup speed | Package size | Dev speed | GC runtime |
|---|---|---|---|---|---|
| Python + PySide6 (Qt) | ✅ Good | ⚠️ Moderate | ⚠️ Large | ✅ Fast | ✅ Yes |
| Python + GTK | ✅ Linux / ⚠️ Windows | ⚠️ Moderate | ⚠️ Large | ✅ Fast | ✅ Yes |
| C# + Avalonia | ✅ Good | ✅ Fast | ⚠️ Moderate | ⚠️ Moderate | ✅ Yes |
| Go + Fyne | ❌ Custom rendering | ✅ Fast | ✅ Small | ⚠️ Moderate | ✅ Yes |
| Go + gotk4 | ✅ Linux / ❌ Windows | ✅ Fast | ⚠️ Moderate | ⚠️ Moderate | ✅ Yes |
| Go + go-qt | ✅ Good | ✅ Fast | ⚠️ Large | ❌ Slow | ✅ Yes |
| Go + Gio | ❌ Custom rendering | ✅ Fast | ✅ Small | ❌ Slow | ✅ Yes |
| Go + Wails | ⚠️ Web-based | ✅ Fast | ⚠️ Moderate | ✅ Fast | ✅ Yes |
| Rust + egui/iced/gtk-rs | ⚠️ Varies | ✅ Fast | ✅ Small | ❌ Slow | ❌ No |

## Packaging & Distribution

No technology produces a single binary that runs on both Windows and Linux — different OSes require different executables. The question is which options give you a **single self-contained binary per platform** with zero runtime dependencies on the target machine.

| Stack | Single binary? | Dependencies on target | Notes |
|---|---|---|---|
| Go + Fyne | ✅ One static binary | None | Cleanest packaging story |
| Go + Gio | ✅ One static binary | None | |
| Rust + iced/egui | ✅ One static binary | None | |
| C# + Avalonia (AOT) | ✅ One binary | None (with Native AOT) | Best balance of native feel + single binary |
| Go + Wails | ⚠️ One binary | WebView2 on Windows | |
| Python + PySide6 | ⚠️ Bundled via PyInstaller | None, but ~100–150 MB | Works but large |
| Go + gotk4 | ❌ Needs GTK | GTK runtime | |
| Go + go-qt | ❌ Needs Qt | Qt libraries | |
| Python + GTK | ❌ Hard to bundle | GTK runtime on Windows | |
