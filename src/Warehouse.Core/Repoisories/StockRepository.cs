using LanguageExt;

using Warehouse.Core.Model;

namespace Warehouse.Core.Repositories;

public interface IWarehouseReader
{
    Task<Option<Stock>> GetStock(ProductId productId, WarehouseId warehouseId, CancellationToken cancellationToken);
}

public interface IWarehouseWriter
{
    Task<Result<Unit>> Write(IEnumerable<Stock> stocks, CancellationToken cancellationToken);
}