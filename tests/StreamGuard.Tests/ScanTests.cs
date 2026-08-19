using System.IO;
using StreamGuard;

namespace StreamGuard.Tests;

public class ScanTests
{
    [Fact]
    public async Task ScanAsync_CountsLines()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "line1\nline2\nline3\n");
            ScanResult result = await LogScanner.ScanAsync(path);
            Assert.Equal(3, result.LineCount);
            Assert.Equal(path, result.Path);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ScanAsync_EmptyFile_HasZeroLines()
    {
        string path = Path.GetTempFileName();
        try
        {
            ScanResult result = await LogScanner.ScanAsync(path);
            Assert.Equal(0, result.LineCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ScanAsync_NoTrailingNewline_StillCounts()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "line1\nline2");
            ScanResult result = await LogScanner.ScanAsync(path);
            Assert.Equal(2, result.LineCount);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ScanAsync_DurationIsNonNegative()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "a\nb\n");
            ScanResult result = await LogScanner.ScanAsync(path);
            Assert.True(result.Duration >= TimeSpan.Zero);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task ScanAsync_CountsMatchedLines()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                path,
                "Aug 15 10:01:22 host sshd[1234]: Failed password for invalid user root from 192.168.1.10 port 51234 ssh2\n"
                + "Aug 15 10:02:10 host sshd[1235]: Accepted password for alice from 192.168.1.50 port 54321 ssh2\n"
                + "Aug 15 10:06:11 host sshd[1238]: Connection closed by authenticating user alice 192.168.1.50 port 54322\n");

            ScanResult result = await LogScanner.ScanAsync(path);

            Assert.Equal(3, result.LineCount);
            Assert.Equal(2, result.MatchedLines);
        }
        finally
        {
            File.Delete(path);
        }
    }
}