using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using MediatR;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using RabbitMQ.Stream.Client;

using Warehouse.Core.Commands;
using Warehouse.Infrastructure.Logging;

namespace Warehouse.Infrastructure.Rabbitmq;

internal class ProductCreated
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public double Price { get; set; }
    public double? PromotionPrice { get; set; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ProductCreated))]
internal partial class ProductCreatedJsonContext : JsonSerializerContext
{
}
internal class WarehouseRabbitMqProductCreatedSubscriber : BackgroundService
{
    public const string ProductCreatedStreamName = "product-imported";
    private readonly StreamSystem _system;
    private readonly ILogger<WarehouseRabbitMqProductCreatedSubscriber> _logger;
    private readonly IServiceProvider _serviceProvider;

    public WarehouseRabbitMqProductCreatedSubscriber(StreamSystem streamSystem, ILogger<WarehouseRabbitMqProductCreatedSubscriber> logger, IServiceProvider serviceProvider)
    {
        _system = streamSystem;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start listening for product created events");
        await _system.CreateStream(new StreamSpec(ProductCreatedStreamName) { MaxAge = TimeSpan.FromHours(24) });
        var consume = ProcessMessages(_serviceProvider);
        using var consumer = await _system.CreateConsumer(
            new ConsumerConfig
            {
                Reference = "warehouse.api",
                Stream = ProductCreatedStreamName,
                // Consume the stream from the beginning 
                // See also other OffsetSpec 
                OffsetSpec = new OffsetTypeFirst(),
                // Receive the messages
                MessageHandler = consume
            });
        _logger.LogInformation("Finish processing");
    }

    private static Func<Consumer,MessageContext,Message, Task> ProcessMessages(IServiceProvider serviceProvider)
    {
        return async (consumer, context, message) =>
        {
            using var activity = RabbitmqOpenTelemetry.RabbitMqSource.StartActivity(ActivityKind.Consumer,
                name: "message.consumption");
            if (activity is not null)
            {
                activity.SetTag("messaging.system", "rabbitmq");
                activity.SetTag("messaging.destination_kind", "subject");
                activity.SetTag("messaging.destination", ProductCreatedStreamName);
                activity.SetTag("messaging.protocol", "AMQP");
                activity.SetTag("messaging.protocol_version", "1.0.0");
            }

            var json = Encoding.UTF8.GetString(message.Data.Contents);
            var product = JsonSerializer.Deserialize(json, ProductCreatedJsonContext.Default.ProductCreated);
            await using var scope = serviceProvider.CreateAsyncScope();
            if (product is null)
            {
                var logger = scope.ServiceProvider.GetService<ILogger<WarehouseRabbitMqProductCreatedSubscriber>>();
                logger!.LogFailedToDeserializeMessage("Message is null or empty");
                activity?.AddEvent(new ActivityEvent("message is null or empty"));
                return;
            }

            activity?.SetTag("product.id", product.Id);
            var sender = scope.ServiceProvider.GetService<ISender>();
            await sender!.Send(new CreateWarehouseStateCommand(new Core.Model.ProductId(product.Id)));
            
        };
    }

    public override void Dispose()
    {
        if (!_system.IsClosed)
        {
            _system.Close();
        }
        base.Dispose();
    }
}