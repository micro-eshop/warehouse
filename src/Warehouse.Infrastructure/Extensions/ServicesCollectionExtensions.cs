using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Warehouse.Core.Repositories;
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
        return builder;
    }
}