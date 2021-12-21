using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Musement.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;
using System;

namespace Microsoft.Extensions.Hosting;

public static class ApplicationBuilderExtensions
{
    public static IHostBuilder UseMusementLogging(this IHostBuilder builder)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        builder.ConfigureServices(ConfigureServices);

        return builder;
    }

    // This method mimics the behavior of SerilogWebHostBuilderExtensions.UseSerilog() in Serilog.AspNetCore
    // It's not used directly to avoid unnecessary dependencies
    private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext();
        
        ConfigureSink(context, loggerConfiguration);

        var logger = loggerConfiguration.CreateLogger();
        Log.Logger = logger;

        collection.AddSingleton<ILoggerFactory>(services => new SerilogLoggerFactory());
        collection.AddSingleton(logger);
        var diagnosticContext = new DiagnosticContext(logger);
        collection.AddSingleton(diagnosticContext);
        collection.AddSingleton<IDiagnosticContext>(diagnosticContext);
    }

    private static void ConfigureSink(HostBuilderContext context, LoggerConfiguration loggerConfiguration)
    {
        var logPath = Environment.GetEnvironmentVariable("LOG_PATH");
        var isFile = !string.IsNullOrWhiteSpace(logPath);
        var isDev = context.HostingEnvironment.IsDevelopment();

        switch (isFile, isDev)
        {
            case (false, false):
                loggerConfiguration.WriteTo.Console(new MusementLogFormatter());
                break;

            case (false, true):
                loggerConfiguration.WriteTo.Console();
                break;

            case (true, false):
                loggerConfiguration.WriteTo.File(new MusementLogFormatter(), logPath);
                break;

            case (true, true):
                loggerConfiguration.WriteTo.File(logPath);
                break;
        }
    }
}