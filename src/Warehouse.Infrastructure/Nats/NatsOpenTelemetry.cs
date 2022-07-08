using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

using NATS.Client;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Warehouse.Infrastructure.Nats;

internal static class NatsOpenTelemetry
{
    public const string RabbitMqOpenTelemetrySourceName = $"{nameof(Warehouse)}.Rabbitmq";
    
    public readonly static ActivitySource NatsSource = new(RabbitMqOpenTelemetrySourceName, "v1.0.0");
    
    public static void AddActivityToHeader(Activity activity, MsgHeader? props)
    {
        var context = new PropagationContext(activity.Context, Baggage.Current);
        Propagators.DefaultTextMapPropagator.Inject(context, props,(properties, key, value) =>  InjectContextIntoHeader(properties, key, value));
    }
    
    public static PropagationContext GetHeaderFromProps(MsgHeader? props)
    {
        var context = new PropagationContext();
        return Propagators.DefaultTextMapPropagator.Extract(context, props, (properties, s) => ExtractContextFromHeader(properties, s));
    }

    public static void InjectContextIntoHeader(MsgHeader? props, string key, string value)
    {
        props ??= new MsgHeader();
        props[key] = value;
    }
    
    public static string[] ExtractContextFromHeader(MsgHeader? props, string key)
    {
        if (props is null)
        {
            return Array.Empty<string>();
        }

        var value = props[key];
        if(!string.IsNullOrEmpty(value))
        {
            return new[] { value };
        }
        return Array.Empty<string>();
    }
}