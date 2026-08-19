using System.IO;
using System.Globalization;
using StreamGuard;

if (args.Length == 0 || args.Length > 3)
{
    Console.Error.WriteLine("Usage: dotnet run -- <path-to-log> [--output <report-path>]");
    return 1;
}

string path = args[0];
string outputPath;

if (args.Length == 1)
{
    outputPath = "report.json";
}
else if (args.Length == 3 && args[1] == "--output" && !string.IsNullOrWhiteSpace(args[2]))
{
    outputPath = args[2];
}
else
{
    Console.Error.WriteLine("Usage: dotnet run -- <path-to-log> [--output <report-path>]");
    return 1;
}

if (!File.Exists(path))
{
    Console.Error.WriteLine($"Error: file not found: {path}");
    return 1;
}

ScanResult result = await LogScanner.ScanAsync(path);
ThreatAssessment threatAssessment = ThreatAssessor.Assess(result);
ReportGenerator.WriteReport(outputPath, result, threatAssessment);
string ratioText = threatAssessment.FailureToSuccessRatio?.ToString("F2", CultureInfo.InvariantCulture) ?? "undefined";

Console.WriteLine($"File scanned: {result.Path}");
Console.WriteLine($"Lines processed: {result.LineCount}");
Console.WriteLine($"Matched lines: {result.MatchedLines}");
Console.WriteLine($"Event counts: {string.Join(", ", result.EventCounts.Select(pair => $"{pair.Key}={pair.Value}"))}");
Console.WriteLine($"Top usernames: {string.Join(", ", result.TopUsernames.Select(item => $"{item.Value} ({item.Count})"))}");
Console.WriteLine($"Top source IPs: {string.Join(", ", result.TopSourceIps.Select(item => $"{item.Value} ({item.Count})"))}");
Console.WriteLine($"Threat level: {threatAssessment.Level.ToString().ToUpperInvariant()} (ratio: {ratioText})");
Console.WriteLine($"Threat explanation: {threatAssessment.Explanation}");
Console.WriteLine($"Report written: {outputPath}");
Console.WriteLine($"Execution time: {result.Duration}");

return 0;
