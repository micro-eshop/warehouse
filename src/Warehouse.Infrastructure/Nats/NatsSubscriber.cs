using System.ComponentModel;
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
    public int Id { get; init; }
    public string? Name { get; init; }
    public string? Brand { get; init; }
    public string? Description { get; init; }
    public double Price { get; init; }
    public double? PromotionPrice { get; init; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ProductCreated))]
internal partial class ProductCreatedJsonContext : JsonSerializerContext
{
}

internal class WarehouseNatsProductCreatedSubscriber : BackgroundService
{
    private readonly IConnection _connection;
    private readonly ILogger<WarehouseNatsProductCreatedSubscriber> _logger;
    private IAsyncSubscription? _subscription;
    private readonly ISender _sender;

    public WarehouseNatsProductCreatedSubscriber(IConnection connection, ILogger<WarehouseNatsProductCreatedSubscriber> logger, ISender sender)
    {
        _connection = connection;
        _logger = logger;
        _sender = sender;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var chan = Channel.CreateBounded<ProductCreated>(new BoundedChannelOptions(10) { SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });
        var subscribe = Subscribe(chan.Writer, _logger);
        _subscription = _connection.SubscribeAsync("PRODUCTS.created");
        _subscription.MessageHandler += subscribe;
        var processor = ProcessMessages(chan, _sender, _logger, stoppingToken);
        _subscription.Start();
        chan.Writer.Complete();
        await processor;
    }


    private static EventHandler<MsgHandlerEventArgs> Subscribe(ChannelWriter<ProductCreated> writer, ILogger logger)
    {
        return (sender, args) =>
        {
            var body = args.Message.Data;
            var json = Encoding.UTF8.GetString(body);
            var product = JsonSerializer.Deserialize(json, ProductCreatedJsonContext.Default.ProductCreated);
            if (product is null)
            {
                logger.LogFailedToDeserializeMessage(json);
                return;
            }
            writer.TryWrite(product);
        };
    }
    private static async Task ProcessMessages(ChannelReader<ProductCreated> reader, ISender sender, ILogger logger, CancellationToken cancellationToken)
    {
        await foreach (var msg in reader.ReadAllAsync(cancellationToken))
        {
            logger.LogProductsCreated(msg);
            await sender.Send(new CreateWarehouseStateCommand(new Core.Model.ProductId(msg.Id)), cancellationToken);
        }
    }

    public override void Dispose()
    {
        if(_subscription is not null) {
            this._subscription.Unsubscribe();
            this._subscription.Drain();
        }
        this._connection.Close();
        base.Dispose();
    }
}