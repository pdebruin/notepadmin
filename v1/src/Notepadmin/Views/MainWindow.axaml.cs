using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using AvaloniaEdit.Search;
using Notepadmin.ViewModels;

namespace Notepadmin.Views;

public partial class MainWindow : Window
{
    private TextEditor _editor = null!;
    private SearchPanel? _searchPanel;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _editor = this.FindControl<TextEditor>("Editor")!;
        _searchPanel = SearchPanel.Install(_editor);

        WireViewModel();
        WireEditMenuItems();
        WireEditorEvents();

        // Load file from CLI argument now that everything is wired
        if (DataContext is MainWindowViewModel vm && Program.InitialFilePath != null)
            vm.LoadFromFile(Program.InitialFilePath);
    }

    private void WireViewModel()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        vm.OnNewRequested = () =>
        {
            _editor.Document.Text = "";
        };

        vm.OnLoadText = (text) =>
        {
            _editor.Document.Text = text;
        };

        vm.OnGetText = () => _editor.Document.Text;

        vm.OnFindRequested = () =>
        {
            _searchPanel?.Open();
        };

        vm.OnConfirmSaveDialog = async () =>
        {
            var name = vm.CurrentFilePath != null
                ? System.IO.Path.GetFileName(vm.CurrentFilePath)
                : "Untitled";

            var dialog = new Window
            {
                Title = "Notepadmin",
                Width = 400,
                Height = 150,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false,
                Content = CreateSaveDialogContent(name)
            };

            var tcs = new TaskCompletionSource<bool?>();
            dialog.Tag = tcs;
            await dialog.ShowDialog(this);
            return tcs.Task.IsCompleted ? tcs.Task.Result : null;
        };

        vm.OnShowOpenDialog = async () =>
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt", "*.*" } }
                }
            });

            return files.Count > 0 ? files[0].Path.LocalPath : null;
        };

        vm.OnShowSaveDialog = async () =>
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save File",
                DefaultExtension = "txt",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt" } },
                    new FilePickerFileType("All Files") { Patterns = new[] { "*.*" } }
                },
                SuggestedFileName = vm.CurrentFilePath != null
                    ? System.IO.Path.GetFileName(vm.CurrentFilePath)
                    : "Untitled.txt"
            });

            return file?.Path.LocalPath;
        };

        vm.OnExitRequested = () =>
        {
            Close();
        };

        vm.OnShowAbout = () =>
        {
            var about = new AboutWindow();
            about.ShowDialog(this);
        };
    }

    private StackPanel CreateSaveDialogContent(string fileName)
    {
        var panel = new StackPanel { Margin = new Thickness(20), Spacing = 15 };
        panel.Children.Add(new TextBlock
        {
            Text = $"Do you want to save changes to {fileName}?",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        });

        var buttons = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var saveBtn = new Button { Content = "Save", Width = 80 };
        var dontSaveBtn = new Button { Content = "Don't Save", Width = 100 };
        var cancelBtn = new Button { Content = "Cancel", Width = 80 };

        saveBtn.Click += (_, _) => CloseSaveDialog(saveBtn, true);
        dontSaveBtn.Click += (_, _) => CloseSaveDialog(dontSaveBtn, false);
        cancelBtn.Click += (_, _) => CloseSaveDialog(cancelBtn, null);

        buttons.Children.Add(saveBtn);
        buttons.Children.Add(dontSaveBtn);
        buttons.Children.Add(cancelBtn);
        panel.Children.Add(buttons);

        return panel;
    }

    private static void CloseSaveDialog(Button button, bool? result)
    {
        var window = button.FindLogicalAncestorOfType<Window>();
        if (window?.Tag is TaskCompletionSource<bool?> tcs)
            tcs.TrySetResult(result);
        window?.Close();
    }

    private void WireEditMenuItems()
    {
        var menuUndo = this.FindControl<MenuItem>("MenuUndo");
        var menuRedo = this.FindControl<MenuItem>("MenuRedo");
        var menuCut = this.FindControl<MenuItem>("MenuCut");
        var menuCopy = this.FindControl<MenuItem>("MenuCopy");
        var menuPaste = this.FindControl<MenuItem>("MenuPaste");
        var menuDelete = this.FindControl<MenuItem>("MenuDelete");
        var menuSelectAll = this.FindControl<MenuItem>("MenuSelectAll");

        if (menuUndo != null) menuUndo.Click += (_, _) => _editor.Undo();
        if (menuRedo != null) menuRedo.Click += (_, _) => _editor.Redo();
        if (menuCut != null) menuCut.Click += (_, _) => _editor.Cut();
        if (menuCopy != null) menuCopy.Click += (_, _) => _editor.Copy();
        if (menuPaste != null) menuPaste.Click += async (_, _) => await PastePlainText();
        if (menuDelete != null) menuDelete.Click += (_, _) => _editor.Delete();
        if (menuSelectAll != null) menuSelectAll.Click += (_, _) => _editor.SelectAll();
    }

    private void WireEditorEvents()
    {
        if (DataContext is not MainWindowViewModel vm) return;

        // Track modifications
        _editor.Document.TextChanged += (_, _) =>
        {
            vm.IsModified = true;
        };

        // Ctrl+Scroll for zoom — use tunnel to intercept before editor scrolls
        _editor.TextArea.AddHandler(PointerWheelChangedEvent, (_, e) =>
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                vm.AdjustFontSize(e.Delta.Y);
                e.Handled = true;
            }
        }, RoutingStrategies.Tunnel);

        // Intercept Ctrl+V for plain-text-only paste
        _editor.TextArea.AddHandler(KeyDownEvent, OnEditorKeyDown, RoutingStrategies.Tunnel);
    }

    private async void OnEditorKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            e.Handled = true;
            await PastePlainText();
        }
    }

    private async Task PastePlainText()
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard == null) return;

#pragma warning disable CS0618 // GetTextAsync is deprecated but TryGetTextAsync not yet available in all versions
        var text = await clipboard.GetTextAsync();
#pragma warning restore CS0618
        if (!string.IsNullOrEmpty(text))
        {
            _editor.TextArea.Selection.ReplaceSelectionWithText(text);
        }
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is MainWindowViewModel { IsModified: true } vm)
        {
            e.Cancel = true;
            if (await vm.CheckUnsavedChangesPublic())
            {
                vm.IsModified = false; // Prevent re-triggering
                Close();
            }
        }
        base.OnClosing(e);
    }
}