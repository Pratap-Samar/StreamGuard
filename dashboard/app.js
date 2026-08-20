/* Presentation only: StreamGuard's report.json remains the analysis source of truth. */
const DEFAULT_REPORT_PATH = "../report.json";
const EVENT_KEYS = ["FailedAuthentication", "SuccessfulAuthentication", "InvalidUserProbe", "SudoEscalation"];
const EVENT_LABELS = ["Failed authentication", "Successful authentication", "Invalid-user probes", "Sudo events"];
let eventChart;
let currentSource = { type: "url", value: DEFAULT_REPORT_PATH, label: DEFAULT_REPORT_PATH };

const $ = (id) => document.getElementById(id);
const number = (value) => new Intl.NumberFormat().format(value);

function required(value, name) {
  if (value === undefined || value === null) throw new Error(`The report is missing the expected “${name}” field.`);
  return value;
}

function validate(report) {
  required(report, "file"); required(report, "durationMilliseconds");
  const summary = required(report, "summary"); const threat = required(report, "threatAssessment");
  ["totalLines", "matchedLines", "eventCounts", "topUsernames", "topSourceIps"].forEach((key) => required(summary[key], `summary.${key}`));
  ["level", "failureToSuccessRatio", "failureSignals", "successfulAuthentications", "sudoEvents", "explanation"].forEach((key) => required(threat[key], `threatAssessment.${key}`));
  if (!Array.isArray(summary.topUsernames) || !Array.isArray(summary.topSourceIps)) throw new Error("The bounded telemetry fields must be arrays.");
  EVENT_KEYS.forEach((key) => required(summary.eventCounts[key], `summary.eventCounts.${key}`));
}

function escapeHtml(value) { const element = document.createElement("span"); element.textContent = String(value); return element.innerHTML; }
function renderTable(items) {
  if (!items.length) return '<p class="empty">No values were recorded.</p>';
  return `<table><thead><tr><th>Value</th><th>Count</th></tr></thead><tbody>${items.map((item) => `<tr><td>${escapeHtml(item.value)}</td><td>${number(item.count)}</td></tr>`).join("")}</tbody></table>`;
}
function showError(message) { $("error-message").textContent = message; $("error-panel").hidden = false; $("dashboard-content").hidden = true; }
function showDashboard() { $("error-panel").hidden = true; $("dashboard-content").hidden = false; }

function renderChart(counts) {
  if (!window.Chart) throw new Error("Chart.js could not be loaded. Check your internet connection and reload the page.");
  if (eventChart) eventChart.destroy();
  eventChart = new Chart($("event-chart"), {
    type: "bar",
    data: { labels: EVENT_LABELS, datasets: [{ label: "Events", data: EVENT_KEYS.map((key) => counts[key]), backgroundColor: ["#ff667a", "#43d7a7", "#f7b955", "#818cf8"], borderRadius: 6, borderSkipped: false }] },
    options: { maintainAspectRatio: false, plugins: { legend: { display: false } }, scales: { x: { ticks: { color: "#c9d8e9" }, grid: { display: false } }, y: { beginAtZero: true, ticks: { color: "#9cb0c8", precision: 0 }, grid: { color: "#203956" } } } }
  });
}

function render(report) {
  validate(report); showDashboard();
  const { summary, threatAssessment: threat } = report;
  $("threat-banner").dataset.level = String(threat.level).toLowerCase();
  $("threat-title").textContent = String(threat.level).toUpperCase();
  $("threat-ratio").textContent = String(threat.failureToSuccessRatio);
  $("total-lines").textContent = number(summary.totalLines); $("matched-lines").textContent = number(summary.matchedLines);
  $("execution-time").textContent = `${Number(report.durationMilliseconds).toFixed(2)} ms`; $("source-file").textContent = report.file;
  $("file-path").textContent = report.file; $("loaded-report").textContent = currentSource.label;
  $("failure-signals").textContent = number(threat.failureSignals); $("successful-authentications").textContent = number(threat.successfulAuthentications); $("sudo-events").textContent = number(threat.sudoEvents);
  $("event-failed").textContent = number(summary.eventCounts.FailedAuthentication);
  $("event-successful").textContent = number(summary.eventCounts.SuccessfulAuthentication);
  $("event-probes").textContent = number(summary.eventCounts.InvalidUserProbe);
  $("event-sudo").textContent = number(summary.eventCounts.SudoEscalation);
  $("scan-timestamp").textContent = report.scanTimestamp ?? "Not included in report";
  $("threat-explanation").textContent = threat.explanation;
  $("usernames-table").innerHTML = renderTable(summary.topUsernames); $("ips-table").innerHTML = renderTable(summary.topSourceIps);
  renderChart(summary.eventCounts);
}

async function loadReport() {
  try {
    let raw;
    if (currentSource.type === "file") raw = await currentSource.value.text();
    else {
      const response = await fetch(currentSource.value, { cache: "no-store" });
      if (!response.ok) throw new Error(`Could not find ${currentSource.value} (HTTP ${response.status}). Choose a report file or serve the repository root locally.`);
      raw = await response.text();
    }
    try { render(JSON.parse(raw)); } catch (error) { if (error instanceof SyntaxError) throw new Error("The selected report is not valid JSON."); throw error; }
  } catch (error) {
    const message = currentSource.type === "url" && window.location.protocol === "file:"
      ? "Your browser blocks automatic report loading when this page is opened directly from a file. Use “Choose report” to select report.json, or open the dashboard through http://localhost:8080/dashboard/."
      : (error.message || "An unexpected error occurred while loading the report.");
    showError(message);
  }
}

$("report-file").addEventListener("change", (event) => {
  const file = event.target.files[0];
  if (!file) return;
  currentSource = { type: "file", value: file, label: file.name };
  loadReport();
});
$("reload-button").addEventListener("click", loadReport);
loadReport();
