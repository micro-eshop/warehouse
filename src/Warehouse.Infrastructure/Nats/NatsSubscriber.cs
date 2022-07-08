using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warehouse.Infrastructure.Logging;
using NATS.Client;
using MediatR;
using Warehouse.Core.Commands;

namespace Warehouse.Infrastructure.Nats;

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

internal record NatsMessage(ProductCreated Msg, ActivityContext? Context);

internal class WarehouseNatsProductCreatedSubscriber : BackgroundService
{
    private readonly IConnection _connection;
    private readonly ILogger<WarehouseNatsProductCreatedSubscriber> _logger;
    private IAsyncSubscription? _subscription;
    private readonly ISender _sender;
    public const string Subject = "PRODUCTS.created";
    private readonly Channel<NatsMessage> _channel;

    public WarehouseNatsProductCreatedSubscriber(IConnection connection, ILogger<WarehouseNatsProductCreatedSubscriber> logger, ISender sender)
    {
        _connection = connection;
        _logger = logger;
        _sender = sender;
        _channel = Channel.CreateBounded<NatsMessage>(new BoundedChannelOptions(10) { SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start listening for product created events");
        var subscribe = Subscribe(_channel.Writer, _logger);
        _subscription = _connection.SubscribeAsync(Subject);
        _subscription.MessageHandler += subscribe;
        var processor = ProcessMessages(_channel.Reader, _sender, _logger, stoppingToken);
        _subscription.Start();
        await processor;
        _logger.LogInformation("Finish processing");
    }


    private static EventHandler<MsgHandlerEventArgs> Subscribe(ChannelWriter<NatsMessage> writer, ILogger logger)
    {
        return (_, args) =>
        {
            using var activity = NatsOpenTelemetry.NatsSource.StartActivity(name: "nats.consumer",
                ActivityKind.Consumer,
                NatsOpenTelemetry.GetHeaderFromProps(args.Message.Header).ActivityContext);
            if (activity is not null)
            {
                activity.SetTag("messaging.destination", args.Message.Subject);
                activity.SetTag("messaging.system", "nats");
                activity.SetTag("messaging.destination_kind", "subject");
                activity.SetTag("messaging.protocol", "nats");
            }
            var body = args.Message.Data;
            var json = Encoding.UTF8.GetString(body);
            var product = JsonSerializer.Deserialize(json, ProductCreatedJsonContext.Default.ProductCreated);
            if (product is null)
            {
                logger.LogFailedToDeserializeMessage(json);
                activity?.AddEvent(new ActivityEvent("message is null or cannot be deserialized"));
                return;
            }
            writer.TryWrite(new NatsMessage(product, activity?.Context));
        };
    }
    private static async Task ProcessMessages(ChannelReader<NatsMessage> reader, ISender sender, ILogger logger, CancellationToken cancellationToken)
    {
        await foreach (var msg in reader.ReadAllAsync(cancellationToken).WithCancellation(cancellationToken))
        {
            var links = msg.Context.HasValue ? new[] { new ActivityLink(msg.Context.Value) } : null;
            using var activity = NatsOpenTelemetry.NatsSource.StartActivity(ActivityKind.Internal,
                name: "message.consumption", links: links);
            if (activity is not null)
            {
                activity.SetTag("product.id", msg.Msg.Id);
            }
            logger.LogProductsCreated(msg.Msg);
            await sender.Send(new CreateWarehouseStateCommand(new Core.Model.ProductId(msg.Msg.Id)), cancellationToken);
        }
    }

    public override void Dispose()
    {
        if(_subscription is not null) {
            this._subscription.Unsubscribe();
        }
        this._channel.Writer.Complete();
        this._connection.Drain();
        this._connection.Close();
        base.Dispose();
    }
}