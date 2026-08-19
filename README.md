# StreamGuard

> A bounded-memory, single-threaded security log analyzer built with C# and .NET 10.

## Overview

StreamGuard processes security log files sequentially, one line at a time, without ever loading the entire file into memory.

The project's primary engineering challenge is processing multi-gigabyte input without allowing memory usage to grow with the total file size.

## Current Status

**Phase 4 — Threat Assessment and Reporting**

Implemented and verified:

- Accepts a log-file path as a command-line argument and validates it.
- Reads the file sequentially with `StreamReader.ReadLineAsync()`, counting lines.
- Detects four security event types on syslog `auth.log` lines:
  - Failed authentication
  - Successful authentication
  - Invalid-user probe
  - Sudo/privilege escalation
- Extracts timestamp, username, and source IP per event.
- Tracks event counts.
- Tracks bounded top usernames and source IPs with capacity 100.
- Calculates an explainable LOW/MEDIUM/HIGH threat assessment.
- Writes a structured camelCase JSON report to `report.json` by default.
- Reports total lines, matched lines, event counts, top usernames, top source IPs, threat assessment, and execution time.

## Usage

    dotnet run --project src/StreamGuard -- <path-to-log>

Example:

    dotnet run --project src/StreamGuard -- samples/sample.log

Use `--output <path>` to choose a different report path:

    dotnet run --project src/StreamGuard -- samples/sample.log --output output/report.json

Output:

    File scanned: samples/sample.log
    Lines processed: 7
    Matched lines: 6
    Event counts: FailedAuthentication=2, SuccessfulAuthentication=1, InvalidUserProbe=2, SudoEscalation=1
    Top usernames: alice (3), bob (1), guest (1), root (1)
    Top source IPs: 192.168.1.10 (3), 192.168.1.20 (1), 192.168.1.50 (1)
    Threat level: MEDIUM (ratio: 4.00)
    Threat explanation: Failure-to-success ratio: 4.00 (4 failure signals / 1 successful authentications); Sudo events: 1; threat level: MEDIUM.
    Report written: report.json
    Execution time: 00:00:00.0720570

If no path is given, a usage message is printed and the process exits with a non-zero status. If the file does not exist, an error is printed and the process exits with a non-zero status.

## Core Goals

- Process large logs using streaming I/O.
- Detect security-relevant events.
- Extract structured security telemetry.
- Keep stateful analysis bounded.
- Produce an explainable threat assessment.
- Generate machine-readable JSON output.
- Validate performance and memory behavior experimentally.

## Technology

- C#
- .NET 10
- `StreamReader`
- `ReadLineAsync()`
- `System.Text.RegularExpressions`

## Architecture

Phase 4 implements streaming input, security-event parsing, bounded telemetry, threat assessment, and JSON reporting. The dashboard remains deferred.

```text
Log File
   │
   ▼
Streaming Reader        ← implemented
   │
   ▼
Security Event Parser   ← implemented
   │
   ▼
Bounded Telemetry        ← implemented in Phase 3
   │
   ▼
Threat Assessment       ← implemented in Phase 4
   │
   ▼
Scan Report             ← implemented in Phase 4
```

## Project Structure

```text
StreamGuard/
├── src/StreamGuard/            Console application
├── tests/StreamGuard.Tests/    Minimal automated tests
├── samples/sample.log          Example log for manual testing
└── README.md
```

## Testing

    dotnet test StreamGuard.slnx
