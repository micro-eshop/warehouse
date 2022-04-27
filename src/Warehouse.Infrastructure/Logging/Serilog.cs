using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;


using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;

namespace Warehouse.Infrastructure.Logging;

public class SerilogOptions
{
    internal readonly static Dictionary<string, string> DefaultOverride = new() { { "Microsoft.AspNetCore", "Warning" } };
    public bool ConsoleEnabled { get; set; } = true;
    public string MinimumLevel { get; set; } = "Information";
    public string Format { get; set; } = "compact";
    public Dictionary<string, string>? Override { get; set; } = DefaultOverride;

    internal LogEventLevel GetMinimumLogEventLevel()
    {
        if (!Enum.TryParse<LogEventLevel>(MinimumLevel, true, out var level))
        {
            level = LogEventLevel.Information;
        }
        return level;
    }

    public static SerilogOptions Empty => new();
}

public class SeqOptions
{
    public bool Enabled { get; set; }
    public string? Url { get; set; }
}

public static class Extensionss
{
    private static Dictionary<string, string> BindOverride(Dictionary<string, string>? o)
    {
        if (o is null)
        {
            return SerilogOptions.DefaultOverride;
        }
        foreach (var (key, value) in SerilogOptions.DefaultOverride)
        {
            o.TryAdd(key, value);
        }
        return o;
    }

    private static LogEventLevel ParseLevel(string? level)
    {
        return Enum.TryParse<LogEventLevel>(level, true, out var lvl) ? lvl : LogEventLevel.Information;
    }

    public static IHostBuilder UseLogging(this WebApplicationBuilder builder, string applicationName)
    {
        return builder.Host.UseSerilog((context, configuration) =>
        {
            var serilogOptions = context.Configuration.GetSection("Serilog").Get<SerilogOptions>();
            var level = serilogOptions.GetMinimumLogEventLevel();
            var seq = context.Configuration.GetSection("Seq").Get<SeqOptions>();
            var conf = configuration
                .MinimumLevel.Is(level)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .Enrich.WithClientAgent()
                .Enrich.WithExceptionDetails();

            foreach ((string name, string lvl) in BindOverride(serilogOptions.Override))
            {
                conf.MinimumLevel.Override(name, ParseLevel(lvl));
            }
            if (seq is { Enabled: true, Url.Length: > 0 })
            {
                conf.WriteTo.Seq(seq.Url);
            }

            if (serilogOptions.ConsoleEnabled)
            {
                conf.WriteTo.Async((logger) =>
                {
                    switch (serilogOptions.Format.ToUpperInvariant())
                    {
                        case "ELASTICSEARCH":
                            logger.Console(new ElasticsearchJsonFormatter());
                            break;
                        case "COMPACT":
                            logger.Console(new RenderedCompactJsonFormatter());
                            break;
                        case "COLORED":
                            logger.Console(theme: AnsiConsoleTheme.Code);
                            break;
                        default:
                            logger.Console(new RenderedCompactJsonFormatter());
                            break;
                    }
                });
            }
        });
    }
}