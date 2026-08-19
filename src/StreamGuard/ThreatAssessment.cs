using System.Globalization;

namespace StreamGuard;

public enum ThreatLevel
{
    Low,
    Medium,
    High
}

public sealed record ThreatAssessment(
    ThreatLevel Level,
    double? FailureToSuccessRatio,
    long FailureSignals,
    long SuccessfulAuthentications,
    long SudoEvents,
    string Explanation);

public static class ThreatAssessor
{
    private const double MediumThreshold = 3;
    private const double HighThreshold = 10;

    public static ThreatAssessment Assess(ScanResult scanResult)
    {
        long failedAuthentications = GetCount(scanResult, SecurityEventType.FailedAuthentication);
        long invalidUserProbes = GetCount(scanResult, SecurityEventType.InvalidUserProbe);
        long successfulAuthentications = GetCount(scanResult, SecurityEventType.SuccessfulAuthentication);
        long sudoEvents = GetCount(scanResult, SecurityEventType.SudoEscalation);
        long failureSignals = failedAuthentications + invalidUserProbes;

        if (successfulAuthentications == 0)
        {
            ThreatLevel level = failureSignals == 0 ? ThreatLevel.Low : ThreatLevel.High;
            string zeroSuccessExplanation = failureSignals == 0
                ? $"No successful authentications or failure signals; ratio is undefined and treated as 0. Sudo events: {sudoEvents}. Threat level: {FormatLevel(level)}."
                : $"No successful authentications; ratio is undefined and one or more failure signals produce HIGH. Failure signals: {failureSignals}; sudo events: {sudoEvents}.";

            return new ThreatAssessment(
                level,
                null,
                failureSignals,
                successfulAuthentications,
                sudoEvents,
                zeroSuccessExplanation);
        }

        double ratio = (double)failureSignals / successfulAuthentications;
        ThreatLevel ratioLevel = ratio < MediumThreshold
            ? ThreatLevel.Low
            : ratio < HighThreshold
                ? ThreatLevel.Medium
                : ThreatLevel.High;
        string explanation = $"Failure-to-success ratio: {ratio.ToString("F2", CultureInfo.InvariantCulture)} ({failureSignals} failure signals / {successfulAuthentications} successful authentications); Sudo events: {sudoEvents}; threat level: {FormatLevel(ratioLevel)}.";

        return new ThreatAssessment(
            ratioLevel,
            ratio,
            failureSignals,
            successfulAuthentications,
            sudoEvents,
            explanation);
    }

    private static long GetCount(ScanResult scanResult, SecurityEventType eventType)
    {
        return scanResult.EventCounts.TryGetValue(eventType, out long count) ? count : 0;
    }

    private static string FormatLevel(ThreatLevel level)
    {
        return level.ToString().ToUpperInvariant();
    }
}
