using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Notepadmin.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        SourceLink.PointerPressed += OnSourceLinkClick;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnSourceLinkClick(object? sender, PointerPressedEventArgs e)
    {
        var url = "https://github.com/notepadmin";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", url);
    }
}
