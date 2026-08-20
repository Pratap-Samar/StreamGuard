# StreamGuard Dashboard

## Purpose

This standalone presentation layer displays a completed StreamGuard `report.json`. It does not read logs, run the scanner, or perform any threat analysis.

## Technology stack

HTML5, CSS3, vanilla JavaScript, and Chart.js loaded from a CDN.

## Expected `report.json`

The dashboard consumes the existing StreamGuard report schema. Put `dashboard/` alongside the generated `report.json`, or select any generated report using **Choose report**. The report is the source of truth for every displayed value.

## Run locally

From the repository root, run a simple static server:

```powershell
python -m http.server 8080
```

Open `http://localhost:8080/dashboard/`. Generate the report at the repository root with `--output report.json`, or select a completed JSON report manually.

Most browsers block `fetch()` when opening `index.html` from the file system. The file picker may still work, but a local static server is the reliable option.

## Usage

Open the dashboard to load `../report.json`. Use **Choose report** to load another completed report. Use **Reload report** after regenerating the current report.
