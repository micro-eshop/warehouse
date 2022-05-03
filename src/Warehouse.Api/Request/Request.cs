namespace Warehouse.Api.Request;

public class GetProductStockRequest
{
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
}