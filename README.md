# StreamGuard

> A bounded-memory, single-threaded security log analyzer built with C# and .NET 10.

## Overview

StreamGuard processes security log files sequentially, one line at a time, without ever loading the entire file into memory.

The project's primary engineering challenge is processing multi-gigabyte input without allowing memory usage to grow with the total file size.

## Current Status

**Phase 1 — Core Streaming Engine**

The core streaming architecture is implemented and verified:

- Accepts a log-file path as a command-line argument.
- Validates that the path was provided and that the file exists.
- Reads the file sequentially with `StreamReader.ReadLineAsync()`.
- Counts the total lines processed.
- Measures execution time with `Stopwatch`.
- Prints a concise summary (file, line count, execution time).

## Usage

    dotnet run --project src/StreamGuard -- <path-to-log>

Example:

    dotnet run --project src/StreamGuard -- samples/sample.log

Output:

    File scanned: samples/sample.log
    Lines processed: 6
    Execution time: 00:00:00.0154540

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

## Architecture

Phase 1 implements only the streaming reader stage. Later phases add event parsing, telemetry, threat assessment, and reporting.

```text
Log File
   │
   ▼
Streaming Reader        ← implemented in Phase 1
   │
   ▼
Security Event Parser   (planned)
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