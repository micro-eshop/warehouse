using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NATS.Client;

using Warehouse.Core.Repositories;
using Warehouse.Infrastructure.Nats;
using Warehouse.Infrastructure.Redis;
using Warehouse.Infrastructure.Repositories;

namespace Warehouse.Infrastructure.Extensions;

public static class ServicesCollectionExtensions
{
    public static async Task<WebApplicationBuilder> AddInfrastructure(this WebApplicationBuilder builder)
    {
        var multiplexer = await ConnectionMultiplexerProvider.CreateMultiplexer(builder.Configuration.GetConnectionString("Redis"));
        builder.Services.AddSingleton(multiplexer);
        builder.Services.AddTransient<IWarehouseReader, RedisWarehouseRepository>();
        builder.Services.AddTransient<IWarehouseWriter, RedisWarehouseRepository>();
        builder.Services.AddSingleton(new ConnectionFactory());
        builder.Services.AddTransient<IConnection>(sp => {
            var factory = sp.GetService<ConnectionFactory>();
            return factory!.CreateConnection(builder.Configuration.GetConnectionString("Nats"));
        });
        builder.Services.AddHostedService<WarehouseNatsProductCreatedSubscriber>();
        return builder;
    }
}