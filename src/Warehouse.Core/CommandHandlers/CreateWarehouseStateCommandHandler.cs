using MediatR;

using Warehouse.Core.Commands;

namespace Warehouse.Core.CommandHandlers;

public class CreateWarehouseStateCommandHandler : IRequestHandler<CreateWarehouseStateCommand>
{
    public Task<Unit> Handle(CreateWarehouseStateCommand request, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}