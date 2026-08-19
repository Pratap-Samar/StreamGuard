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

    [Fact]
    public async Task ScanAsync_TracksEventCountsAndBoundedTopValues()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(
                path,
                "Aug 15 10:01:22 host sshd[1234]: Failed password for alice from 192.168.1.10 port 51234 ssh2\n"
                + "Aug 15 10:01:23 host sshd[1235]: Failed password for alice from 192.168.1.10 port 51235 ssh2\n"
                + "Aug 15 10:02:10 host sshd[1236]: Accepted password for alice from 192.168.1.50 port 54321 ssh2\n"
                + "Aug 15 10:03:00 host sshd[1237]: Invalid user guest from 192.168.1.10 port 51236\n"
                + "Aug 15 10:04:01 host sudo: alice : TTY=pts/0 ; PWD=/home/alice ; USER=root ; COMMAND=/bin/bash\n"
                + "unrelated input\n");

            ScanResult result = await LogScanner.ScanAsync(path);

            Assert.Equal(6, result.LineCount);
            Assert.Equal(5, result.MatchedLines);
            Assert.Equal(2, result.EventCounts[SecurityEventType.FailedAuthentication]);
            Assert.Equal(1, result.EventCounts[SecurityEventType.SuccessfulAuthentication]);
            Assert.Equal(1, result.EventCounts[SecurityEventType.InvalidUserProbe]);
            Assert.Equal(1, result.EventCounts[SecurityEventType.SudoEscalation]);
            Assert.Equal("alice", result.TopUsernames[0].Value);
            Assert.Equal(4, result.TopUsernames[0].Count);
            Assert.Equal("192.168.1.10", result.TopSourceIps[0].Value);
            Assert.Equal(3, result.TopSourceIps[0].Count);
            Assert.DoesNotContain(result.TopSourceIps, item => item.Value == "alice");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
