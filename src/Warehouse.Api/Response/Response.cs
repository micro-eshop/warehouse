namespace Warehouse.Api.Response;

public class GetProductStockResponse
{
    public int ProductId { get; init; }
    public int WarehouseId { get; init; }
    public int AvailableQuantity { get; init; }
}