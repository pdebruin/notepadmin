using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Notepadmin.Services;

namespace Notepadmin.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DocumentService _documentService = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private string? _currentFilePath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WindowTitle))]
    private bool _isModified;

    [ObservableProperty]
    private bool _wordWrapEnabled;

    [ObservableProperty]
    private double _fontSize = 14;

    private LineEnding _currentLineEnding = LineEnding.PlatformDefault;

    private const double MinFontSize = 8;
    private const double MaxFontSize = 72;

    // Set by MainWindow after editor is initialized
    public Action? OnNewRequested { get; set; }
    public Action<string>? OnLoadText { get; set; }
    public Func<string>? OnGetText { get; set; }
    public Action? OnFindRequested { get; set; }
    public Func<Task<bool?>>? OnConfirmSaveDialog { get; set; }
    public Func<Task<string?>>? OnShowOpenDialog { get; set; }
    public Func<Task<string?>>? OnShowSaveDialog { get; set; }
    public Action? OnExitRequested { get; set; }
    public Action? OnShowAbout { get; set; }

    public string WindowTitle
    {
        get
        {
            var name = CurrentFilePath != null ? Path.GetFileName(CurrentFilePath) : "Untitled";
            var modified = IsModified ? "*" : "";
            return $"{modified}{name} — Notepadmin";
        }
    }

    public void LoadFromFile(string path)
    {
        var fullPath = Path.GetFullPath(path);
        if (File.Exists(fullPath))
        {
            var doc = _documentService.ReadFile(fullPath);
            _currentLineEnding = doc.DetectedLineEnding;
            CurrentFilePath = fullPath;
            OnLoadText?.Invoke(doc.Text);
            IsModified = false;
        }
        else
        {
            // File doesn't exist yet — use as default save path
            CurrentFilePath = fullPath;
            OnNewRequested?.Invoke();
            IsModified = false;
        }
    }

    private async Task<bool> CheckUnsavedChanges()
    {
        if (!IsModified) return true;
        if (OnConfirmSaveDialog == null) return true;

        var result = await OnConfirmSaveDialog();
        // true = Save, false = Don't Save, null = Cancel
        if (result == true)
        {
            await DoSave();
            return true;
        }
        return result == false; // Don't Save = proceed, Cancel = abort
    }

    [RelayCommand]
    private async Task NewFile()
    {
        if (!await CheckUnsavedChanges()) return;
        CurrentFilePath = null;
        _currentLineEnding = LineEnding.PlatformDefault;
        OnNewRequested?.Invoke();
        IsModified = false;
    }

    [RelayCommand]
    private async Task OpenFile()
    {
        if (!await CheckUnsavedChanges()) return;
        var path = OnShowOpenDialog != null ? await OnShowOpenDialog() : null;
        if (path == null) return;

        var doc = _documentService.ReadFile(path);
        _currentLineEnding = doc.DetectedLineEnding;
        CurrentFilePath = path;
        OnLoadText?.Invoke(doc.Text);
        IsModified = false;
    }

    [RelayCommand]
    private async Task SaveFile()
    {
        await DoSave();
    }

    private async Task DoSave()
    {
        if (CurrentFilePath == null)
        {
            await SaveFileAs();
            return;
        }

        var text = OnGetText?.Invoke() ?? "";
        _documentService.WriteFile(CurrentFilePath, text, _currentLineEnding);
        IsModified = false;
    }

    [RelayCommand]
    private async Task SaveFileAs()
    {
        var path = OnShowSaveDialog != null ? await OnShowSaveDialog() : null;
        if (path == null) return;

        CurrentFilePath = path;
        var text = OnGetText?.Invoke() ?? "";
        _documentService.WriteFile(CurrentFilePath, text, _currentLineEnding);
        IsModified = false;
    }

    [RelayCommand]
    private async Task CloseFile()
    {
        if (!await CheckUnsavedChanges()) return;
        CurrentFilePath = null;
        _currentLineEnding = LineEnding.PlatformDefault;
        OnNewRequested?.Invoke();
        IsModified = false;
    }

    [RelayCommand]
    private async Task ExitApp()
    {
        if (!await CheckUnsavedChanges()) return;
        OnExitRequested?.Invoke();
    }

    [RelayCommand]
    private void Find()
    {
        OnFindRequested?.Invoke();
    }

    [RelayCommand]
    private void ZoomIn()
    {
        if (FontSize < MaxFontSize)
            FontSize = Math.Min(FontSize + 2, MaxFontSize);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (FontSize > MinFontSize)
            FontSize = Math.Max(FontSize - 2, MinFontSize);
    }

    [RelayCommand]
    private void ToggleWordWrap()
    {
        WordWrapEnabled = !WordWrapEnabled;
    }

    [RelayCommand]
    private void ShowAbout()
    {
        OnShowAbout?.Invoke();
    }

    public void AdjustFontSize(double delta)
    {
        if (delta > 0)
            ZoomIn();
        else
            ZoomOut();
    }

    public async Task<bool> CheckUnsavedChangesPublic()
    {
        return await CheckUnsavedChanges();
    }
}
