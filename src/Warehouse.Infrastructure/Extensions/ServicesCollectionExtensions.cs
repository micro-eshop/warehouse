using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Trace;

using RabbitMQ.Stream.Client;

using StackExchange.Redis;

using Warehouse.Core.Repositories;
using Warehouse.Infrastructure.Rabbitmq;
using Warehouse.Infrastructure.Redis;
using Warehouse.Infrastructure.Repositories;

namespace Warehouse.Infrastructure.Extensions;
internal record RedisDatabaseNumber(int DatabaseNumber);

public static class ServicesCollectionExtensions
{
    public static async Task<WebApplicationBuilder> AddInfrastructure(this WebApplicationBuilder builder)
    {

        builder = await AddRabbitmq(builder);
        var multiplexer = await ConnectionMultiplexerProvider.CreateMultiplexer(builder.Configuration.GetConnectionString("Redis"));
        builder.Services.AddSingleton(multiplexer);
        builder.Services.AddSingleton(new RedisDatabaseNumber(int.Parse(builder.Configuration["Redis:Database"])));
        builder.Services.AddTransient<IDatabase>(sp => {
            var multiplexer = sp.GetService<IConnectionMultiplexer>();
            var databaseNumber = sp.GetService<RedisDatabaseNumber>();
            return multiplexer!.GetDatabase(databaseNumber!.DatabaseNumber);
        });
        builder.Services.AddTransient<IWarehouseReader, RedisWarehouseRepository>();
        builder.Services.AddTransient<IWarehouseWriter, RedisWarehouseRepository>();
        builder.Services.AddHostedService<WarehouseRabbitMqProductCreatedSubscriber>();
        return builder;
    }

    private static async Task<WebApplicationBuilder> AddRabbitmq(this WebApplicationBuilder builder)
    {
        var rabbitmqCfg = builder.Configuration.GetSection("RabbitMq").Get<RabbitmqConfig>();
        var rabbitMqUrl = new UriBuilder(rabbitmqCfg.Endpoint);
        IPAddress ipAddress = (await Dns.GetHostEntryAsync(rabbitMqUrl.Host)).AddressList.FirstOrDefault() ??
                              IPAddress.Loopback;
        var ipEndPoint = new IPEndPoint(ipAddress, 5552);
        Console.WriteLine(rabbitmqCfg.Endpoint);
        var config = new StreamSystemConfig
        {
            UserName = rabbitmqCfg.UserName,
            Password = rabbitmqCfg.Password,
            Endpoints = new List<EndPoint>() { ipEndPoint }
        };
        var system = await StreamSystem.Create(config);

        builder.Services.AddSingleton(system);

        return builder;
    }

    public static TracerProviderBuilder AddNatsSource(this TracerProviderBuilder builder)
    {
        return builder.AddSource(RabbitmqOpenTelemetry.RabbitMqOpenTelemetrySourceName);
    }
}