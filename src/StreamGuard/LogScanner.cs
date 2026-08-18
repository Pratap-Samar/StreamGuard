using System.Diagnostics;
using System.IO;

namespace StreamGuard;

public sealed record ScanResult(string Path, long LineCount, TimeSpan Duration);

public static class LogScanner
{
    public static async Task<ScanResult> ScanAsync(string path)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lineCount = 0;

        using StreamReader reader = new(path);

        while (await reader.ReadLineAsync() is not null)
        {
            lineCount++;
        }

        return new ScanResult(path, lineCount, stopwatch.Elapsed);
    }
}