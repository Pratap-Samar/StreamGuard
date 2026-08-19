using System.Text.RegularExpressions;

namespace StreamGuard;

public static class SecurityEventParser
{
    private static readonly Regex InvalidUserRegex = new(
        @"^(?<ts>[A-Z][a-z]{2}\s+\d{1,2}\s+\d{2}:\d{2}:\d{2})\s+\S+\s+sshd\[\d+\]:\s+Failed password for invalid user (?<user>\S+) from (?<ip>\S+)",
        RegexOptions.NonBacktracking);

    private static readonly Regex StandaloneInvalidUserRegex = new(
        @"^(?<ts>[A-Z][a-z]{2}\s+\d{1,2}\s+\d{2}:\d{2}:\d{2})\s+\S+\s+sshd\[\d+\]:\s+Invalid user (?<user>\S+) from (?<ip>\S+)",
        RegexOptions.NonBacktracking);

    private static readonly Regex FailedAuthRegex = new(
        @"^(?<ts>[A-Z][a-z]{2}\s+\d{1,2}\s+\d{2}:\d{2}:\d{2})\s+\S+\s+sshd\[\d+\]:\s+Failed password for (?<user>\S+) from (?<ip>\S+)",
        RegexOptions.NonBacktracking);

    private static readonly Regex AcceptedAuthRegex = new(
        @"^(?<ts>[A-Z][a-z]{2}\s+\d{1,2}\s+\d{2}:\d{2}:\d{2})\s+\S+\s+sshd\[\d+\]:\s+Accepted password for (?<user>\S+) from (?<ip>\S+)",
        RegexOptions.NonBacktracking);

    private static readonly Regex SudoRegex = new(
        @"^(?<ts>[A-Z][a-z]{2}\s+\d{1,2}\s+\d{2}:\d{2}:\d{2})\s+\S+\s+sudo:\s+(?<user>\S+)\s*:",
        RegexOptions.NonBacktracking);

    public static SecurityEvent? TryParse(string line)
    {
        Match invalidUser = InvalidUserRegex.Match(line);
        if (invalidUser.Success)
        {
            return CreateEvent(invalidUser, SecurityEventType.InvalidUserProbe);
        }

        Match standaloneInvalidUser = StandaloneInvalidUserRegex.Match(line);
        if (standaloneInvalidUser.Success)
        {
            return CreateEvent(standaloneInvalidUser, SecurityEventType.InvalidUserProbe);
        }

        Match failedAuth = FailedAuthRegex.Match(line);
        if (failedAuth.Success)
        {
            return CreateEvent(failedAuth, SecurityEventType.FailedAuthentication);
        }

        Match acceptedAuth = AcceptedAuthRegex.Match(line);
        if (acceptedAuth.Success)
        {
            return CreateEvent(acceptedAuth, SecurityEventType.SuccessfulAuthentication);
        }

        Match sudo = SudoRegex.Match(line);
        if (sudo.Success)
        {
            return CreateEvent(sudo, SecurityEventType.SudoEscalation);
        }

        return null;
    }

    private static SecurityEvent CreateEvent(Match match, SecurityEventType eventType)
    {
        string? sourceIp = match.Groups["ip"].Success ? match.Groups["ip"].Value : null;

        return new SecurityEvent(
            match.Groups["ts"].Value,
            eventType,
            match.Groups["user"].Value,
            sourceIp);
    }
}
