using System.Runtime.CompilerServices;

using LanguageExt;

using StackExchange.Redis;

using Warehouse.Core.Model;
using Warehouse.Core.Repositories;

using static LanguageExt.Prelude;

[assembly: InternalsVisibleTo("Warehouse.FunctionalTests")]
namespace Warehouse.Infrastructure.Repositories;

internal class RedisWarehouseRepository : IWarehouseReader, IWarehouseWriter
{
    private readonly IDatabase _database;

    public RedisWarehouseRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task<Option<Stock>> GetStock(ProductId productId, WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var stock = await _database.StringGetAsync(GetWarehouseQuantityQuantityKey(productId, warehouseId));
        if (stock.IsNullOrEmpty)
        {
            return None;
        }
        var reserved = await _database.StringGetAsync(GetReservedQuantityKey(productId, warehouseId));
        var reservedQ = reserved.IsNullOrEmpty ? 0 : (int) reserved;

        return new Stock(productId, warehouseId, new StockQuantity((int)stock, reservedQ));

    }

    private static string GetReservedQuantityKey(ProductId productId, WarehouseId warehouseId)
    {
        return $"{{reserved/{warehouseId.Value.ToString()}}}/{productId.Value.ToString()}";
    }

    private static string GetWarehouseQuantityQuantityKey(ProductId productId, WarehouseId warehouseId)
    {
        return $"{{stock/{warehouseId.Value.ToString()}}}/{productId.Value.ToString()}";
    }

    public async Task<Result<Unit>> Write(IReadOnlyCollection<Stock> stocks, CancellationToken cancellationToken)
    {
        List<Task> addTasks = new List<Task>(stocks.Count * 2);
        IBatch batch = _database.CreateBatch();

        foreach(var stock in stocks)
        {
            addTasks.Add(batch.StringSetAsync(GetWarehouseQuantityQuantityKey(stock.ProductId, stock.WarehouseId), stock.Quantity.WarehouseQuantity));
            addTasks.Add(batch.StringSetAsync(GetReservedQuantityKey(stock.ProductId, stock.WarehouseId), stock.Quantity.ReservedQuantity));
        }
        batch.Execute();
        Task[] tasks = addTasks.ToArray();
        await Task.WhenAll(tasks);

        return Result.UnitResult;
    }
}