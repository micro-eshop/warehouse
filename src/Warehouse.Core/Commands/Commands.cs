using MediatR;

using Warehouse.Core.Model;

namespace Warehouse.Core.Commands;

public record CreateWarehouseStateCommand(ProductId ProductId) : IRequest;