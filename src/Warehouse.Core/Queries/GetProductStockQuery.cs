using Warehouse.Core.Model;
using MediatR;
using LanguageExt;

namespace Warehouse.Core.Queries;

public record struct AvailableQuantity(int Quantity);

public record GetProductStockQuery(ProductId ProductId, WarehouseId WarehouseId) : IRequest<Option<AvailableQuantity>>;