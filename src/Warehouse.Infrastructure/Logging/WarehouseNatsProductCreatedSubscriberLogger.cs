using Microsoft.Extensions.Logging;

namespace Warehouse.Infrastructure.Logging;

internal static partial class WarehouseNatsProductCreatedSubscriberLogger
{
   [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to deserialize message: {Message}")]
   public static partial void LogFailedToDeserializeMessage(this ILogger logger, string message);
}