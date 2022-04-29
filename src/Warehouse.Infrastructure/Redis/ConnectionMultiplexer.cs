using StackExchange.Redis;

namespace Warehouse.Infrastructure.Redis;

internal static class ConnectionMultiplexerProvider
{
    public static IConnectionMultiplexer CreateMultiplexer(string redisConnection)
    {
        var options = ConfigurationOptions.Parse(redisConnection);
        options.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(options);
    }
}