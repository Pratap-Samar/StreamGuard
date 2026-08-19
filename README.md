# StreamGuard

> A bounded-memory, single-threaded security log analyzer built with C# and .NET 10.

## Overview

StreamGuard processes security log files sequentially, one line at a time, rather than loading the entire file into memory.

The project's primary engineering challenge is processing multi-gigabyte input without allowing memory usage to grow with the total file size.

## Why StreamGuard?

Whole-file approaches can require memory proportional to input size. StreamGuard processes input incrementally and retains only bounded state required for analysis. The project experimentally evaluates whether this design keeps observed process memory approximately stable while processing increasingly large security logs.

## Key Features

- Sequential streaming with `StreamReader.ReadLineAsync()`.
- Security-event detection for supported syslog `auth.log` formats.
- Failed authentication detection.
- Successful authentication detection.
- Both supported invalid-user probe formats.
- Sudo/privilege-escalation event detection.
- Timestamp, username, and source-IP extraction.
- Bounded event, username, and IP telemetry.
- Explainable LOW/MEDIUM/HIGH threat assessment.
- JSON report generation using `System.Text.Json`.
- Experimental performance and memory validation.

## Architecture

```text
Log File
   │
   ▼
Streaming Reader
   │
   ▼
Security Event Parser
   │
   ▼
Bounded Telemetry
   │
   ▼
Threat Assessment
   │
   ▼
Scan Report
```

## Technology Stack

- C#
- .NET 10 console application
- `StreamReader`
- `ReadLineAsync()`
- `System.Text.RegularExpressions`
- `System.Text.Json`
- xUnit

## Getting Started

### Prerequisites

- .NET 10 SDK

### Build

```text
dotnet build -c Release
```

### Test

```text
dotnet test StreamGuard.slnx
```

The current test suite contains 41 tests, all passing.

## Usage

```text
dotnet run --project src/StreamGuard -- <path-to-log>
```

Example:

```text
dotnet run --project src/StreamGuard -- samples/sample.log
```

Use `--output <path>` to choose a different report path:

```text
dotnet run --project src/StreamGuard -- samples/sample.log --output output/report.json
```

## Output

```text
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
```

## Performance & Memory Validation

### Methodology

The benchmark used Release builds, one warm-up run per dataset, and three measured runs per dataset. Throughput was calculated as file size in MiB divided by application-reported execution time. Process working set was measured externally at 50 ms intervals and compared with `PeakWorkingSet64`.

### Environment

| Property | Value |
|---|---|
| .NET | 10.0.302 |
| OS | Windows NT 10.0.26200.0 |
| CPU | 12th Gen Intel Core i5-12450H |
| RAM | Approximately 15.73 GB |
| Storage | Fixed NTFS D: volume |
| Configuration | Release |

### Benchmark Scope

The mixed workload covered 10 MiB, 100 MiB, 500 MiB, 1 GiB, and approximately 1.65 GiB. The noise-heavy workload covered 100 MiB, 500 MiB, and 1 GiB with approximately 95% unrelated lines and 5% representative supported security events.

### Results

| Dataset | Size | Mean Time | Mean Throughput | Peak Working Set |
|---|---:|---:|---:|---:|
| `mixed_10mb.log` | 10 MiB | 1.0586 s | 9.45 MiB/s | 46.34 MiB |
| `mixed_100mb.log` | 100 MiB | 3.5052 s | 28.53 MiB/s | 48.73 MiB |
| `mixed_500mb.log` | 500 MiB | 12.8075 s | 39.04 MiB/s | 46.12 MiB |
| `mixed_1gb.log` | 1 GiB | 25.5368 s | 40.11 MiB/s | 48.89 MiB |
| `massive_auth.log` | 1.65 GiB | 42.1203 s | 40.18 MiB/s | 44.79 MiB |

### Findings

Larger mixed inputs reached approximately 39–40 MiB/s. Small inputs had lower throughput, consistent with greater relative startup overhead. Peak working set remained approximately 44–50 MiB for the larger tested inputs, and input size increased substantially without a proportional increase in observed process working set.

Noise-heavy inputs showed higher observed throughput, but the workloads also differ in the proportion of matching security events. This demonstrates workload behavior rather than isolating the effect of noise percentage alone.

This is not a mathematical O(1) memory proof. Working set is not equivalent to managed heap size, and runtime buffering, OS/filesystem behavior, sampling limits, background activity, and individual line size can affect the measurement.

For the detailed methodology, individual runs, and analysis, see [benchmark_summary.md](benchmark_summary.md).

Raw measurements are available in [benchmark_results.csv](benchmark_results.csv).

## Reproducing the Benchmark

The deterministic benchmark datasets can be generated with:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\Generate-BenchmarkLogs.ps1
```

The generator creates datasets under `samples/benchmarks/`. These large generated inputs are excluded from version control; the generator itself is committed for reproducibility.

## Limitations

- Supported parsing is limited to documented syslog `auth.log` formats.
- Threat assessment is a heuristic, not a definitive security classification.
- Bounded top-user/IP results are approximate after eviction.
- Benchmark results depend on hardware, OS, storage, runtime, and workload.
- Working-set measurements are not equivalent to managed heap measurements.
- The benchmark does not prove mathematical O(1) memory.
- Individual line size can affect memory usage.

## Project Structure

```text
StreamGuard/
├── src/StreamGuard/              Console application
├── tests/StreamGuard.Tests/      Automated tests
├── tools/                        Reproducible benchmark generator
├── samples/                      Example and generated benchmark inputs
├── benchmark_summary.md          Detailed benchmark analysis
├── benchmark_results.csv         Raw benchmark measurements
└── README.md
```
