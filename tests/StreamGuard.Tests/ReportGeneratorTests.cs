using System.Text.Json;
using StreamGuard;

namespace StreamGuard.Tests;

public class ReportGeneratorTests
{
    [Fact]
    public void WriteReport_ProducesCamelCaseStructuredJson()
    {
        string path = Path.Combine(Path.GetTempPath(), $"streamguard-report-{Guid.NewGuid():N}.json");
        try
        {
            ScanResult scanResult = new(
                "sample.log",
                10,
                6,
                TimeSpan.FromMilliseconds(12.5),
                new Dictionary<SecurityEventType, long>
                {
                    [SecurityEventType.FailedAuthentication] = 3,
                    [SecurityEventType.SuccessfulAuthentication] = 1,
                    [SecurityEventType.InvalidUserProbe] = 2,
                    [SecurityEventType.SudoEscalation] = 0
                },
                new[] { new FrequencyCount("alice", 3) },
                new[] { new FrequencyCount("192.168.1.10", 4) });
            ThreatAssessment threatAssessment = ThreatAssessor.Assess(scanResult);

            ReportGenerator.WriteReport(path, scanResult, threatAssessment);

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement root = document.RootElement;
            JsonElement summary = root.GetProperty("summary");

            Assert.Equal("sample.log", root.GetProperty("file").GetString());
            Assert.Equal(10, summary.GetProperty("totalLines").GetInt64());
            Assert.Equal(6, summary.GetProperty("matchedLines").GetInt64());
            Assert.Equal("Medium", root.GetProperty("threatAssessment").GetProperty("level").GetString());
            Assert.False(root.TryGetProperty("TotalLines", out _));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
