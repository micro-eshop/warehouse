namespace Warehouse.Core.Model;
using StronglyTypedIds;

[StronglyTypedId(converters: StronglyTypedIdConverter.SystemTextJson, backingType: StronglyTypedIdBackingType.Int)]
public partial struct ProductId { }

[StronglyTypedId(converters: StronglyTypedIdConverter.SystemTextJson, backingType: StronglyTypedIdBackingType.Int)]
public partial struct WarehouseId { }

public record struct StockQuantity(int WarehouseQuantity, int ReservedQuantity);

public record Stock(ProductId ProductId, WarehouseId WarehouseId, StockQuantity Quantity)
{
    public int GetAvailableQuantity()
    {
        var q = Quantity.WarehouseQuantity - Quantity.ReservedQuantity;
        return q < 0 ? 0 : q;
    }
}