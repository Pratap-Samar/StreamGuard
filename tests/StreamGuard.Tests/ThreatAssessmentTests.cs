using StreamGuard;

namespace StreamGuard.Tests;

public class ThreatAssessmentTests
{
    [Theory]
    [InlineData(2, 0, 1, ThreatLevel.Low, 2)]
    [InlineData(3, 0, 1, ThreatLevel.Medium, 3)]
    [InlineData(10, 0, 1, ThreatLevel.High, 10)]
    [InlineData(1, 2, 1, ThreatLevel.Medium, 3)]
    public void Assess_UsesFailureToSuccessThresholds(
        long failedAuthentications,
        long invalidUserProbes,
        long successfulAuthentications,
        ThreatLevel expectedLevel,
        double expectedRatio)
    {
        ThreatAssessment result = ThreatAssessor.Assess(CreateScanResult(
            failedAuthentications,
            invalidUserProbes,
            successfulAuthentications,
            0));

        Assert.Equal(expectedLevel, result.Level);
        Assert.Equal(expectedRatio, result.FailureToSuccessRatio);
    }

    [Fact]
    public void Assess_ZeroSuccessWithFailures_IsHighAndRatioUndefined()
    {
        ThreatAssessment result = ThreatAssessor.Assess(CreateScanResult(2, 1, 0, 0));

        Assert.Equal(ThreatLevel.High, result.Level);
        Assert.Null(result.FailureToSuccessRatio);
        Assert.Contains("No successful authentications", result.Explanation);
        Assert.Contains("undefined", result.Explanation);
    }

    [Fact]
    public void Assess_ZeroSuccessAndZeroFailures_IsLowAndRatioUndefined()
    {
        ThreatAssessment result = ThreatAssessor.Assess(CreateScanResult(0, 0, 0, 2));

        Assert.Equal(ThreatLevel.Low, result.Level);
        Assert.Null(result.FailureToSuccessRatio);
        Assert.Contains("Sudo events: 2", result.Explanation);
    }

    [Fact]
    public void Assess_SudoIsReportedWithoutChangingRatioLevel()
    {
        ThreatAssessment withoutSudo = ThreatAssessor.Assess(CreateScanResult(2, 0, 1, 0));
        ThreatAssessment withSudo = ThreatAssessor.Assess(CreateScanResult(2, 0, 1, 3));

        Assert.Equal(withoutSudo.Level, withSudo.Level);
        Assert.Equal(withoutSudo.FailureToSuccessRatio, withSudo.FailureToSuccessRatio);
        Assert.Contains("Sudo events: 3", withSudo.Explanation);
    }

    private static ScanResult CreateScanResult(
        long failedAuthentications,
        long invalidUserProbes,
        long successfulAuthentications,
        long sudoEvents)
    {
        return new ScanResult(
            "test.log",
            0,
            0,
            TimeSpan.Zero,
            new Dictionary<SecurityEventType, long>
            {
                [SecurityEventType.FailedAuthentication] = failedAuthentications,
                [SecurityEventType.SuccessfulAuthentication] = successfulAuthentications,
                [SecurityEventType.InvalidUserProbe] = invalidUserProbes,
                [SecurityEventType.SudoEscalation] = sudoEvents
            },
            Array.Empty<FrequencyCount>(),
            Array.Empty<FrequencyCount>());
    }
}
