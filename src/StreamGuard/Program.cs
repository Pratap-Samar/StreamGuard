using System.IO;
using StreamGuard;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: dotnet run -- <path-to-log>");
    return 1;
}

string path = args[0];

if (!File.Exists(path))
{
    Console.Error.WriteLine($"Error: file not found: {path}");
    return 1;
}

ScanResult result = await LogScanner.ScanAsync(path);

Console.WriteLine($"File scanned: {result.Path}");
Console.WriteLine($"Lines processed: {result.LineCount}");
Console.WriteLine($"Matched lines: {result.MatchedLines}");
Console.WriteLine($"Event counts: {string.Join(", ", result.EventCounts.Select(pair => $"{pair.Key}={pair.Value}"))}");
Console.WriteLine($"Top usernames: {string.Join(", ", result.TopUsernames.Select(item => $"{item.Value} ({item.Count})"))}");
Console.WriteLine($"Top source IPs: {string.Join(", ", result.TopSourceIps.Select(item => $"{item.Value} ({item.Count})"))}");
Console.WriteLine($"Execution time: {result.Duration}");

return 0;
