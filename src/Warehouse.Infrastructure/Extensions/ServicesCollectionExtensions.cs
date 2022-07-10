using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NATS.Client;

using OpenTelemetry.Trace;

using StackExchange.Redis;

using Warehouse.Core.Repositories;
using Warehouse.Infrastructure.Nats;
using Warehouse.Infrastructure.Redis;
using Warehouse.Infrastructure.Repositories;

namespace Warehouse.Infrastructure.Extensions;

internal record NatsConnectionString(string Connection);
internal record RedisDatabaseNumber(int DatabaseNumber);

public static class ServicesCollectionExtensions
{
    public static async Task<WebApplicationBuilder> AddInfrastructure(this WebApplicationBuilder builder)
    {
        var multiplexer = await ConnectionMultiplexerProvider.CreateMultiplexer(builder.Configuration.GetConnectionString("Redis"));
        builder.Services.AddSingleton(multiplexer);
        builder.Services.AddSingleton(new NatsConnectionString(builder.Configuration.GetConnectionString("Nats")));
        builder.Services.AddSingleton(new RedisDatabaseNumber(int.Parse(builder.Configuration["Redis:Database"])));
        builder.Services.AddTransient<IDatabase>(sp => {
            var multiplexer = sp.GetService<IConnectionMultiplexer>();
            var databaseNumber = sp.GetService<RedisDatabaseNumber>();
            return multiplexer!.GetDatabase(databaseNumber!.DatabaseNumber);
        });
        builder.Services.AddTransient<IWarehouseReader, RedisWarehouseRepository>();
        builder.Services.AddTransient<IWarehouseWriter, RedisWarehouseRepository>();
        builder.Services.AddSingleton(new ConnectionFactory());
        builder.Services.AddTransient<IConnection>(sp => {
            var factory = sp.GetService<ConnectionFactory>();
            var connectionString = sp.GetService<NatsConnectionString>();
            return factory!.CreateConnection(connectionString!.Connection);
        });
        builder.Services.AddHostedService<WarehouseNatsProductCreatedSubscriber>();
        return builder;
    }

    public static TracerProviderBuilder AddNatsSource(this TracerProviderBuilder builder)
    {
        return builder.AddSource(NatsOpenTelemetry.NatsOpenTelemetrySourceName);
    }
}