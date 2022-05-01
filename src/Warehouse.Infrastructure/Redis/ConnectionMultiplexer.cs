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
        options.ConnectTimeout = 5000;
        options.ResolveDns = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var _);
        return await ConnectionMultiplexer.ConnectAsync(options);
    }
}