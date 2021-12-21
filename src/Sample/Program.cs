#pragma warning disable CA1812

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateDefaultBuilder()
    .UseConsoleLifetime()
    .UseMusementLogging()
    ;

var app = builder.Build();
var logger = app.Services
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("Main");

app.Start();

logger.LogDebug("Debug message");
logger.LogInformation("Info message");
logger.LogWarning("Warning message");
logger.LogError("Error message");
logger.LogCritical("Critical message");
