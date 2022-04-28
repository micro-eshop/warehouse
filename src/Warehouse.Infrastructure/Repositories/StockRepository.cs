using LanguageExt;

using StackExchange.Redis;

using Warehouse.Core.Model;
using Warehouse.Core.Repositories;
using static LanguageExt.Prelude;

namespace Warehouse.Infrastructure.Repositories;

internal class RedisWarehouseRepository : IWarehouseReader
{
    private IDatabase _database;

    public RedisWarehouseRepository(IDatabase database)
    {
        _database = database;
    }

    public async Task<Option<Stock>> GetStock(ProductId productId, WarehouseId warehouseId)
    {
        var stock = await _database.StringGetAsync(GetWarehouseQuantityQuantityKey(productId, warehouseId));
        if (stock.IsNullOrEmpty || !stock.IsInteger)
        {
            return None;
        }
        var reserved = await _database.StringGetAsync(GetReservedQuantityKey(productId, warehouseId));
        var reservedQ = reserved.IsNullOrEmpty && reserved.IsInteger ? 0 : (int) reserved;

        return new Stock(productId, warehouseId, new StockQuantity((int)stock, reservedQ));

    }

    private static string GetReservedQuantityKey(ProductId productId, WarehouseId warehouseId)
    {
        return $"{{reserved/{warehouseId.Value}}}/{productId.Value}";
    }

    private static string GetWarehouseQuantityQuantityKey(ProductId productId, WarehouseId warehouseId)
    {
        return $"{{stock/{warehouseId.Value}}}/{productId.Value}";
    }

    public static RedisWarehouseRepository Create(ConnectionMultiplexer connection)
    {
        var database = connection.GetDatabase();
        return new RedisWarehouseRepository(database);
    }
}