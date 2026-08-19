namespace StreamGuard;

public enum SecurityEventType
{
    FailedAuthentication,
    SuccessfulAuthentication,
    InvalidUserProbe,
    SudoEscalation
}

public sealed record SecurityEvent(
    string Timestamp,
    SecurityEventType EventType,
    string? Username,
    string? SourceIp);