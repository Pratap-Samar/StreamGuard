using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamGuard;

public sealed record ReportDocument(
    string File,
    double DurationMilliseconds,
    ReportSummary Summary,
    ThreatAssessment ThreatAssessment);

public sealed record ReportSummary(
    long TotalLines,
    long MatchedLines,
    IReadOnlyDictionary<SecurityEventType, long> EventCounts,
    IReadOnlyList<FrequencyCount> TopUsernames,
    IReadOnlyList<FrequencyCount> TopSourceIps);

public static class ReportGenerator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void WriteReport(string outputPath, ScanResult scanResult, ThreatAssessment threatAssessment)
    {
        ReportDocument report = new(
            scanResult.Path,
            scanResult.Duration.TotalMilliseconds,
            new ReportSummary(
                scanResult.LineCount,
                scanResult.MatchedLines,
                scanResult.EventCounts,
                scanResult.TopUsernames,
                scanResult.TopSourceIps),
            threatAssessment);

        string json = JsonSerializer.Serialize(report, SerializerOptions);
        File.WriteAllText(outputPath, json);
    }
}
