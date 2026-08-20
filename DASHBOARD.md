# StreamGuard — Dashboard Assignment

## Objective

Build a lightweight dashboard that visualizes the results of a completed StreamGuard scan.

The dashboard is a **presentation layer only**. It consumes the existing `report.json` and must not duplicate StreamGuard's parsing, telemetry, or threat-assessment logic.

```text
Log File
   │
   ▼
StreamGuard
   │
   └──► report.json
            │
            ▼
        Dashboard
```

The dashboard must be independently removable without affecting StreamGuard.

## Technology

Use:

- HTML5
- CSS3
- Vanilla JavaScript
- Chart.js for charts

Do not introduce a frontend framework, backend/API server, database or authentication.

## Repository Scope

Create:

```text
dashboard/
├── index.html
├── style.css
├── app.js
└── README.md
```

Do not modify StreamGuard source, tests, or benchmark files.

Do not change the `report.json` schema unless explicitly approved.

## Functional Requirements

### 1. Load Report

Load an existing `report.json`.

The dashboard must not read the original log, parse log lines, run StreamGuard, or perform regex matching.

Before implementation, inspect an actual generated `report.json` and use its exact schema.

### 2. Scan Summary

Display:

- File name/path
- Total lines
- Matched lines
- Execution time
- Scan timestamp

### 3. Event Distribution

Display counts for:

- Failed authentication
- Successful authentication
- Invalid-user probes
- Sudo events

Use a clear chart such as a bar or doughnut chart.

### 4. Threat Assessment

Display:

- Threat level
- Failure-to-success ratio
- Relevant event counts
- Threat explanation

The dashboard must display these values from `report.json`. It must not recalculate the ratio or threat level.

### 5. Top Usernames and Source IPs

Display the bounded top usernames and source IPs returned by StreamGuard.

Do not reconstruct or calculate global frequencies.

Include a clear notice that these values are bounded estimates and may be approximate after counter eviction.

> Top username and source-IP statistics are bounded estimates and may not represent exact global frequencies.

### 6. Error Handling

Handle:

- missing `report.json`
- invalid JSON
- missing expected fields

Show a clear user-facing error.

### 7. Refresh

Provide a simple **Reload Report** action that rereads `report.json`.

Live log monitoring is out of scope.

## UI Direction

Keep the dashboard professional and minimal.

A suitable layout is:

```text
┌──────────────────────────────────────────────────┐
│                  STREAMGUARD                     │
│              Security Scan Report               │
├──────────────────────────────────────────────────┤
│ THREAT LEVEL: HIGH       Ratio: 12.4             │
├──────────────┬──────────────┬────────────────────┤
│ Total Lines  │ Matched      │ Execution Time     │
├──────────────┴──────────────┴────────────────────┤
│              Event Distribution                  │
│                    [Chart]                       │
├─────────────────────────┬────────────────────────┤
│ Threat Assessment       │ Scan Information       │
├─────────────────────────┼────────────────────────┤
│ Top Usernames           │ Top Source IPs         │
└─────────────────────────┴────────────────────────┘

                 [Reload Report]
```

This is a design direction, not a requirement to reproduce it exactly.

Do not add unnecessary pages or features.

## Architecture Rules

The dashboard may:

- read and validate JSON
- transform values for presentation
- create charts, cards, and tables
- handle UI interaction
- display errors

The dashboard must not:

- parse logs
- use StreamGuard security regexes
- classify events
- calculate threat levels or ratios
- implement bounded counters
- create another security-analysis engine

`report.json` is the source of truth.

```text
StreamGuard = analysis
Dashboard   = visualization
```

## Out of Scope

Do not add:

- Live log monitoring
- Real-time streaming
- Authentication or user accounts
- Database
- REST API
- Backend server
- WebSockets
- ML
- Cloud deployment
- Notifications
- Historical scan storage
- Advanced filtering
- Multiple dashboard pages

If a feature appears necessary, raise it for review before expanding scope.

## Acceptance Criteria

- [ ] `report.json` loads successfully.
- [ ] Scan summary is displayed.
- [ ] All four event types are displayed.
- [ ] Event distribution is visualized.
- [ ] Threat level, ratio, and explanation are displayed.
- [ ] Top usernames are displayed.
- [ ] Top source IPs are displayed.
- [ ] Bounded/approximate telemetry is clearly identified.
- [ ] Missing report produces a clear error.
- [ ] Invalid JSON produces a clear error.
- [ ] Reload works.
- [ ] No log parsing or security-analysis logic exists in the dashboard.
- [ ] No backend or database is introduced.
- [ ] Existing `report.json` schema is respected.
- [ ] Removing `dashboard/` does not affect StreamGuard.
- [ ] `dashboard/README.md` explains how to run and use the dashboard.

## Testing

Test using a real `report.json` generated by StreamGuard.

Verify:

1. Normal report loads and displayed values match the JSON.
2. Zero/low-activity data displays correctly.
3. HIGH-threat data displays correctly.
4. Missing report produces a clear error.
5. Invalid JSON produces a clear error.
6. Reload reflects a newly generated report.

Do not modify the StreamGuard test suite for dashboard-only functionality unless specifically required.

## Dashboard README

Create `dashboard/README.md` containing only:

- Purpose
- Technology stack
- Expected `report.json`
- How to run locally
- Any browser/local-file limitation
- Brief usage instructions

Do not duplicate the main StreamGuard README.

## Git / Review

Keep the implementation isolated to `dashboard/`.

Before requesting review:

```powershell
git status --short
git diff --stat
```

Do not commit:

- `report.json`
- generated benchmark logs
- unrelated source changes
- private project MD files

Use a focused commit such as:

```text
feat: add StreamGuard dashboard
```

Do not merge the PR yourself. The project owner will review and merge it.

## Self-Review Before PR

Confirm:

- The dashboard only consumes `report.json`.
- StreamGuard remains independent of the dashboard.
- No backend/security logic was duplicated.
- The existing report schema is respected.
- Bounded top-N values are presented as approximate.
- Errors are handled clearly.
- No unnecessary infrastructure was introduced.
- The dashboard can be removed without breaking StreamGuard.
- The implementation is simple enough to explain to a mentor.

## Definition of Success

The goal is not to build a sophisticated web application.

The goal is to demonstrate that:

> StreamGuard performs the analysis and produces a structured report. The dashboard consumes that report and presents it visually without coupling itself to the scanning engine.

Keep the implementation simple, isolated, explainable, and consistent with StreamGuard's bounded-memory and separation-of-concerns philosophy.
