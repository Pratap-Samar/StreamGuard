using System.Diagnostics;
using System.IO;

namespace StreamGuard;

public sealed record ScanResult(
    string Path,
    long LineCount,
    long MatchedLines,
    TimeSpan Duration,
    IReadOnlyDictionary<SecurityEventType, long> EventCounts,
    IReadOnlyList<FrequencyCount> TopUsernames,
    IReadOnlyList<FrequencyCount> TopSourceIps);

public static class LogScanner
{
    private const int TopOutputCount = 10;

    public static async Task<ScanResult> ScanAsync(string path)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        long lineCount = 0;
        long matchedLines = 0;
        BoundedFrequencyCounter usernameCounter = new();
        BoundedFrequencyCounter sourceIpCounter = new();
        Dictionary<SecurityEventType, long> eventCounts = new()
        {
            [SecurityEventType.FailedAuthentication] = 0,
            [SecurityEventType.SuccessfulAuthentication] = 0,
            [SecurityEventType.InvalidUserProbe] = 0,
            [SecurityEventType.SudoEscalation] = 0
        };

        using StreamReader reader = new(path);

        while (await reader.ReadLineAsync() is { } line)
        {
            lineCount++;
            SecurityEvent? securityEvent = SecurityEventParser.TryParse(line);
            if (securityEvent is null)
            {
                continue;
            }

            matchedLines++;
            eventCounts[securityEvent.EventType]++;

            if (securityEvent.Username is not null)
            {
                usernameCounter.Observe(securityEvent.Username);
            }

            if (securityEvent.SourceIp is not null)
            {
                sourceIpCounter.Observe(securityEvent.SourceIp);
            }
        }

        return new ScanResult(
            path,
            lineCount,
            matchedLines,
            stopwatch.Elapsed,
            eventCounts,
            usernameCounter.GetTop(TopOutputCount),
            sourceIpCounter.GetTop(TopOutputCount));
    }
}
