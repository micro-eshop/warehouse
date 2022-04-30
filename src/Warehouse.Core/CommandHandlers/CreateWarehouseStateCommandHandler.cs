using MediatR;

using Warehouse.Core.Commands;
using Warehouse.Core.Repositories;

namespace Warehouse.Core.CommandHandlers;

public class CreateWarehouseStateCommandHandler : IRequestHandler<CreateWarehouseStateCommand>
{
    private readonly IWarehouseWriter _warehouseWriter;

    public CreateWarehouseStateCommandHandler(IWarehouseWriter warehouseWriter)
    {
        _warehouseWriter = warehouseWriter;
    }

    public Task<Unit> Handle(CreateWarehouseStateCommand request, CancellationToken cancellationToken)
    {
        var ids = 
        var stock = new Stock(request.ProductId, , new StockQuantity(request.Quantity, 0));
    }
}