# StreamGuard

> A bounded-memory, single-threaded security log analyzer built with C# and .NET 10.

## Overview

StreamGuard processes security log files sequentially, one line at a time, without ever loading the entire file into memory.

The project's primary engineering challenge is processing multi-gigabyte input without allowing memory usage to grow with the total file size.

## Current Status

**Phase 2 — Security Event Parsing**

Implemented and verified:

- Accepts a log-file path as a command-line argument and validates it.
- Reads the file sequentially with `StreamReader.ReadLineAsync()`, counting lines.
- Detects four security event types on syslog `auth.log` lines:
  - Failed authentication
  - Successful authentication
  - Invalid-user probe
  - Sudo/privilege escalation
- Extracts timestamp, username, and source IP per event.
- Reports total lines, matched lines, and execution time.

## Usage

    dotnet run --project src/StreamGuard -- <path-to-log>

Example:

    dotnet run --project src/StreamGuard -- samples/sample.log

Output:

    File scanned: samples/sample.log
    Lines processed: 6
    Matched lines: 5
    Execution time: 00:00:00.1026962

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

Phase 2 implements streaming input and security-event parsing. Later phases add telemetry, threat assessment, and reporting.

```text
Log File
   │
   ▼
Streaming Reader        ← implemented
   │
   ▼
Security Event Parser   ← implemented in Phase 2
   │
   ▼
Bounded Telemetry       (planned)
   │
   ▼
Threat Assessment       (planned)
   │
   ▼
Scan Report             (planned)
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