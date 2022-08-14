using System.Diagnostics;

using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

using RabbitMQ.Stream.Client.AMQP;

namespace Warehouse.Infrastructure.Rabbitmq;

internal static class RabbitmqOpenTelemetry
{
    public const string RabbitMqOpenTelemetrySourceName = $"{nameof(Warehouse)}.Rabbitmq";
    
    public readonly static ActivitySource RabbitMqSource = new(RabbitMqOpenTelemetrySourceName, "v1.0.0");
    
    public static void AddActivityToHeader(Activity activity, Annotations? props)
    {
        var context = new PropagationContext(activity.Context, Baggage.Current);
        Propagators.DefaultTextMapPropagator.Inject(context, props,(properties, key, value) =>  InjectContextIntoHeader(properties, key, value));
    }
    
    public static PropagationContext GetHeaderFromProps(Annotations? props)
    {
        var context = new PropagationContext();
        return Propagators.DefaultTextMapPropagator.Extract(context, props, (properties, s) => ExtractContextFromHeader(properties, s));
    }

    public static void InjectContextIntoHeader(Annotations? props, string key, string value)
    {
        props ??= new Annotations();
        props[key] = value;
    }
    
    public static string[] ExtractContextFromHeader(Annotations? props, string key)
    {
        if (props is null)
        {
            return Array.Empty<string>();
        }

        var value = props[key];
        if(value is string { Length: >0 } prop)
        {
            return new[] { prop };
        }
        return Array.Empty<string>();
    }
}