# Notepadmin — Test Checklist

Manual test checklist to validate requirements. Run through on both Windows and Linux (Ubuntu).

---

## Window & Layout

- [ ] App launches and shows a single window with a menu bar and text area
- [ ] No toolbar, status bar, or side panels are visible
- [ ] Window is freely resizable by dragging edges and corners
- [ ] Text area grows and shrinks with the window, always filling space below the menu bar
- [ ] Font is Consolas (monospace)

## Title Bar

- [ ] New app shows `Untitled — Notepadmin`
- [ ] After opening a file, shows `myfile.txt — Notepadmin`
- [ ] After editing, shows `*myfile.txt — Notepadmin` (asterisk for unsaved)
- [ ] After saving, asterisk disappears

## File Menu

- [ ] **New** (`Ctrl+N`) — clears editor, resets to Untitled
- [ ] **Open** (`Ctrl+O`) — opens file dialog, loads plain-text file
- [ ] **Save** (`Ctrl+S`) — saves current file; if Untitled, prompts for filename
- [ ] **Save As** (`Ctrl+Shift+S`) — prompts for new filename and saves
- [ ] **Close** (`Ctrl+W`) — closes current file, resets to Untitled
- [ ] **Exit** (`Alt+F4`) — quits the application

## Edit Menu

- [ ] **Undo** (`Ctrl+Z`) — undoes last edit
- [ ] **Redo** (`Ctrl+Y`) — redoes last undone edit
- [ ] **Cut** (`Ctrl+X`) — cuts selected text to clipboard
- [ ] **Copy** (`Ctrl+C`) — copies selected text to clipboard
- [ ] **Paste** (`Ctrl+V`) — pastes as plain text (strip formatting)
- [ ] **Delete** (`Del`) — deletes selected text
- [ ] **Select All** (`Ctrl+A`) — selects all text
- [ ] **Find** (`Ctrl+F`) — opens find bar, searches for text

## View Menu

- [ ] **Word Wrap** — toggles word wrap on and off
- [ ] **Zoom In** (`Ctrl++`) — increases text size
- [ ] **Zoom Out** (`Ctrl+-`) — decreases text size

## Clipboard — Plain Text Only

- [ ] Copy rich text from another app (e.g., browser, Word), paste into Notepadmin — only plain text appears
- [ ] Copy from Notepadmin, paste into another app — plain text is pasted

## Text Editing

- [ ] Caret, selection, and scrolling work as expected
- [ ] Vertical scrollbar appears when text exceeds visible height
- [ ] Horizontal scrollbar appears when a line exceeds visible width (word wrap off)
- [ ] Unicode characters display correctly (e.g., accented letters, emoji)

## Command-Line Usage

- [ ] `notepadmin` — launches with an empty Untitled document
- [ ] `notepadmin existing.txt` — opens and displays the file contents
- [ ] `notepadmin newfile.txt` (non-existent) — opens empty editor, Save uses `newfile.txt` as default name

## File Handling

- [ ] Saves as UTF-8 by default
- [ ] Opens and correctly displays a UTF-8 file
- [ ] On Windows: new files use CRLF line endings
- [ ] On Linux: new files use LF line endings
- [ ] Opening a file preserves its existing line-ending style

## Unsaved Changes

- [ ] Typing marks the document as modified (asterisk in title bar)
- [ ] **New** with unsaved changes — shows Save / Don't Save / Cancel dialog
- [ ] **Open** with unsaved changes — shows Save / Don't Save / Cancel dialog
- [ ] **Close** with unsaved changes — shows Save / Don't Save / Cancel dialog
- [ ] **Exit** with unsaved changes — shows Save / Don't Save / Cancel dialog
- [ ] Choosing **Save** saves and proceeds
- [ ] Choosing **Don't Save** discards changes and proceeds
- [ ] Choosing **Cancel** returns to the editor without doing anything

## Accessibility

- [ ] `Ctrl+Scroll` (mouse wheel) zooms text in and out
- [ ] Screen reader (NVDA on Windows, Orca on Linux) can read menu items and editor content
- [ ] Voice input (OS-level dictation) is received as text in the editor

## Help Menu

- [ ] **About** — opens a dialog showing app name, version, license, and source link
- [ ] Source link opens the default browser
- [ ] Dialog closes with OK button

## Cross-Platform

- [ ] All tests above pass on **Windows**
- [ ] All tests above pass on **Linux (Ubuntu)**
- [ ] App feels native on each platform
- [ ] App launches quickly and uses low memory
