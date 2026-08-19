using StreamGuard;

namespace StreamGuard.Tests;

public class SecurityEventParserTests
{
    [Theory]
    [InlineData("Aug 15 10:03:45 host sshd[1236]: Failed password for alice from 192.168.1.10 port 51235 ssh2", SecurityEventType.FailedAuthentication)]
    [InlineData("Aug 15 10:02:10 host sshd[1235]: Accepted password for alice from 192.168.1.50 port 54321 ssh2", SecurityEventType.SuccessfulAuthentication)]
    [InlineData("Aug 15 10:01:22 host sshd[1234]: Failed password for invalid user root from 192.168.1.10 port 51234 ssh2", SecurityEventType.InvalidUserProbe)]
    [InlineData("Aug 15 10:01:22 host sshd[1234]: Invalid user guest from 192.168.1.10 port 51234", SecurityEventType.InvalidUserProbe)]
    [InlineData("Aug 15 10:04:01 host sudo: alice : TTY=pts/0 ; PWD=/home/alice ; USER=root ; COMMAND=/bin/bash", SecurityEventType.SudoEscalation)]
    public void TryParse_DetectsEventType(string line, SecurityEventType expected)
    {
        SecurityEvent? result = SecurityEventParser.TryParse(line);

        Assert.NotNull(result);
        Assert.Equal(expected, result!.EventType);
    }

    [Theory]
    [InlineData("Aug 15 10:03:45 host sshd[1236]: Failed password for alice from 192.168.1.10 port 51235 ssh2", "alice", "192.168.1.10")]
    [InlineData("Aug 15 10:02:10 host sshd[1235]: Accepted password for alice from 192.168.1.50 port 54321 ssh2", "alice", "192.168.1.50")]
    [InlineData("Aug 15 10:01:22 host sshd[1234]: Failed password for invalid user root from 192.168.1.10 port 51234 ssh2", "root", "192.168.1.10")]
    [InlineData("Aug 15 10:01:22 host sshd[1234]: Invalid user guest from 192.168.1.10 port 51234", "guest", "192.168.1.10")]
    [InlineData("Aug 15 10:05:30 host sshd[1237]: Failed password for bob from 192.168.1.10 port 51236 ssh2", "bob", "192.168.1.10")]
    public void TryParse_ExtractsUsernameAndSourceIp(string line, string expectedUser, string expectedIp)
    {
        SecurityEvent? result = SecurityEventParser.TryParse(line);

        Assert.NotNull(result);
        Assert.Equal(expectedUser, result!.Username);
        Assert.Equal(expectedIp, result.SourceIp);
    }

    [Theory]
    [InlineData("Aug 15 10:03:45 host sshd[1236]: Failed password for alice from 192.168.1.10 port 51235 ssh2", "Aug 15 10:03:45")]
    [InlineData("Aug 15 10:02:10 host sshd[1235]: Accepted password for alice from 192.168.1.50 port 54321 ssh2", "Aug 15 10:02:10")]
    [InlineData("Aug 15 10:01:22 host sshd[1234]: Failed password for invalid user root from 192.168.1.10 port 51234 ssh2", "Aug 15 10:01:22")]
    [InlineData("Aug  5 10:01:22 host sshd[1234]: Invalid user guest from 192.168.1.10 port 51234", "Aug  5 10:01:22")]
    [InlineData("Aug 15 10:04:01 host sudo: alice : TTY=pts/0 ; PWD=/home/alice ; USER=root ; COMMAND=/bin/bash", "Aug 15 10:04:01")]
    public void TryParse_ExtractsTimestamp(string line, string expectedTimestamp)
    {
        SecurityEvent? result = SecurityEventParser.TryParse(line);

        Assert.NotNull(result);
        Assert.Equal(expectedTimestamp, result!.Timestamp);
    }

    [Fact]
    public void TryParse_SudoEvent_HasNoSourceIp()
    {
        SecurityEvent? result = SecurityEventParser.TryParse("Aug 15 10:04:01 host sudo: alice : TTY=pts/0 ; PWD=/home/alice ; USER=root ; COMMAND=/bin/bash");

        Assert.NotNull(result);
        Assert.Equal("alice", result!.Username);
        Assert.Null(result.SourceIp);
    }

    [Theory]
    [InlineData("Aug 15 10:06:11 host sshd[1238]: Connection closed by authenticating user alice 192.168.1.50 port 54322")]
    [InlineData("Aug 15 10:04:01 host cron[99]: (alice) CMD (echo hello)")]
    [InlineData("this is not a syslog line")]
    [InlineData("")]
    [InlineData("Aug 15 10:01:22 host sshd[1234]: Invalid user from 192.168.1.10 port 51234")]
    [InlineData("Aug 15 10:01:22 host sshd[1234]: Failed password for invalid user root")]
    [InlineData("not a complete event Invalid user guest from 192.168.1.10")]
    public void TryParse_UnsupportedLine_ReturnsNull(string line)
    {
        Assert.Null(SecurityEventParser.TryParse(line));
    }
}
