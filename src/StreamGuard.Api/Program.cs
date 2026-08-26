using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Dashboard path relative to the API project content root (which is src/StreamGuard.Api)
var dashboardPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "../../dashboard"));

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(dashboardPath),
    RequestPath = "/dashboard"
});

// Optionally redirect root to dashboard
app.MapGet("/", () => Results.Redirect("/dashboard/index.html"));
app.MapGet("/dashboard", () => Results.Redirect("/dashboard/index.html"));

app.MapGet("/api/report", async (IConfiguration config) =>
{
    var reportPath = config.GetValue<string>("ReportPath") ?? "../../report.json";
    
    // Resolve relative path to absolute
    if (!Path.IsPathRooted(reportPath))
    {
        reportPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, reportPath));
    }

    if (!File.Exists(reportPath))
    {
        return Results.NotFound(new { error = "Report file not found." });
    }

    try
    {
        var json = await File.ReadAllTextAsync(reportPath);
        return Results.Content(json, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, statusCode: 500, title: "Error reading report");
    }
});

app.Run();
