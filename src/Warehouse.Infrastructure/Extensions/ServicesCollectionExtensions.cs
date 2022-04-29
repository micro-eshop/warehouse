using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Warehouse.Infrastructure.Redis;

namespace Warehouse.Infrastructure.Extensions;

public static class ServicesCollectionExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(ConnectionMultiplexerProvider.CreateMultiplexer(builder.Configuration.GetConnectionString("Redis")));

        return builder;
    }
}