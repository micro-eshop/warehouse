using System.Runtime.CompilerServices;
using StackExchange.Redis;

[assembly: InternalsVisibleTo("Warehouse.FunctionalTests")]
namespace Warehouse.Infrastructure.Redis;

internal static class ConnectionMultiplexerProvider
{
    public static async Task<IConnectionMultiplexer> CreateMultiplexer(string redisConnection)
    {
        var options = ConfigurationOptions.Parse(redisConnection);
        options.AbortOnConnectFail = false;
        return await ConnectionMultiplexer.ConnectAsync(options);
    }
}