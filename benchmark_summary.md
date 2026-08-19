# StreamGuard Phase 5 — Performance & Memory Validation

## 1. Objective

This benchmark evaluates StreamGuard processing performance as input size increases and observes process working-set behavior across larger inputs. The central question is whether observed memory remains approximately stable relative to total input size; this is an experiment, not a formal proof of O(1) memory.

## 2. Methodology

- Release configuration was used throughout.
- One warm-up run was performed for each dataset.
- Three measured runs were performed for each dataset.
- The deterministic benchmark datasets were kept unchanged during measurement.
- Execution time came from the application’s reported scan duration.
- Throughput was calculated as `file size in MiB / execution time in seconds`.
- Process working set was measured externally at 50 ms intervals and compared with `PeakWorkingSet64` after exit.
- No source changes, optimization, or benchmark-specific instrumentation was used.

## 3. Test Environment

| Property | Value |
|---|---|
| Date | 2026-08-20 |
| .NET | 10.0.302 |
| OS | Windows NT 10.0.26200.0 |
| CPU | 12th Gen Intel Core i5-12450H |
| RAM | Approximately 15.73 GB |
| Storage | Fixed NTFS D: volume |
| Configuration | Release |

## 4. Workloads

### Mixed workload

The mixed workload contains a representative mixture of supported security events and unrelated lines. Dataset sizes were 10 MiB, 100 MiB, 500 MiB, 1 GiB, and approximately 1.65 GiB.

### Noise-heavy workload

The noise-heavy workload contains approximately 95% unrelated lines and 5% representative supported security events. Dataset sizes were 100 MiB, 500 MiB, and 1 GiB. The generator is documented in `tools/Generate-BenchmarkLogs.ps1`.

## 5. Results

### Primary scaling results

| Dataset | Size | Mean Time | Mean Throughput | Peak Working Set |
|---|---:|---:|---:|---:|
| `mixed_10mb.log` | 10 MiB | 1.0586 s | 9.45 MiB/s | 46.34 MiB |
| `mixed_100mb.log` | 100 MiB | 3.5052 s | 28.53 MiB/s | 48.73 MiB |
| `mixed_500mb.log` | 500 MiB | 12.8075 s | 39.04 MiB/s | 46.12 MiB |
| `mixed_1gb.log` | 1 GiB | 25.5368 s | 40.11 MiB/s | 48.89 MiB |
| `massive_auth.log` | 1.65 GiB | 42.1203 s | 40.18 MiB/s | 44.79 MiB |

### Noise-heavy comparison

| Dataset | Size | Mean Time | Mean Throughput | Peak Working Set |
|---|---:|---:|---:|---:|
| `noise_100mb.log` | 100 MiB | 2.2468 s | 44.52 MiB/s | 49.90 MiB |
| `noise_500mb.log` | 500 MiB | 6.5090 s | 76.84 MiB/s | 50.29 MiB |
| `noise_1gb.log` | 1 GiB | 12.0229 s | 85.20 MiB/s | 47.13 MiB |

## 6. Individual Runs

| Dataset | Run 1 | Run 2 | Run 3 |
|---|---:|---:|---:|
| `mixed_10mb.log` | 1.0238879 s | 1.0880711 s | 1.0638287 s |
| `mixed_100mb.log` | 3.5659265 s | 3.4662108 s | 3.4833371 s |
| `mixed_500mb.log` | 12.9683215 s | 12.6573715 s | 12.7968937 s |
| `mixed_1gb.log` | 26.1216382 s | 25.4862848 s | 25.0024703 s |
| `massive_auth.log` | 41.9063124 s | 42.5282270 s | 41.9263628 s |
| `noise_100mb.log` | 2.2074832 s | 2.2805878 s | 2.2524096 s |
| `noise_500mb.log` | 6.4957006 s | 6.6370638 s | 6.3940939 s |
| `noise_1gb.log` | 11.8546380 s | 11.8892494 s | 12.3247116 s |

## 7. Findings

### Memory

Peak process working-set usage remained approximately 44–50 MiB for the larger inputs despite substantial increases in total input-file size. This is empirical evidence from the tested environment and workloads, not a mathematical proof of O(1) memory.

### Throughput

Small mixed inputs had lower throughput. Throughput increased and stabilized near 39–40 MiB/s for the larger mixed inputs. Noise-heavy inputs showed higher observed throughput, reaching 85.20 MiB/s at 1 GiB.

### Noise comparison

Noise-heavy inputs showed higher throughput in the observed runs, but the workloads also differ in the proportion of matching security events. This is workload behavior, not isolated causal evidence about noise percentage alone.

## 8. Correctness Checks

- 41/41 tests passed before benchmarking.
- 41/41 tests passed after benchmarking.
- Line counts remained consistent across repeated runs.
- Matched counts remained consistent across repeated runs.
- No source changes or optimization were introduced during measurement.

## 9. Limitations

- Results are dependent on the tested hardware, operating system, storage, and runtime environment.
- Filesystem and OS caching can affect timing and working-set measurements.
- Working set is not equivalent to managed heap size.
- 50 ms sampling can miss short-lived memory peaks.
- Runtime and `StreamReader` buffering contribute to process memory.
- Individual line size can affect memory usage.
- Background system activity was not artificially eliminated.
- The benchmark does not prove mathematical O(1) memory.
- Application timing excludes report serialization, while working-set sampling covers the full process lifetime.

## 10. Conclusion

Under the tested Windows environment and workloads, StreamGuard processed inputs up to approximately 1.65 GiB while observed process working-set usage remained approximately stable for the larger inputs. The results support the project’s bounded-memory design empirically, but do not constitute a formal proof of constant memory usage. Mixed-workload throughput stabilized near 40 MiB/s at larger sizes, while the noise-heavy workload produced higher observed throughput under its different event density.
