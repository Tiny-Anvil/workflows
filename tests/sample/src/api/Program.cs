var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var version = app.Configuration["APP_VERSION"] ?? "local";
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/v1/version", () => Results.Ok(new { version }));
app.MapFallbackToFile("index.html");
app.Run();

public partial class Program;

#if DEV_AUTH_BYPASS
// Stands in for a development-only auth bypass: compiled into Debug, never into Release.
internal sealed class DevBypassAuthenticationHandler;
#endif
