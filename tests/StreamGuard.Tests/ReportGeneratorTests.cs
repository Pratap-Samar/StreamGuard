using System.Text.Json;
using StreamGuard;

namespace StreamGuard.Tests;

public class ReportGeneratorTests
{
    Step 1 — Imports and namespace (lines 1–4)
    [Fact]
    public void WriteReport_ProducesCamelCaseStructuredJson()
    {
        Step 2 — Generate a unique, safe temp file path (line 11)
        string path = Path.Combine(Path.GetTempPath(), $"streamguard-report-{Guid.NewGuid():N}.json");
        try
        {
            Step 4 — Arrange: build a fake ScanResult (lines 14–27)
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
Step 5 — Act, part 1: run the threat assessment (line 28)
            ReportGenerator.WriteReport(path, scanResult, threatAssessment);
            Step 6 — Act, part 2: write the actual file (line 30)

            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement root = document.RootElement;
            JsonElement summary = root.GetProperty("summary");
            Step 7 — Read the file back as raw JSON (lines 32–34)
                Step 8 — Assert the file's actual shape and values (lines 36–40)

            Assert.Equal("sample.log", root.GetProperty("file").GetString());
            Assert.Equal(10, summary.GetProperty("totalLines").GetInt64());
            Assert.Equal(6, summary.GetProperty("matchedLines").GetInt64());
            Assert.Equal("Medium", root.GetProperty("threatAssessment").GetProperty("level").GetString());
            Assert.False(root.TryGetProperty("TotalLines", out _));
        }
        Step 9 — Cleanup (finally block, line 44)
        finally
        {
            File.Delete(path);
        }
    }
}
