using System.Threading;
using System.Threading.Tasks;

using LanguageExt.UnsafeValueAccess;

using Warehouse.Core.Model;
using Warehouse.FunctionalTests.Redis;
using Warehouse.Infrastructure.Repositories;

using Xunit;

namespace Warehouse.FunctionalTests.Infrastructure.Repositories;

[Collection(nameof(Warehouse.FunctionalTests.Infrastructure))]
public class RedisWarehouseRepositoryTests : IClassFixture<RedisFixture> 
{
    private readonly RedisWarehouseRepository _repository;

    public RedisWarehouseRepositoryTests(RedisFixture fixture)
    {
        _repository = new RedisWarehouseRepository(fixture.RedisConnection);
    }

    [Fact]
    public async Task GetStock_WhenProductExists_ReturnsStock()
    {
        var productId = new ProductId(1);
        var warehouseId = new WarehouseId(1);
        var stock = new Stock(productId, warehouseId, new StockQuantity(10, 5));

        await _repository.Write(new[] { stock }, CancellationToken.None);

        var result = await _repository.GetStock(productId, warehouseId, CancellationToken.None);

        Assert.True(result.IsSome);
        Assert.Equal(stock, result.ValueUnsafe());
    }

    [Fact]
    public async Task GetStock_WhenProductNotExists_ReturnsNone()
    {
        var productId = new ProductId(1);
        var warehouseId = new WarehouseId(1);

        var result = await _repository.GetStock(productId, warehouseId, CancellationToken.None);

        Assert.True(result.IsNone);
    }
}