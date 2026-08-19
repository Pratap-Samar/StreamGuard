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
Console.WriteLine($"Execution time: {result.Duration}");

return 0;