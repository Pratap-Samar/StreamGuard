using System.Diagnostics;
using System.IO;

namespace StreamGuard;

public sealed record ScanResult(string Path, long LineCount, long MatchedLines, TimeSpan Duration);

public static class LogScanner
{
    public static async Task<ScanResult> ScanAsync(string path)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lineCount = 0;
        long matchedLines = 0;

        using StreamReader reader = new(path);

        while (await reader.ReadLineAsync() is { } line)
        {
            lineCount++;
            if (SecurityEventParser.TryParse(line) is not null)
            {
                matchedLines++;
            }
        }

        return new ScanResult(path, lineCount, matchedLines, stopwatch.Elapsed);
    }
}