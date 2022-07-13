using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Warehouse.Infrastructure.Telemetry;


public class OpenTelemetryConfiguration
{
    private bool _openTelemetryEnabled = false;
    private bool _openTelemetryLoggingEnabled = false;
    private bool _openTelemetryMetricsEnabled = false;
    private bool _oltpExporterEnabled = false;
    private bool _consoleExporterEnabled = false;

    private string? _serviceVersion;
    private string? _serviceName;

    public string? ServiceName
    {
        get =>
            string.IsNullOrEmpty(_serviceName)
                ? Environment.GetEnvironmentVariable("SERVICE_NAME") ??
                  Assembly.GetExecutingAssembly().FullName 
                : _serviceName;
        set => _serviceName = value;
    }

    public string ServiceVersion
    {
        get =>
            string.IsNullOrEmpty(_serviceVersion)
                ? Environment.GetEnvironmentVariable("SERVICE_VERSION") ?? "v1.0.0"
                : _serviceVersion;
        set => _serviceVersion = value;
    }
    
    public bool OpenTelemetryEnabled
    {
        get =>
            bool.TryParse(Environment.GetEnvironmentVariable("OPENTELEMETRY_ENABLED"),
                out var openTelemetryEnabled) ? openTelemetryEnabled : _openTelemetryEnabled;
        set => _openTelemetryEnabled = value;
    }

    public bool OpenTelemetryLoggingEnabled
    {
        get => bool.TryParse(Environment.GetEnvironmentVariable("OPENTELEMETRY_LOGGING_ENABLED"), out var otle) ? otle : _openTelemetryLoggingEnabled;
        set => _openTelemetryLoggingEnabled = value;
    }

    public bool OpenTelemetryMetricsEnabled
    {
        get => bool.TryParse(Environment.GetEnvironmentVariable("OPENTELEMETRY_METRICS_ENABLED"), out var ome) ? ome : _openTelemetryMetricsEnabled;
        set => _openTelemetryMetricsEnabled = value;
    }

    public bool OltpExporterEnabled
    {
        get => bool.TryParse(Environment.GetEnvironmentVariable("OPENTELEMETRY_OLTP_EXPORTER_ENABLED"), out var ome) ? ome : _oltpExporterEnabled;
        set => _oltpExporterEnabled = value;
    }

    public bool ConsoleExporterEnabled
    {
        get => bool.TryParse(Environment.GetEnvironmentVariable("OPENTELEMETRY_CONSOLE_EXPORTER_ENABLED"), out var ome) ? ome : _consoleExporterEnabled;
        set => _consoleExporterEnabled = value;
    }
}

public static class OpenTelemetryExtensions
{
    private static ResourceBuilder GetResourceBuilder(OpenTelemetryConfiguration config) => ResourceBuilder
        .CreateDefault()
        .AddTelemetrySdk()
        .AddService(serviceName: config.ServiceName, serviceVersion: config.ServiceVersion);
    public static WebApplicationBuilder AddOpenTelemetryTracing(this WebApplicationBuilder builder,
        OpenTelemetryConfiguration? configure = null, Action<TracerProviderBuilder>? setup = null)
    {
        var config = configure ?? new OpenTelemetryConfiguration();
        if (config.OpenTelemetryEnabled)
        {
            builder.Services.AddOpenTelemetryTracing(b =>
            {
                b.AddAspNetCoreInstrumentation();
                setup?.Invoke(b);
                b.SetResourceBuilder(GetResourceBuilder(config));
                
                if (config.OltpExporterEnabled)
                {
                    b.AddOtlpExporter();
                }
            });
        }
        return builder;
    }

    public static WebApplicationBuilder AddOpenTelemetryLogging(this WebApplicationBuilder builder,
        OpenTelemetryConfiguration? configure = null, Action<OpenTelemetryLoggerOptions>? setup = null)
    {
        var config = configure ?? new OpenTelemetryConfiguration();
        if (config.OpenTelemetryLoggingEnabled)
        {
            builder.Logging
                .ClearProviders()
                .AddOpenTelemetry(b =>
            {
                b.IncludeFormattedMessage = true;
                b.IncludeScopes = true;
                b.ParseStateValues = true;
                b.SetResourceBuilder(GetResourceBuilder(config));
                setup?.Invoke(b);
                b.AddConsoleExporter(options =>
                {
                    options.Targets = ConsoleExporterOutputTargets.Console;
                });
                if (config.OltpExporterEnabled)
                {
                    b.AddOtlpExporter();
                }
            });
        }
        return builder;
    }

    public static WebApplicationBuilder AddOpenTelemetryMetrics(this WebApplicationBuilder builder,
        OpenTelemetryConfiguration? configure = null, Action<MeterProviderBuilder>? setup = null)
    {
        var config = configure ?? new OpenTelemetryConfiguration();
        if (config.OpenTelemetryMetricsEnabled)
        {
            builder.Services.AddOpenTelemetryMetrics(b =>
            {
                b.AddAspNetCoreInstrumentation();
                b.SetResourceBuilder(GetResourceBuilder(config));
                setup?.Invoke(b);
                if (config.OltpExporterEnabled)
                {
                    b.AddOtlpExporter();
                }
            });
        }
        return builder;
    }
}