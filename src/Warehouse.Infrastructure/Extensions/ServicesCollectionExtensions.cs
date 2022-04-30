using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Warehouse.Core.Repositories;
using Warehouse.Infrastructure.Redis;
using Warehouse.Infrastructure.Repositories;

namespace Warehouse.Infrastructure.Extensions;

public static class ServicesCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(ConnectionMultiplexerProvider.CreateMultiplexer(builder.Configuration.GetConnectionString("Redis")));
        builder.Services.AddTransient<IWarehouseReader, RedisWarehouseRepository>();
        builder.Services.AddTransient<IWarehouseWriter, RedisWarehouseRepository>();
        return builder;
    }
}