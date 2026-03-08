using Avalonia;
using Avalonia.X11;
using System;

namespace Notepadmin;

sealed class Program
{
    public static string? InitialFilePath { get; private set; }

    [STAThread]
    public static void Main(string[] args)
    {
        InitialFilePath = args.Length > 0 ? args[0] : null;
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .With(new X11PlatformOptions
            {
                RenderingMode = [X11RenderingMode.Software]
            })
            .WithInterFont()
            .LogToTrace();
}
