using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using StackExchange.Redis;

using Warehouse.Infrastructure.Redis;

using Xunit;

namespace Warehouse.FunctionalTests.Redis;

public class RedisFixture : IAsyncLifetime, IDisposable
{
    private readonly TestcontainerDatabaseConfiguration configuration = new RedisTestcontainerConfiguration("redis:6-alpine");

    public RedisFixture()
    {
        Redis = new TestcontainersBuilder<RedisTestcontainer>()
            .WithDatabase(this.configuration)
            .Build();
    }

    public TestcontainerDatabase Redis { get; }
    public IConnectionMultiplexer RedisConnection { get; private set; }

    public async Task InitializeAsync()
    {
        await Redis.StartAsync();
        RedisConnection = await ConnectionMultiplexerProvider.CreateMultiplexer(Redis.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await RedisConnection.CloseAsync();
        await Redis.DisposeAsync();
    }

    public void Dispose()
    {
        this.configuration.Dispose();
    }
}
