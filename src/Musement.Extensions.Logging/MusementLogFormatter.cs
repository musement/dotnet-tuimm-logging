using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Musement.Extensions.Logging;

public sealed class MusementLogFormatter : ITextFormatter
{
    private readonly JsonValueFormatter _valueFormatter;

    public MusementLogFormatter() =>
        _valueFormatter = new JsonValueFormatter("type");

    public MusementLogFormatter(JsonValueFormatter valueFormatter) =>
        _valueFormatter = valueFormatter;

    public void Format(LogEvent logEvent, TextWriter output)
    {
        if (logEvent is null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        if (output is null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        FormatPlainEvent(logEvent, output);
        output.WriteLine();
    }

    private void FormatPlainEvent(LogEvent logEvent, TextWriter output)
    {
        output.Write("{\"timestamp\":\"");
        output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));
        output.Write("\",\"level\":");
        output.Write(MapLogLevel(logEvent));
        output.Write(",\"message\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Render(logEvent.Properties), output);

        if (logEvent.Properties.TryGetValue("extra", out var extra))
        {
            logEvent.RemovePropertyIfPresent("extra");
        }

        if (logEvent.Exception != null)
        {
            var ex = logEvent.Exception;
            var stacktraceValue = new ScalarValue(ex.StackTrace);
            var targetSiteValue = new ScalarValue(ex.TargetSite);
            var sourceValue = new ScalarValue(ex.Source);
            var fullNamevalue = new ScalarValue(ex.GetType().FullName);
            var exceptionMessageValue = new ScalarValue(ex.Message);

            logEvent.AddOrUpdateProperty(new LogEventProperty("Stacktrace", stacktraceValue));
            logEvent.AddOrUpdateProperty(new LogEventProperty("TargetSite", targetSiteValue));
            logEvent.AddOrUpdateProperty(new LogEventProperty("Source", sourceValue));
            logEvent.AddOrUpdateProperty(new LogEventProperty("FullName", fullNamevalue));
            logEvent.AddOrUpdateProperty(new LogEventProperty("ExceptionMessage", exceptionMessageValue));
        }

        if (logEvent.Properties.Count > 0)
        {
            output.Write(",");
            FormatContext(logEvent.Properties, output, _valueFormatter);
        }

        if (extra != null)
        {
            output.Write(",\"extra\":");
            _valueFormatter.Format(extra, output);
        }

        output.Write("}");
    }

    private static void FormatContext(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output, JsonValueFormatter valueFormatter)
    {
        output.Write("\"context\":{");
        var keys = properties.Keys.ToArray();

        for (var i = 0; i < keys.Length; i++)
        {
            var key = keys[i];
            var value = properties[key];

            output.Write($"\"{key}\":");
            valueFormatter.Format(value, output);

            if (i < keys.Length - 1)
            {
                output.Write(",");
            }
        }

        output.Write("}");
    }

    private static int MapLogLevel(LogEvent logEvent) =>
        logEvent.Level switch
        {
            LogEventLevel.Debug => 7,
            LogEventLevel.Verbose => 6,
            LogEventLevel.Information => 6,
            LogEventLevel.Warning => 4,
            LogEventLevel.Error => 3,
            LogEventLevel.Fatal when !logEvent.Properties.ContainsKey("critical") => 2,
            LogEventLevel.Fatal => 0,
            _ => 6
        };
}
