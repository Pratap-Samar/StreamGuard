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
}