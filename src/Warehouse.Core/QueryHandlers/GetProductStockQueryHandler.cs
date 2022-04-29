using LanguageExt;

using MediatR;

using Warehouse.Core.Queries;
using Warehouse.Core.Repositories;

namespace Warehouse.Core.QueryHandlers;

public class GetProductStockQueryQueryHandler : IRequestHandler<GetProductStockQuery, Option<AvailableQuantity>>
{
    private readonly IWarehouseReader _warehouseReader;

    public GetProductStockQueryQueryHandler(IWarehouseReader warehouseReader)
    {
        _warehouseReader = warehouseReader;
    }

    public async Task<Option<AvailableQuantity>> Handle(GetProductStockQuery request, CancellationToken cancellationToken)
    {
        var result = await _warehouseReader.GetStock(request.ProductId, request.WarehouseId, cancellationToken);

        return result.Map(stock => stock.GetAvailableQuantity()).Map(availableQuantity => new AvailableQuantity(availableQuantity));
    }
}