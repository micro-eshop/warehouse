using Microsoft.Extensions.Logging;

using Warehouse.Infrastructure.Nats;

namespace Warehouse.Infrastructure.Logging;

internal static partial class WarehouseNatsProductCreatedSubscriberLogger
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Warning, Message = "Failed to deserialize message: {Message}")]
    public static partial void LogFailedToDeserializeMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Product created: {Product}")]
    public static partial void LogProductsCreated(this ILogger logger, ProductCreated product);
}