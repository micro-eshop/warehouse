using MediatR;

using Warehouse.Core.Commands;
using Warehouse.Core.Model;
using Warehouse.Core.Repositories;

namespace Warehouse.Core.CommandHandlers;

public class CreateWarehouseStateCommandHandler : IRequestHandler<CreateWarehouseStateCommand>
{
    private readonly IWarehouseWriter _warehouseWriter;

    public CreateWarehouseStateCommandHandler(IWarehouseWriter warehouseWriter)
    {
        _warehouseWriter = warehouseWriter;
    }

    public async Task<Unit> Handle(CreateWarehouseStateCommand request, CancellationToken cancellationToken)
    {
        var warehouseIds = Enumerable.Range(1, 10).Select(i => new WarehouseId(i));
        var stocks = CreateStocks(request.ProductId, warehouseIds);

        var result = await _warehouseWriter.Write(stocks.ToList(), cancellationToken);

        return Unit.Value;
    }


    private static IEnumerable<Stock> CreateStocks(ProductId productId, IEnumerable<WarehouseId> warehouseIds)
    {
        foreach (var id in warehouseIds)
        {
            yield return new Stock(productId, id, new StockQuantity(10, 1));
        }
    }
}