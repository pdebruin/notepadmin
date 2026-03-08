using System;
using System.IO;
using System.Text;

namespace Notepadmin.Services;

public enum LineEnding
{
    PlatformDefault,
    CRLF,
    LF
}

public record DocumentContent(string Text, LineEnding DetectedLineEnding);

public class DocumentService
{
    public DocumentContent ReadFile(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var text = System.Text.Encoding.UTF8.GetString(bytes);
        var lineEnding = DetectLineEnding(text);
        // Normalize to \n internally for the editor
        text = text.Replace("\r\n", "\n");
        return new DocumentContent(text, lineEnding);
    }

    public void WriteFile(string path, string text, LineEnding lineEnding)
    {
        var ending = lineEnding switch
        {
            LineEnding.CRLF => "\r\n",
            LineEnding.LF => "\n",
            LineEnding.PlatformDefault => Environment.NewLine,
            _ => Environment.NewLine
        };

        // Normalize to \n first, then apply desired line ending
        var normalized = text.Replace("\r\n", "\n").Replace("\n", ending);
        File.WriteAllText(path, normalized, System.Text.Encoding.UTF8);
    }

    public static LineEnding DetectLineEnding(string text)
    {
        if (text.Contains("\r\n"))
            return LineEnding.CRLF;
        if (text.Contains('\n'))
            return LineEnding.LF;
        return LineEnding.PlatformDefault;
    }
}
