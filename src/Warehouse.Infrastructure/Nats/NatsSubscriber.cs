using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Warehouse.Infrastructure.Logging;
using NATS.Client;

namespace Warehouse.Infrastructure.Nats;

internal class ProductCreated
{
    private int Id { get; init; }
    private string? Name { get; init; }
    private string? Brand { get; init; }
    private string? Description { get; init; }
    private double Price { get; init; }
    private double? PromotionPrice { get; init; }
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

    public WarehouseNatsProductCreatedSubscriber(IConnection connection, ILogger<WarehouseNatsProductCreatedSubscriber> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var chan = Channel.CreateBounded<ProductCreated>(new BoundedChannelOptions(10) { SingleReader = true, SingleWriter = true, FullMode = BoundedChannelFullMode.Wait });
        _connection.SubscribeAsync("PRODUCTS.created", (sender, args) =>
        {
            var body = args.Message.Data;
            var json = Encoding.UTF8.GetString(body);
            var product = JsonSerializer.Deserialize(json, ProductCreatedJsonContext.Default.ProductCreated);
            if (product is null)
            {
                _logger.LogFailedToDeserializeMessage(json);
                return;
            }
            chan.Writer.TryWrite(product);
        });

        await foreach (var msg in chan.Reader.ReadAllAsync(stoppingToken))
        {
            _logger.LogInformation("Product created: {@Product}", msg);
        }
    }
}